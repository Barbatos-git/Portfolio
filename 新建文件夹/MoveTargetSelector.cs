using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class MoveTargetSelector : MonoBehaviour
{
    public static MoveTargetSelector Instance;

    public Tilemap tilemap;
    public Transform selector;

    private List<Vector3Int> validCells = new();
    private Vector3Int currentCell;
    private UnitActionHandler currentUnit;
    private bool selecting = false;

    private float moveCooldown = 0.2f;
    private float lastMoveTime = 0f;

    private List<Vector3Int> currentPath;

    private SpriteRenderer selectorRenderer;

    void Awake() => Instance = this;

    void Start()
    {
        selectorRenderer = selector.GetComponent<SpriteRenderer>();
    }

    public void StartMoveSelection(UnitActionHandler unit, Vector3Int origin, int range)
    {
        UnitSelectorManager.disableInput = true;

        currentUnit = unit;
        selecting = true;
        ActionSelectorState.SetMove();
        validCells.Clear();

        List<Vector3Int> blockedCells = new();
        List<Vector3Int> waterCells = new();
        List<Vector3Int> enemyCells = new();

        // 有効移動範囲を計算する
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) <= range)
                {
                    Vector3Int offset = new Vector3Int(x, y, 0);
                    Vector3Int cell = origin + offset;

                    // 有効な移動範囲生成: 空間または現在の位置
                    if (!tilemap.HasTile(cell)) continue;

                    // 水か石かを判断する
                    TileBase water = tilemap.GetTile(cell);
                    TileBase rock = tilemap.GetTile(cell);
                    if (water != null && IsWaterTile(water))
                    {
                        waterCells.Add(cell); // 水です→赤色表示
                        continue;
                    }
                    else if (rock != null && IsRockTile(rock))
                    {
                        blockedCells.Add(cell);
                        continue;
                    }

                    // 対象グリッドにユニットがあり、それが自身ではない場合
                    var unitAtCell = TurnManager.Instance.GetUnitAtCell(cell);
                    if (unitAtCell != null && cell != origin)
                    {
                        if (unitAtCell == currentUnit)
                        {
                            validCells.Add(cell);
                        }
                        else if (unitAtCell.owner == currentUnit.owner)
                        {
                            validCells.Add(cell);
                        }
                        else
                        {
                            validCells.Add(cell);
                            enemyCells.Add(cell);
                        }
                    }
                    else
                    {
                        validCells.Add(cell);
                    }
                }
            }
        }

        currentUnit.rangeVisualizer.Clear();
        currentUnit.rangeVisualizer.ShowCells(validCells, RangeVisualizer.Instance.moveTile);
        currentUnit.rangeVisualizer.ShowCells(blockedCells, RangeVisualizer.Instance.blockedTile);
        currentUnit.rangeVisualizer.ShowWaterCells(waterCells, RangeVisualizer.Instance.waterTile);
        currentUnit.rangeVisualizer.ShowCells(enemyCells, RangeVisualizer.Instance.blockedTile);

        currentCell = origin;
        MoveSelector(Vector3Int.zero);
    }

    void Update()
    {
        if (PauseUIManager.IsGamePaused) return;

        if (ActionSelectorState.CurrentMode == SelectorMode.Attack) return;

        var input = TurnManager.Instance.GetCurrentPlayerInput();

        if (selecting)
        {
            Vector2 moveInput = input.GetMoveInput();
            Vector3Int dir = Vector3Int.zero;

            if (moveInput.magnitude > 0.5f && Time.time - lastMoveTime > moveCooldown)
            {
                if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
                    dir = moveInput.x > 0 ? Vector3Int.right : Vector3Int.left;
                else
                    dir = moveInput.y > 0 ? Vector3Int.up : Vector3Int.down;

                if (dir != Vector3Int.zero)
                {
                    lastMoveTime = Time.time;
                }
            }

            if (dir != Vector3Int.zero)
                MoveSelector(dir);

            Vector3Int unitCell = GridSelector.Instance.tilemap.WorldToCell(currentUnit.transform.position);
            if (validCells.Contains(currentCell))
            {
                currentPath = Pathfinding.FindPath(tilemap, unitCell, currentCell, new HashSet<Vector3Int>(validCells));

                if (currentPath.Count == 0 || currentPath[0] != unitCell)
                    currentPath.Insert(0, unitCell);

                // パス生成と移動
                RangeVisualizer.Instance.ShowPath(currentPath);
            }
            else
            {
                currentPath = null;
                RangeVisualizer.Instance.ClearPath();
            }

            // ターゲットを移動する（Aボタン）
            if (input.IsConfirmPressed())
            {
                if (validCells.Contains(currentCell))
                {
                    var unitAtCell = TurnManager.Instance.GetUnitAtCell(currentCell);
                    if (unitAtCell != null && unitAtCell != currentUnit)
                    {
                        Debug.Log("そのマスには他のユニットがいます！");
                        return;
                    }

                    if (currentPath != null && currentPath.Count > 1)
                    {
                        currentUnit.IsMoving = true;
                        //currentUnit.unitController.MoveAlongPath(currentPath);
                        currentUnit.hasMoved = true;
                        StartCoroutine(MoveAndMarkDone(currentUnit, currentPath));
                    }
                    else
                    {
                        //currentUnit.unitController.SnapToGrid(tilemap.GetCellCenterWorld(currentCell));
                        return;
                    }

                    currentUnit.MarkAsMoved();

                    RangeVisualizer.Instance.ClearPath();
                    //var nextUnit = currentUnit;
                    EndSelection();
                    // 移動後、操作メニューを再表示（移動ボタンは無効化された状態で）
                    //ActionMenuUI.Instance.ShowMenu(nextUnit);
                }
            }
        }

        // キャンセル（Bボタン）
        if (input.IsCancelPressed())
        {
            if (currentUnit == null || currentUnit.IsMoving || currentUnit.isDone)
            {
                return;
            }

            currentUnit.CancelMove();
            UnitSelectorManager.Instance.SetSwitchEnabled(true);
            GridSelector.Instance.SetToCell(currentUnit.unitController.GetCurrentCell());
            RangeVisualizer.Instance.ClearPath();
            EndSelection();
        }
    }

    private IEnumerator MoveAndMarkDone(UnitActionHandler unit, List<Vector3Int> path)
    {
        yield return unit.unitController.MoveAlongPath(path);

        unit.IsMoving = false;
        unit.MarkAsMoved();
        ActionMenuUI.Instance.ShowMenu(unit);
    }

    void MoveSelector(Vector3Int delta)
    {
        Vector3Int next = currentCell + delta;
        if (!validCells.Contains(next)) return;

        currentCell = next;
        selector.position = tilemap.GetCellCenterWorld(currentCell);

        var unit = TurnManager.Instance.GetUnitAtCell(currentCell);

        if (unit != null && unit != currentUnit && unit.owner == currentUnit.owner)
        {
            selectorRenderer.color = Color.red;  // 友軍
        }
        else
        {
            selectorRenderer.color = Color.white; // 自分か敵か空きスペースか
        }
    }

    public void EndSelection()
    {
        UnitSelectorManager.disableInput = false;
        if (currentUnit != null)
        {
            currentUnit.rangeVisualizer.Clear();

            // 再度メニュー表示（移動済みフラグが反映された状態）
            ActionMenuUI.Instance.ShowMenu(currentUnit);
        }
        selecting = false;
        ActionSelectorState.SetNone();
        //currentUnit = null;
        selectorRenderer.color = Color.white;
    }

    public bool IsWaterTile(TileBase tile)
    {
        return tile.name.ToLower().Contains("water"); 
    }

    public bool IsRockTile(TileBase tile)
    {
        return tile.name.ToLower().Contains("rock");
    }
}
