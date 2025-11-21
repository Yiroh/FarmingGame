using System.Collections.Generic;

[System.Serializable]
public class GridCellData
{
    public int x;
    public int y;
    public string prefabName;
    public float rotY;
}

[System.Serializable]
public class GridSaveData
{
    public List<GridCellData> cells = new List<GridCellData>();
}
