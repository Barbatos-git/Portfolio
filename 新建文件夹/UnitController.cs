using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(SpriteRenderer))]
public class UnitController : MonoBehaviour
{
    private Tilemap tilemap;
    public float speed = 3f;

    public int baseOrder = 1000;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (tilemap == null)
        {
            var obj = GameObject.Find("GroundTilemap");
            if (obj != null)
                tilemap = obj.GetComponent<Tilemap>();
        }
    }

    void Start()
    {
        SnapToGrid(transform.position);
    }

    void LateUpdate()
    {
        // Yが低いほど → sortingOrder が大きくなります（前に近いほど）
        int order = baseOrder - Mathf.RoundToInt(transform.position.y * 100);
        sr.sortingOrder = Mathf.Max(order, 100);
    }

    public void SnapToGrid(Vector3 worldPos)
    {
        // 指定されたワールド座標から、対応するタイルのセル位置を取得
        Vector3Int cell = tilemap.WorldToCell(worldPos);

        SnapToGrid(cell);
    }

    public void SnapToGrid(Vector3Int cell)
    {
        // 該当セルの中心のワールド座標を取得
        Vector3 gridCenter = tilemap.GetCellCenterWorld(cell);

        // ピボット位置（足元）をセルの中心に配置（そのまま吸着）
        transform.position = gridCenter;
    }

    public Vector3Int GetCurrentCell()
    {
        return tilemap.WorldToCell(transform.position);
    }

    public IEnumerator MoveAlongPath(List<Vector3Int> path)
    {
        var charaAni = GetComponent<CharaAnimator>();
        if (charaAni != null)
        {
            charaAni.MoveAlongPath(path, tilemap);
            yield return new WaitForSeconds(0.2f * path.Count);
        }
        else
        {
            yield return StartCoroutine(MoveStepByStep(path)); // fallback
        }
    }

    private IEnumerator MoveStepByStep(List<Vector3Int> path)
    {
        foreach (var cell in path)
        {
            Vector3 targetPos = tilemap.GetCellCenterWorld(cell);
            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * speed);
                yield return null;
            }
        }
    }
}
