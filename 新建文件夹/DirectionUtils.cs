using UnityEngine;
using UnityEngine.Tilemaps;

public static class DirectionUtils
{
    private static readonly int[] isometricFlipMap = {
        0, // →
        7, // ↗
        2, // ↓
        5, // ↖
        4, // ←
        3, // ↙
        6, // ↑
        1  // ↘
    };

    public static int GetDirection8(Vector3 fromWorld, Vector3 toWorld)
    {
        Vector2 dir = toWorld - fromWorld;
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        // 0～360度の角度マッピング
        angle = (angle + 360f) % 360f;

        // 360°を8方向に分割し、各方向を45°ずつにする
        // 0 = → (0°)、1 = ↘ (45°)、2 = ↓ (90°)...
        int rawDir = Mathf.RoundToInt(angle / 45f) % 8;

        return isometricFlipMap[rawDir];
    }

    public static int GetDirection8(Tilemap tilemap, Vector3Int fromCell, Vector3Int toCell)
    {
        Vector3 from = tilemap.GetCellCenterWorld(fromCell);
        Vector3 to = tilemap.GetCellCenterWorld(toCell);
        return GetDirection8(from, to);
    }

    //public static Vector3Int GetDirectionOffset(int direction)
    //{
    //    switch (direction)
    //    {
    //        case 0: return new Vector3Int(1, 0, 0);   // →
    //        case 1: return new Vector3Int(1, -1, 0);  // ↘
    //        case 2: return new Vector3Int(0, -1, 0);  // ↓
    //        case 3: return new Vector3Int(-1, -1, 0); // ↙
    //        case 4: return new Vector3Int(-1, 0, 0);  // ←
    //        case 5: return new Vector3Int(-1, 1, 0);  // ↖
    //        case 6: return new Vector3Int(0, 1, 0);   // ↑
    //        case 7: return new Vector3Int(1, 1, 0);   // ↗
    //        default: return Vector3Int.zero;
    //    }
    //}
}