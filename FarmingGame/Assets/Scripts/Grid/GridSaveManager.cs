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

    public void Save(GridSystem gridSystem)
    {
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

            // --- NEW: save hive data if this cell has a hive ---
            Beehive hive = kvp.Value.objectOnCell.GetComponent<Beehive>();
            if (hive != null)
            {
                cell.hiveData = hive.CreateSaveData();
            }

            saveData.cells.Add(cell);
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(FullPath, json);
        Debug.Log("Grid saved!");
    }
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

            // Restore hive data if this is a hive
            Beehive hive = obj.GetComponent<Beehive>();
            if (hive != null && cell.hiveData != null)
            {
                hive.LoadFromSave(cell.hiveData);
            }

            // Existing flower code
            if (obj.TryGetComponent(out Flower flower))
                FlowerManager.Instance.allFlowers.Add(flower);

            gridSystem.grid[key] = new GridCell
            {
                occupied = true,
                objectOnCell = obj
            };
        }

        Debug.Log("Grid loaded!");
    }


}
