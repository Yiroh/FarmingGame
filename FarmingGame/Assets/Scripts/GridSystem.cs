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

    [Header("Highlight Color")]
    [Tooltip("Color of the grid highlight square.")]
    public Color highlightColor = new Color(1f, 1f, 1f, 0.25f);

    [Header("Ghost Preview")]
    public Material ghostMaterial;
    private GameObject ghostInstance;

    [Header("Interaction Constraints")]
    [Tooltip("Max distance from player to place / delete / interact.")]
    public float maxInteractionDistance = 5f;

    [Header("Ghost Colors")]
    public Color validColor = new Color(0f, 1f, 0f, 0.5f);   // place OK
    public Color invalidColor = new Color(1f, 0f, 0f, 0.5f); // place blocked / delete

    [Header("Rotation")]
    [Tooltip("How many degrees to rotate each time you press R in place mode.")]
    public float rotationStep = 90f;
    private float currentRotationY = 0f;

    // Dictionary-based unlimited grid
    private Dictionary<Vector2Int, GridCell> grid = new Dictionary<Vector2Int, GridCell>();
    public List<GameObject> allPlaceableItems; // Assign in inspectator

    [Header("Save/Load Settings")]
    public string saveFileName = "gridSave.json";

    public static GridSystem Instance;

    private void Awake()
    {
        Instance = this;
        
        Debug.Log(Application.persistentDataPath);
    }
     private void Start()
    {
        // Load after all Awakes (including FlowerManager’s) have fired
        LoadGrid();
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
        {
            highlightInstance = Instantiate(highlightPrefab);
            ApplyHighlightColor();
        }

        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask))
        {
            Vector2Int cell = WorldToGrid(hit.point);
            Vector3 center = GridToWorld(cell);

            highlightInstance.transform.position = center;
        }
    }

    private void ApplyHighlightColor()
    {
        if (highlightInstance == null) return;

        foreach (var r in highlightInstance.GetComponentsInChildren<Renderer>())
        {
            var mat = r.material;
            if (mat.HasProperty("_BaseColor"))       // URP Lit
                mat.SetColor("_BaseColor", highlightColor);
            else if (mat.HasProperty("_Color"))      // Built-in
                mat.color = highlightColor;
        }
    }

    private void UpdateGhostPreview()
    {
        if (PlayerController.Instance == null)
            return;

        var action = PlayerController.Instance.currentAction;
        placePrefab = PlayerController.Instance.SelectedPrefab;

        // TODO: When none of placePrefab in inventory, hide ghost.

        // No ghost if no highlight yet
        if (highlightInstance == null)
        {
            if (ghostInstance != null)
                ghostInstance.SetActive(false);
            return;
        }

        // Ensure ghost exists when needed
        if (ghostInstance == null && placePrefab != null)
        {
            ghostInstance = Instantiate(placePrefab);
            ApplyGhostMaterial(ghostInstance);

            // Disable scripts so ghost doesn't run behavior
            SetActiveComponents(ghostInstance, false);
        }

        // Decide behavior based on mode
        Vector2Int cell = WorldToGrid(highlightInstance.transform.position);

        switch (action)
        {
            case GridAction.Place:
                UpdatePlaceGhost(cell);
                break;

            case GridAction.Delete:
                UpdateDeleteGhost(cell);
                break;

            default:
                if (ghostInstance != null)
                    ghostInstance.SetActive(false);
                break;
        }
    }

    private void UpdatePlaceGhost(Vector2Int cell)
    {
        if (placePrefab == null)
        {
            if (ghostInstance != null)
                ghostInstance.SetActive(false);
            return;
        }

        ghostInstance.SetActive(true);
        ghostInstance.transform.position = highlightInstance.transform.position;
        ghostInstance.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);

        bool canPlace = CanPlaceAtCell(cell);
        SetGhostColor(canPlace ? validColor : invalidColor);
    }

    private void UpdateDeleteGhost(Vector2Int cell)
    {
        // Only show delete ghost if there's something to delete and it's in range
        bool occupied = grid.ContainsKey(cell) && grid[cell].occupied;
        bool inRange = IsWithinInteractionRange(cell);

        if (!occupied || !inRange)
        {
            if (ghostInstance != null)
                ghostInstance.SetActive(false);
            return;
        }

        GameObject obj = grid[cell].objectOnCell;
        Vector3 pos = GridToWorld(cell);
        Quaternion rot = obj != null ? obj.transform.rotation : Quaternion.identity;

        // Destroy old ghost if it exists
        if (ghostInstance != null)
            Destroy(ghostInstance);

        // Instantiate the correct prefab for preview
        if (obj != null)
        {
            string prefabName = obj.name.Replace("(Clone)", "");
            GameObject prefab = GetPrefabByName(prefabName);

            if (prefab != null)
            {
                ghostInstance = Instantiate(prefab);
                ApplyGhostMaterial(ghostInstance);
                SetActiveComponents(ghostInstance, false); // disable scripts
                ghostInstance.transform.position = pos;
                ghostInstance.transform.rotation = rot;
                SetGhostColor(invalidColor);
            }
        }
    }

    private void ApplyGhostMaterial(GameObject obj)
    {
        foreach (var r in obj.GetComponentsInChildren<MeshRenderer>())
        {
            r.material = ghostMaterial;
        }
    }

    private void SetGhostColor(Color color)
    {
        if (ghostInstance == null) return;

        foreach (var r in ghostInstance.GetComponentsInChildren<MeshRenderer>())
        {
            var mat = r.material;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            else if (mat.HasProperty("_Color"))
                mat.color = color;
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
            return true; // if no player reference, skip range check (safe default)

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

        if (placePrefab == null)
        {
            Debug.Log("No placeable prefab selected.");
            return;
        }

        // Check inventory before placing
        var itemObject = placePrefab.GetComponent<ItemObject>();
        if (itemObject != null)
        {
            InventoryItem inventoryItem = InventorySystem.current.Get(itemObject.referenceItem);
            if (inventoryItem == null || inventoryItem.stackSize <= 0)
            {
                Debug.Log("Cannot place: none left in inventory.");
                return;
            }
        }

        // Passed all checks → actually place object
        Vector3 pos = GridToWorld(cell);
        Quaternion rot = Quaternion.Euler(0f, currentRotationY, 0f);
        GameObject obj = Instantiate(placePrefab, pos, rot);

        // Consume one from inventory if applicable
        if (itemObject != null)
        {
            InventorySystem.current.Remove(itemObject.referenceItem);
        }

        if (!grid.ContainsKey(cell))
            grid[cell] = new GridCell();

        grid[cell].occupied = true;
        grid[cell].objectOnCell = obj;

        SetActiveComponents(obj, true);

        if (obj.TryGetComponent<Flower>(out var flowerComp))
        {
            FlowerManager.Instance.allFlowers.Add(flowerComp);
        }

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

        GameObject obj = grid[cell].objectOnCell;

        // Add back to inventory if prefab has ItemObject
        ItemObject itemObj = obj.GetComponent<ItemObject>();
        if (itemObj != null)
        {
            InventorySystem.current.Add(itemObj.referenceItem);
        }

        // Remove flower object from its List
        if (obj.TryGetComponent<Flower>(out var flower))
            FlowerManager.Instance.allFlowers.Remove(flower);

        Destroy(obj);
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
            if (BeehiveUI.Instance != null)
            {
                BeehiveUI.Instance.Hide();
            }
                
            return;

         }
        GameObject obj = grid[cell].objectOnCell;
        var hive = obj.GetComponent<Beehive>();
        
        if (hive != null)
        {
            Debug.Log($"Selected hive: {hive.hiveName}");
            if (BeehiveUI.Instance != null)
            {
                BeehiveUI.Instance.ShowHive(hive);
            }
               
        }
        else
        {
            Debug.Log($"Selected: {obj.name}");
            if (BeehiveUI.Instance != null)
            {
                BeehiveUI.Instance.Hide();
            }
                
        }
    }

    private void Harvest(Vector2Int cell)
    {
        if (!grid.ContainsKey(cell) || !grid[cell].occupied)
        {
            Debug.Log("Nothing to harvest.");
            return;
        }

        GameObject obj = grid[cell].objectOnCell;

        // Add harvested item to inventory if prefab has ItemObject
        ItemObject itemObj = obj.GetComponent<ItemObject>();
        if (itemObj != null)
        {
            InventorySystem.current.Add(itemObj.referenceItem);
            Debug.Log($"Harvested {itemObj.referenceItem.displayName}");
        }

        if (obj.TryGetComponent<Flower>(out var flower))
            FlowerManager.Instance.allFlowers.Remove(flower);

        Destroy(obj);
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
            0.7f,
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
        public float rotY;
        
    }

    [System.Serializable]
    private class GridSaveData
    {
        public List<GridCellData> cells = new List<GridCellData>();
    }
    //TODO: MAKE IT SO SAME BEES COME FROM SAVED HIVE 
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
                prefabName = kvp.Value.objectOnCell.name.Replace("(Clone)", ""),
                rotY = kvp.Value.objectOnCell.transform.eulerAngles.y // <-- NEW
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

            // NEW: apply stored rotation (defaults to 0 if loading old saves)
            Quaternion rot = Quaternion.Euler(0f, cellData.rotY, 0f);

            GameObject obj = Instantiate(prefab, pos, rot);

            // Flower registration stays the same...
            Flower flowerComponent = obj.GetComponent<Flower>();
            if (flowerComponent != null && FlowerManager.Instance != null)
            {
                FlowerManager.Instance.allFlowers.Add(flowerComponent);
            }

            if (!grid.ContainsKey(cell))
                grid[cell] = new GridCell();

            grid[cell].occupied = true;
            grid[cell].objectOnCell = obj;
        }

        Debug.Log("Grid loaded!");
    }


    private GameObject GetPrefabByName(string name)
    {
        foreach (var prefab in allPlaceableItems)
        {
            if (prefab != null && prefab.name == name)
                return prefab;
        }

        Debug.LogWarning($"Prefab {name} not found.");
        return null;
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

        if (ghostInstance != null)
        {
            Destroy(ghostInstance);
            ghostInstance = null;
        }

        // Instantiate new ghost if a prefab is selected
        if (placePrefab != null && highlightInstance != null)
        {
            ghostInstance = Instantiate(placePrefab);
            ApplyGhostMaterial(ghostInstance);
            ghostInstance.SetActive(true);
            SetActiveComponents(ghostInstance, false);
            ghostInstance.transform.position = highlightInstance.transform.position;
        }
    }

    // Helper function for ghost objects
    private void SetActiveComponents(GameObject obj, bool active)
    {
        foreach (var comp in obj.GetComponentsInChildren<MonoBehaviour>())
        {
            comp.enabled = active;
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
