using System.IO;
using UnityEngine;

public class GridSaveManager : MonoBehaviour
{
    public static GridSaveManager Instance;

    public string fileName = "gridSave.json";
    private string FullPath => Path.Combine(Application.persistentDataPath, fileName);

    private void Awake()
    {
        Instance = this;
    }

    // ---------------- SAVE ----------------
    public void Save(GridSystem gridSystem)
    {
        // TODO: MAKE IT SO SAME BEES COME FROM SAVED HIVE

        // TODO: Save Inventory

        // TODO: Save player position

        // TODO: Possibly add multiple save slots (Further down the line feature really)

        GridSaveData saveData = new GridSaveData();

        foreach (var kvp in gridSystem.grid)
        {
            if (!kvp.Value.occupied) continue;

            GridCellData cell = new GridCellData
            {
                x = kvp.Key.x,
                y = kvp.Key.y,
                prefabName = kvp.Value.objectOnCell.name.Replace("(Clone)", ""),
                rotY = kvp.Value.objectOnCell.transform.eulerAngles.y
            };

            saveData.cells.Add(cell);
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(FullPath, json);

        Debug.Log("Grid saved!");
    }

    // ---------------- LOAD ----------------
    public void Load(GridSystem gridSystem)
    {
        if (!File.Exists(FullPath))
        {
            Debug.LogWarning("Grid save file not found.");
            return;
        }

        string json = File.ReadAllText(FullPath);
        GridSaveData data = JsonUtility.FromJson<GridSaveData>(json);

        foreach (var cell in data.cells)
        {
            Vector2Int key = new Vector2Int(cell.x, cell.y);

            GameObject prefab = gridSystem.GetPrefabByName(cell.prefabName);
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab not found for: {cell.prefabName}");
                continue;
            }

            Vector3 pos = gridSystem.GridToWorld(key);
            Quaternion rot = Quaternion.Euler(0f, cell.rotY, 0f);

            GameObject obj = Instantiate(prefab, pos, rot);

            // Register flowers
            if (obj.TryGetComponent(out Flower flower))
                FlowerManager.Instance.allFlowers.Add(flower);

            // Create cell in dictionary
            gridSystem.grid[key] = new GridCell
            {
                occupied = true,
                objectOnCell = obj
            };
        }

        Debug.Log("Grid loaded!");
    }
}
