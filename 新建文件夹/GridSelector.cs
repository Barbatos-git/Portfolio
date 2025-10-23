using UnityEngine;
using UnityEngine.Tilemaps;

public class GridSelector : MonoBehaviour
{
    public static GridSelector Instance;

    public Tilemap tilemap;
    private Vector3Int currentCell;

    void Awake()
    {
        Instance = this;
    }

    public void SetToCell(Vector3Int cell)
    {
        if (!tilemap.HasTile(cell)) return;

        currentCell = cell;
        transform.position = tilemap.GetCellCenterWorld(cell);
    }

    public Vector3Int GetCurrentCell() => currentCell;
}
