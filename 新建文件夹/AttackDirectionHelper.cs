using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

public static class AttackDirectionHelper
{
    // 8方向の角度インデックスを取得する（使用 DirectionUtils）
    public static int GetDirectionIndex(Vector3Int fromCell, Vector3Int toCell, Tilemap tilemap)
    {
        return DirectionUtils.GetDirection8(tilemap, fromCell, toCell); // → 0 ~ 7
    }

    // オフセット配列を対応する方向に回転します（4方向のみサポート：0,2,4,6）
    public static Vector3Int[] RotateOffsets(Vector3Int[] baseOffsets, int dir)
    {
        Vector3Int[] result = new Vector3Int[baseOffsets.Length];
        for (int i = 0; i < baseOffsets.Length; i++)
        {
            result[i] = RotateOffset(baseOffsets[i], dir);
        }
        return result;
    }

    // 単一のオフセットグリッドを回転します
    public static Vector3Int RotateOffset(Vector3Int offset, int dir)
    {
        switch (dir)
        {
            case 1:
            case 5:
                return offset;

            case 3:
            case 7:
                return new Vector3Int(0, offset.x, 0);

            case 2:
                return new Vector3Int(offset.x, -offset.x, 0);
            case 4:
                return new Vector3Int(-offset.x, -offset.x, 0);
            case 6:
                return new Vector3Int(-offset.x, offset.x, 0);
            case 0:
                return new Vector3Int(offset.x, offset.x, 0); 

            default:
                return offset;
        }
    }

    public static int GetFlippedScale(int direction)
    {
        if (direction == 2)
            return 6;
        else if (direction == 6)
            return 2;
        else
            return direction;
    }
}
