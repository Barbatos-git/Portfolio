using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RangeVisualizer : MonoBehaviour
{
    public static RangeVisualizer Instance;

    public Tilemap rangeTilemap;        // RangeTilemap
    public Tilemap waterTilemap;        // WaterTilemap
    public Tilemap patternTilemap;       // PatternTilemap
    public Tilemap baseTilemap;          // GroundTilemap
    public Tile moveTile;
    public Tile attackTile;
    public Tile blockedTile;
    public Tile waterTile;

    private List<Vector3Int> currentTiles = new();

    public GameObject arrowPrefab;
    public Transform arrowContainer;
    public LineRenderer lineRenderer;
    private GameObject currentArrow;

    void Awake() => Instance = this;

    public void ShowRange(Vector3Int center, int range, bool isMove)
    {
        Clear();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) > range) continue;

                Vector3Int pos = center + new Vector3Int(x, y, 0);
                if (!baseTilemap.HasTile(pos)) continue;

                rangeTilemap.SetTileFlags(pos, TileFlags.None);
                rangeTilemap.SetTile(pos, isMove ? moveTile : attackTile);
                Debug.Log($"Set tile at {pos} as {(isMove ? "move" : "attack")} range");
                currentTiles.Add(pos);
            }
        }
    }

    public void ShowCells(IEnumerable<Vector3Int> cells, Tile tileToUse)
    {
        foreach (var pos in cells)
        {
            if (!baseTilemap.HasTile(pos)) continue;

            rangeTilemap.SetTileFlags(pos, TileFlags.None);
            rangeTilemap.SetTile(pos, tileToUse);
            currentTiles.Add(pos);
        }
    }

    public void ShowPatternCells(Vector3Int center, Vector3Int[] offsets, Tile tile)
    {
        foreach (var offset in offsets)
        {
            Vector3Int pos = center + offset;
            if (!baseTilemap.HasTile(pos)) continue;

            patternTilemap.SetTileFlags(pos, TileFlags.None);
            patternTilemap.SetTile(pos, tile);
            currentTiles.Add(pos);
        }
    }

    public void ShowWaterCells(IEnumerable<Vector3Int> cells, Tile tileToUse)
    {
        foreach (var pos in cells)
        {
            if (!baseTilemap.HasTile(pos)) continue;

            waterTilemap.SetTileFlags(pos, TileFlags.None);
            waterTilemap.SetTile(pos, tileToUse);
            currentTiles.Add(pos);
        }
    }


    public void Clear()
    {
        foreach (var pos in currentTiles)
        {
            rangeTilemap.SetTile(pos, null);
            waterTilemap.SetTile(pos, null);
            patternTilemap.SetTile(pos, null);
        }
        currentTiles.Clear();
    }

    // パスの矢印表示
    public void ShowPath(List<Vector3Int> path)
    {
        ClearPath();

        if (path == null || path.Count <= 1) return;

        if (path.Count == 2 && path[0] == path[1])
            return;

        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 pos = GridSelector.Instance.tilemap.GetCellCenterWorld(path[i]);
            lineRenderer.SetPosition(i, pos);
        }

        Vector3 from, to;

        if (path.Count == 1)
        {
            Vector3Int unitCell = path[0];
            Vector3Int targetCell = GridSelector.Instance.GetCurrentCell();

            Vector3 unitWorld = GridSelector.Instance.tilemap.GetCellCenterWorld(unitCell);
            Vector3 targetWorld = GridSelector.Instance.tilemap.GetCellCenterWorld(targetCell);

            from = unitWorld;
            to = targetWorld;
        }
        else
        {
            from = GridSelector.Instance.tilemap.GetCellCenterWorld(path[path.Count - 2]);
            to = GridSelector.Instance.tilemap.GetCellCenterWorld(path[path.Count - 1]);
        }

        Vector3 dir = (to - from).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        currentArrow = Instantiate(arrowPrefab, to, Quaternion.Euler(0, 0, angle), arrowContainer);
    }

    public void ClearPath()
    {
        lineRenderer.positionCount = 0;

        if (currentArrow != null)
            Destroy(currentArrow);
    }
}
