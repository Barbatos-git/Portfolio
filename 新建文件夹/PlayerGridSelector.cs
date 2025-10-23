using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerGridSelector : MonoBehaviour
{
    public Tilemap tilemap;
    public Transform selector; 
    public float moveCooldown = 0.2f;

    private Vector3Int currentCell;
    private float cooldown = 0f;

    private PlayerInputConfig inputConfig;

    public void Initialize(PlayerInputConfig input, bool useDefaultStartPosition = true)
    {
        inputConfig = input;
        if (useDefaultStartPosition)
        {
            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos))
                {
                    currentCell = pos;
                    selector.position = tilemap.GetCellCenterWorld(currentCell);
                    return;
                }
            }
        }
        else
        {
            currentCell = tilemap.WorldToCell(selector.position);
        }
    }

    void Update()
    {
        if (inputConfig == null) return;

        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;

        Vector2 move = inputConfig.GetMoveInput();
        Vector3Int next = currentCell;

        if (move.x > 0.5f) next += Vector3Int.right;
        else if (move.x < -0.5f) next += Vector3Int.left;
        else if (move.y > 0.5f) next += Vector3Int.up;
        else if (move.y < -0.5f) next += Vector3Int.down;
        else return;

        if (tilemap.HasTile(next))
        {
            currentCell = next;
            selector.position = tilemap.GetCellCenterWorld(currentCell);
            cooldown = moveCooldown;
        }
    }

    public Vector3Int GetCurrentCell() => currentCell;
}
