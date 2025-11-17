using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridSystem : MonoBehaviour
{
    [Header("Grid Settings")]
    public float cellSize = 1f;

    [Header("Interactable Object")]
    [Tooltip("Currently selected prefab to place. Your building UI can change this via SetPlacePrefab().")]
    public GameObject placePrefab;
    public LayerMask groundMask;

    [Header("Highlight")]
    public GameObject highlightPrefab;
    private GameObject highlightInstance;

    [Header("Highlight Colors")]
    [Tooltip("Normal highlight color.")]
    public Color highlightNormalColor = new Color(1f, 1f, 1f, 0.25f);
    [Tooltip("Highlight color when in delete mode and you can delete here.")]
    public Color highlightDeleteColor = new Color(1f, 0f, 0f, 0.4f);

    [Header("Ghost Preview (Place Mode)")]
    public Material ghostMaterial;
    private GameObject ghostInstance;

    [Header("Interaction Constraints")]
    [Tooltip("Max distance from player to place / delete / interact.")]
    public float maxInteractionDistance = 5f;

    [Header("Ghost Colors (Place Mode)")]
    public Color validColor = new Color(0f, 1f, 0f, 0.5f);
    public Color invalidColor = new Color(1f, 0f, 0f, 0.5f);

    [Header("Rotation")]
    [Tooltip("How many degrees to rotate each time you press R in place mode.")]
    public float rotationStep = 90f;
    private float currentRotationY = 0f;

    // Dictionary-based unlimited grid
    private Dictionary<Vector2Int, GridCell> grid = new Dictionary<Vector2Int, GridCell>();

    [Header("Save/Load Settings")]
    public string saveFileName = "gridSave.json";

    private void Awake()
    {
        LoadGrid();
        Debug.Log(Application.persistentDataPath);
    }

    private void Update()
    {
        HandleRotationInput();
        UpdateMouseHighlight();
        UpdateGhostPreview();
    }

    #region Rotation

    private void HandleRotationInput()
    {
        if (Keyboard.current == null) return;
        if (PlayerController.Instance == null) return;

        // Only rotate while in Place mode
        if (PlayerController.Instance.currentAction != GridAction.Place)
            return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            currentRotationY += rotationStep;
            currentRotationY %= 360f;

            if (ghostInstance != null)
            {
                ghostInstance.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
            }
        }
    }

    #endregion

    #region Highlight & Ghost

    private void UpdateMouseHighlight()
    {
        if (highlightPrefab == null) return;

        if (highlightInstance == null)
            highlightInstance = Instantiate(highlightPrefab);

        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask))
        {
            Vector2Int cell = WorldToGrid(hit.point);
            Vector3 center = GridToWorld(cell);

            highlightInstance.transform.position = center;

            // Update highlight color based on mode + cell
            UpdateHighlightVisual(cell);
        }
    }

    private void UpdateHighlightVisual(Vector2Int cell)
    {
        if (highlightInstance == null) return;

        Color target = highlightNormalColor;

        // In delete mode, show red if this tile is occupied and within range
        if (PlayerController.Instance != null &&
            PlayerController.Instance.currentAction == GridAction.Delete)
        {
            bool occupied = grid.ContainsKey(cell) && grid[cell].occupied;
            bool inRange = IsWithinInteractionRange(cell);

            if (occupied && inRange)
            {
                target = highlightDeleteColor;
            }
        }

        // Apply color to ANY renderer on the interaction box
        foreach (var r in highlightInstance.GetComponentsInChildren<Renderer>())
        {
            if (r.material.HasProperty("_Color"))
            {
                r.material.color = target;
            }
        }
    }

    private void UpdateGhostPreview()
    {
        if (PlayerController.Instance == null)
            return;

        // Ghost is only for PLACE mode. Delete mode uses the red highlight as preview.
        if (PlayerController.Instance.currentAction != GridAction.Place)
        {
            if (ghostInstance != null)
                ghostInstance.SetActive(false);
            return;
        }

        if (placePrefab == null)
        {
            if (ghostInstance != null)
                ghostInstance.SetActive(false);
            return;
        }

        if (ghostInstance == null)
        {
            ghostInstance = Instantiate(placePrefab);
            ApplyGhostMaterial(ghostInstance);
        }

        ghostInstance.SetActive(true);

        if (highlightInstance != null)
        {
            ghostInstance.transform.position = highlightInstance.transform.position;
            ghostInstance.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);

            Vector2Int cell = WorldToGrid(highlightInstance.transform.position);
            bool canPlace = CanPlaceAtCell(cell);
            SetGhostColor(canPlace);
        }
    }

    private void ApplyGhostMaterial(GameObject obj)
    {
        foreach (var r in obj.GetComponentsInChildren<MeshRenderer>())
        {
            r.material = ghostMaterial;
        }
    }

    private void SetGhostColor(bool canPlace)
    {
        if (ghostInstance == null) return;

        Color targetColor = canPlace ? validColor : invalidColor;

        foreach (var r in ghostInstance.GetComponentsInChildren<MeshRenderer>())
        {
            if (r.material.HasProperty("_Color"))
                r.material.color = targetColor;
        }
    }

    #endregion

    #region Interaction Methods

    public void TryInteractWithHighlighted(GridAction action)
    {
        if (action == GridAction.None) return;
        if (highlightInstance == null) return;

        Vector2Int cell = WorldToGrid(highlightInstance.transform.position);

        switch (action)
        {
            case GridAction.Place:   Place(cell);   break;
            case GridAction.Delete:  Delete(cell);  break;
            case GridAction.Select:  Select(cell);  break;
            case GridAction.Harvest: Harvest(cell); break;
        }
    }

    private bool IsWithinInteractionRange(Vector2Int cell)
    {
        if (PlayerController.Instance == null)
            return true; // if no player reference, skip range check

        Vector3 targetWorldPos = GridToWorld(cell);
        Vector3 playerPos = PlayerController.Instance.transform.position;

        Vector3 flatPlayer = new Vector3(playerPos.x, 0f, playerPos.z);
        Vector3 flatTarget = new Vector3(targetWorldPos.x, 0f, targetWorldPos.z);

        float distance = Vector3.Distance(flatPlayer, flatTarget);
        return distance <= maxInteractionDistance;
    }

    private bool CanPlaceAtCell(Vector2Int cell)
    {
        if (placePrefab == null)
            return false;

        if (!IsWithinInteractionRange(cell))
            return false;

        if (grid.ContainsKey(cell) && grid[cell].occupied)
            return false;

        return true;
    }

    private void Place(Vector2Int cell)
    {
        if (!CanPlaceAtCell(cell))
        {
            Debug.Log("Cannot place here.");
            return;
        }

        Vector3 pos = GridToWorld(cell);
        Quaternion rot = Quaternion.Euler(0f, currentRotationY, 0f);
        GameObject obj = Instantiate(placePrefab, pos, rot);

        if (!grid.ContainsKey(cell))
            grid[cell] = new GridCell();

        grid[cell].occupied = true;
        grid[cell].objectOnCell = obj;

        Debug.Log($"Placed object at {cell}");
        SaveGrid();
    }

    private void Delete(Vector2Int cell)
    {
        if (!IsWithinInteractionRange(cell))
        {
            Debug.Log("Too far to delete.");
            return;
        }

        if (!grid.ContainsKey(cell) || !grid[cell].occupied)
        {
            Debug.Log("Nothing to delete.");
            return;
        }

        Destroy(grid[cell].objectOnCell);
        grid[cell].occupied = false;
        grid[cell].objectOnCell = null;

        Debug.Log($"Deleted object at {cell}");
        SaveGrid();
    }

    private void Select(Vector2Int cell)
    {
        if (!grid.ContainsKey(cell) || !grid[cell].occupied)
        {
            Debug.Log("Selected empty tile.");
            return;
        }

        GameObject obj = grid[cell].objectOnCell;
        var hive = obj.GetComponent<Beehive>();

        if (hive != null)
        {
            Debug.Log($"Selected hive: {hive.hiveName} | Health: {hive.hiveHealth} | Bees: {hive.TotalBees} | Queen: {hive.hasQueen}");
        }
        else
        {
            Debug.Log($"Selected: {obj.name}");
        }
    }

    private void Harvest(Vector2Int cell)
    {
        if (!grid.ContainsKey(cell) || !grid[cell].occupied)
        {
            Debug.Log("Nothing to harvest.");
            return;
        }

        Debug.Log($"Harvested {grid[cell].objectOnCell.name}");
        Destroy(grid[cell].objectOnCell);

        grid[cell].occupied = false;
        grid[cell].objectOnCell = null;

        SaveGrid();
    }

    #endregion

    #region Grid Conversion

    public Vector2Int WorldToGrid(Vector3 world)
    {
        int x = Mathf.FloorToInt(world.x / cellSize);
        int y = Mathf.FloorToInt(world.z / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int cell)
    {
        return new Vector3(
            cell.x * cellSize + cellSize / 2f,
            0.01f,
            cell.y * cellSize + cellSize / 2f
        );
    }

    #endregion

    #region Save & Load

    [System.Serializable]
    private class GridCellData
    {
        public int x;
        public int y;
        public string prefabName;
        // (Rotation not saved yet; can be added later.)
    }

    [System.Serializable]
    private class GridSaveData
    {
        public List<GridCellData> cells = new List<GridCellData>();
    }

    public void SaveGrid()
    {
        GridSaveData saveData = new GridSaveData();

        foreach (var kvp in grid)
        {
            if (!kvp.Value.occupied) continue;

            GridCellData cellData = new GridCellData
            {
                x = kvp.Key.x,
                y = kvp.Key.y,
                prefabName = kvp.Value.objectOnCell.name.Replace("(Clone)", "")
            };
            saveData.cells.Add(cellData);
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, saveFileName), json);

        Debug.Log("Grid saved!");
    }

    public void LoadGrid()
    {
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning("Save file not found.");
            return;
        }

        string json = File.ReadAllText(path);
        GridSaveData saveData = JsonUtility.FromJson<GridSaveData>(json);

        foreach (var cellData in saveData.cells)
        {
            Vector2Int cell = new Vector2Int(cellData.x, cellData.y);
            GameObject prefab = GetPrefabByName(cellData.prefabName);
            if (prefab == null) continue;

            Vector3 pos = GridToWorld(cell);
            GameObject obj = Instantiate(prefab, pos, Quaternion.identity);

            if (!grid.ContainsKey(cell))
                grid[cell] = new GridCell();

            grid[cell].occupied = true;
            grid[cell].objectOnCell = obj;
        }

        Debug.Log("Grid loaded!");
    }

    private GameObject GetPrefabByName(string name)
    {
        if (placePrefab != null && placePrefab.name == name)
            return placePrefab;

        Debug.LogWarning($"Prefab {name} not found. Using default placePrefab.");
        return placePrefab;
    }

    #endregion

    #region Building Selection API

    /// <summary>
    /// Called by your future building/radial UI to change which building is being placed.
    /// For example: gridSystem.SetPlacePrefab(beehivePrefab);
    /// </summary>
    public void SetPlacePrefab(GameObject newPrefab)
    {
        placePrefab = newPrefab;

        // Force the ghost to rebuild with the new prefab
        if (ghostInstance != null)
        {
            Destroy(ghostInstance);
            ghostInstance = null;
        }
    }

    #endregion
}

[System.Serializable]
public class GridCell
{
    public bool occupied;
    public GameObject objectOnCell;
}

public enum GridAction
{
    None,   // No interaction
    Place,
    Delete,
    Select,
    Harvest
}
