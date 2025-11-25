using System.Collections.Generic;

[System.Serializable]
public class GridCellData
{
    public int x;
    public int y;
    public string prefabName;
    public float rotY;

    // Optional: only used when this cell holds a Beehive
    public HiveSaveData hiveData;
}

[System.Serializable]
public class GridSaveData
{
    public List<GridCellData> cells = new List<GridCellData>();
}

// ------------------ HIVE SAVE STRUCTS ------------------

[System.Serializable]
public class HiveBeeSaveData
{
    public string beeName;
    public string beePrefabName; // name of the bee prefab used
}

[System.Serializable]
public class HiveSaveData
{
    public string hiveName;
    public HiveType hiveType;
    public float hiveHealth;
    public bool hasQueen;
    public float queenAgeDays;
    public float queenProductivity;

    public int workerBees;
    public int droneBees;
    public int broodBees;

    public float storedHoney;
    public float storedPollen;
    public float storedWax;
    public float storedPropolis;

    public HiveBeeSaveData[] workerRoster;
}
