using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridSystem : MonoBehaviour
{
    [Header("Grid Settings")]
    public float cellSize = 1f;

    [Header("Interactable Object")]
    public GameObject placePrefab;
    public LayerMask groundMask;

    [Header("Highlight")]
    public GameObject highlightPrefab;
    private GameObject highlightInstance;

    [Header("Ghost Preview")]
    public Material ghostMaterial;
    private GameObject ghostInstance;

    [Header("Placement Constraints")]
    public float maxPlacementDistance = 5f;

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
        UpdateMouseHighlight();
        UpdateGhostPreview();
    }

    #region Highlight and Ghost

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

            if (PlayerController.Instance.currentAction == GridAction.Place && ghostInstance != null)
                ghostInstance.transform.position = center;
        }
    }

    private void UpdateGhostPreview()
    {
        if (PlayerController.Instance.currentAction != GridAction.Place)
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
            ghostInstance.transform.position = highlightInstance.transform.position;
    }

    private void ApplyGhostMaterial(GameObject obj)
    {
        foreach (var r in obj.GetComponentsInChildren<MeshRenderer>())
            r.material = ghostMaterial;
    }

    #endregion

    #region Interaction Methods

    public void TryInteractWithHighlighted(GridAction action)
    {
        if (highlightInstance == null) return;

        Vector2Int cell = WorldToGrid(highlightInstance.transform.position);

        switch (action)
        {
            case GridAction.Place: Place(cell); break;
            case GridAction.Delete: Delete(cell); break;
            case GridAction.Select: Select(cell); break;
            case GridAction.Harvest: Harvest(cell); break;
        }
    }

    private void Place(Vector2Int cell)
    {
        if (grid.ContainsKey(cell) && grid[cell].occupied)
    {
        Debug.Log("Cell occupied — cannot place.");
        return;
    }

    // --- NEW: distance check from player ---
    if (PlayerController.Instance != null)
    {
        Vector3 targetWorldPos = GridToWorld(cell);

        // Flatten to XZ so height doesn't matter
        Vector3 playerPos = PlayerController.Instance.transform.position;
        Vector3 flatPlayer = new Vector3(playerPos.x, 0f, playerPos.z);
        Vector3 flatTarget = new Vector3(targetWorldPos.x, 0f, targetWorldPos.z);

        float distance = Vector3.Distance(flatPlayer, flatTarget);

        if (distance > maxPlacementDistance)
        {
            Debug.Log("Too far from player to place here.");
            return;
        }
    }
    // --------------------------------------

    Vector3 pos = GridToWorld(cell);
    GameObject obj = Instantiate(placePrefab, pos, Quaternion.identity);

    if (!grid.ContainsKey(cell))
        grid[cell] = new GridCell();

    grid[cell].occupied = true;
    grid[cell].objectOnCell = obj;

    Debug.Log($"Placed object at {cell}");
    SaveGrid();
    }

    private void Delete(Vector2Int cell)
    {
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

        Debug.Log($"Selected: {grid[cell].objectOnCell.name}");
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
        return new Vector3(cell.x * cellSize + cellSize / 2f, 0.01f, cell.y * cellSize + cellSize / 2f);
    }

    #endregion

    #region Save & Load

    [System.Serializable]
    private class GridCellData
    {
        public int x;
        public int y;
        public string prefabName;
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
        if (placePrefab.name == name)
            return placePrefab;

        Debug.LogWarning($"Prefab {name} not found. Using default placePrefab.");
        return placePrefab;
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
    Place,
    Delete,
    Select,
    Harvest
}
