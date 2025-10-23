using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AttackTargetSelector : MonoBehaviour
{
    public static AttackTargetSelector Instance;

    public Tilemap tilemap;
    public Transform selector;

    private List<Vector3Int> validCells = new();
    private Vector3Int currentCell;
    private UnitActionHandler currentUnit;
    private bool selecting = false;

    private float moveCooldown = 0.2f;
    private float lastMoveTime = 0f;

    private SpriteRenderer selectorRenderer;

    public QTEExecutor qteExecutor;

    void Awake() => Instance = this;

    void Start()
    {
        selectorRenderer = selector.GetComponent<SpriteRenderer>();
    }

    public void StartAttackSelection(UnitActionHandler unit, Vector3Int origin, int range)
    {
        UnitSelectorManager.disableInput = true;

        currentUnit = unit;
        selecting = true;
        ActionSelectorState.SetAttack();
        validCells.Clear();

        List<Vector3Int> enemyCells = new();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) > range) continue;

                Vector3Int offset = new Vector3Int(x, y, 0);
                Vector3Int cell = origin + offset;

                if (!tilemap.HasTile(cell)) continue;

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

        currentUnit.rangeVisualizer.Clear();
        currentUnit.rangeVisualizer.ShowCells(validCells, RangeVisualizer.Instance.attackTile);
        //currentUnit.rangeVisualizer.ShowCells(enemyCells, RangeVisualizer.Instance.blockedTile);

        currentCell = origin;
        MoveSelector(Vector3Int.zero);
    }

    void Update()
    {
        if (PauseUIManager.IsGamePaused) return;

        if (ActionSelectorState.CurrentMode != SelectorMode.Attack) return;

        var input = TurnManager.Instance.GetCurrentPlayerInput();
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

        //角度値の視覚化
        Vector3 from = currentUnit.unitController.transform.position;
        Vector3 to = tilemap.GetCellCenterWorld(currentCell);
        Vector3 rawDir = (to - from).normalized;
        Debug.DrawRay(from, rawDir, Color.green);

        // Aボタンー攻撃
        // 攻撃範囲エリアのリアルタイム表示（現在選択されているグリッドを中心）
        currentUnit.rangeVisualizer.Clear();
        currentUnit.rangeVisualizer.ShowCells(validCells, RangeVisualizer.Instance.attackTile);

        var attackPattern = currentUnit.GetAtkPattern();
        Vector3Int attackerCell = currentUnit.unitController.GetCurrentCell();
        Vector3Int selected = currentCell;

        int directionIndex = AttackDirectionHelper.GetDirectionIndex(attackerCell, selected, tilemap);

        Vector3Int[] baseOffsets = attackPattern != null ? attackPattern.offsets : new Vector3Int[] { Vector3Int.zero };
        Vector3Int[] finalOffsets = (attackPattern != null && attackPattern.useDirectionalOffset)
           ? AttackDirectionHelper.RotateOffsets(baseOffsets, directionIndex)
           : baseOffsets;

        currentUnit.rangeVisualizer.ShowPatternCells(currentCell, finalOffsets, RangeVisualizer.Instance.blockedTile);

        if (input.IsConfirmPressed())
        {
            if (validCells.Contains(currentCell))
            {
                if (!HasEnemyInArea(currentCell, finalOffsets))
                {
                    return;
                }

                ExecuteAreaAttack(currentCell, finalOffsets);

                //currentUnit.hasActed = true;
                //currentUnit.hasMoved = true;
                //currentUnit.isDone = true;
                //EndSelection();

                //ActionMenuUI.Instance.HideMenu();
                //UnitSelectorManager.Instance.NotifyUnitActedThisTurn();
                //StartCoroutine(WaitForDamageEffectAndEndTurn());

                //var target = TurnManager.Instance.GetUnitAtCell(currentCell);
                //if (target != null && target != currentUnit)
                //{
                //    if (target.owner != currentUnit.owner)
                //    {
                //        Debug.Log($"{currentUnit.name} が {target.name} に攻撃した！");

                //        Vector3Int from = currentUnit.unitController.GetCurrentCell();
                //        Vector3Int to = target.unitController.GetCurrentCell();

                //        // 攻撃アニメーションが再生される（攻撃者）
                //        currentUnit.PlayLightAttack(from, to, tilemap);

                //        // 負傷アニメーションが再生される（ターゲット）
                //        Debug.Log($"[test] {currentUnit.name} → {target.name} 攻撃力 = {currentUnit.GetAtk()}");
                //        target.TakeDamage(currentUnit.GetAtk());
                //        target.PlayHitAnim();

                //        currentUnit.hasActed = true;
                //        currentUnit.hasMoved = true;
                //        EndSelection();

                //        ActionMenuUI.Instance.HideMenu();
                //        UnitSelectorManager.Instance.SelectNextAvailableUnit();
                //    }
                //}
            }
        }

        // Bボタンーキャンセル
        if (input.IsCancelPressed())
        {
            UnitSelectorManager.Instance.SetSwitchEnabled(true);
            GridSelector.Instance.SetToCell(currentUnit.unitController.GetCurrentCell());
            EndSelection();
        }
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

    void EndSelection()
    {
        UnitSelectorManager.disableInput = false;
        if (currentUnit != null)
        {
            currentUnit.rangeVisualizer.Clear();

            // 再度メニュー表示（攻撃済みフラグが反映された状態）
            ActionMenuUI.Instance.ShowMenu(currentUnit);
        }
        selecting = false;
        ActionSelectorState.SetNone();
        currentUnit = null;
        selectorRenderer.color = Color.white;
    }

    void ExecuteAreaAttack(Vector3Int center, Vector3Int[] rotatedOffsets)
    {
        List<UnitActionHandler> targets = new();
        Vector3Int from = currentUnit.unitController.GetCurrentCell();

        foreach (var offset in rotatedOffsets)
        {
            Vector3Int cell = center + offset;
            var target = TurnManager.Instance.GetUnitAtCell(cell);
            if (target != null && target.owner != currentUnit.owner)
            {
                Debug.Log($"{currentUnit.name} が {target.name} に攻撃した！");

                targets.Add(target);

                //Vector3Int from = currentUnit.unitController.GetCurrentCell();
                //Vector3Int to = target.unitController.GetCurrentCell();

                // 攻撃アニメーションが再生される（攻撃者）
                //currentUnit.PlayLightAttack(from, to, tilemap);

                // 負傷アニメーションが再生される（ターゲット）
                //Debug.Log($"[test] {currentUnit.name} → {target.name} 攻撃力 = {currentUnit.GetAtk()}");

                //int basePower = currentUnit.GetCharaType() == CharaType.Physical
                //    ? currentUnit.GetAtk()
                //    : currentUnit.GetAts();

                //CharaType atkType = currentUnit.GetCharaType() == CharaType.Physical
                //    ? CharaType.Physical
                //    : CharaType.Magical;

                //target.TakeDamage(basePower, DamageType.Normal, atkType);

                //target.TakeDamage(currentUnit.GetAtk());
                //target.PlayHitAnim();
                //StartCoroutine(QTEAndDamageRoutine(currentUnit, target, from, to));
            }
        }

        if (targets.Count > 0)
        {
            StartCoroutine(QTEAndDamageRoutine(currentUnit, targets, from));
        }
    }

    private IEnumerator QTEAndDamageRoutine(UnitActionHandler attacker, List<UnitActionHandler> targets,Vector3Int from)
    {
        CharaType atkType = attacker.GetCharaType();
        QTEType qteType = attacker.GetQTEType();

        int basePower = atkType == CharaType.Physical ? attacker.GetAtk() : attacker.GetAts();

        QTERank resultRank = QTERank.Failed;

        yield return StartCoroutine(qteExecutor.ExecuteQTE(qteType, attacker.owner, rank =>
        {
            resultRank = rank;
        }));       

        yield return new WaitUntil(() => qteExecutor.isover);

        // mp or STA の処理
        int cost = resultRank switch
        {
            QTERank.Failed => 40,
            QTERank.Good => 35,
            QTERank.Great => 25,
            _ => 40
        };

        bool costSuccess = false;

        if (atkType == CharaType.Physical)
            costSuccess = attacker.TryConsumeSTA(cost);
        else
            costSuccess = attacker.TryConsumeMP(cost);

        if (!costSuccess)
        {
            if (resultRank == QTERank.Great)
            {
                resultRank = QTERank.Good;
            }
            else if (resultRank == QTERank.Good)
            {
                resultRank = QTERank.Failed;
            }
            else if (resultRank == QTERank.Failed)
            {
                Debug.Log($"{attacker.name} 資源不足かつQTE失敗 → 攻撃失敗");
                attacker.isDone = true;
                EndSelection();
                ActionMenuUI.Instance.HideMenu();
                UnitSelectorManager.Instance.NotifyUnitActedThisTurn();
                yield return new WaitForSeconds(0.3f);
                StartCoroutine(WaitForDamageEffectAndEndTurn());
                yield break;
            }

            // 消耗所有资源
            if (atkType == CharaType.Physical)
            {
                attacker.TryConsumeSTA(attacker.GetCurrentSTA());
            }
            else
            {
                attacker.TryConsumeMP(attacker.GetCurrentMP());
            }
        }

        Vector3Int to = targets[0].unitController.GetCurrentCell();

        if (resultRank == QTERank.Great && !attacker.UnSupportsHeavyAttack())
            attacker.PlayHeavyAttack(from, to, tilemap);
        else
            attacker.PlayLightAttack(from, to, tilemap);

        yield return new WaitForSeconds(0.5f);

        DamageType damageType = resultRank switch
        {
            QTERank.Failed => DamageType.Low,
            QTERank.Good => DamageType.Normal,
            QTERank.Great => DamageType.Crit,
            _ => DamageType.Normal
        };

        foreach (var target in targets)
        {
            target.TakeDamage(basePower, damageType, atkType);
            target.PlayHitAnim();
        }
        yield return new WaitForSeconds(0.5f);

        attacker.isDone = true;

        EndSelection();
        ActionMenuUI.Instance.HideMenu();
        
        UnitSelectorManager.Instance.NotifyUnitActedThisTurn();

        yield return new WaitForSeconds(0.3f);
        StartCoroutine(WaitForDamageEffectAndEndTurn());
    }

    //void ExecuteAreaAttack(Vector3Int center, Vector3Int[] rotatedOffsets)
    //{
    //    foreach (var offset in rotatedOffsets)
    //    {
    //        Vector3Int cell = center + offset;
    //        var target = TurnManager.Instance.GetUnitAtCell(cell);
    //        if (target != null && target.owner != currentUnit.owner)
    //        {
    //            int basePower = currentUnit.GetCharaType() == CharaType.Physical
    //                ? currentUnit.GetAtk()
    //                : currentUnit.GetAts();

    //            CharaType atkType = currentUnit.GetCharaType();

    //            // ✅ 尝试消耗资源
    //            bool canAttack = atkType == CharaType.Physical
    //                ? currentUnit.TryConsumeSTA(10)
    //                : currentUnit.TryConsumeMP(10);

    //            if (!canAttack)
    //            {
    //                Debug.LogWarning($"{currentUnit.name} はリソース不足で攻撃できない！");
    //                continue; // 跳过这次攻击
    //            }

    //            Debug.Log($"{currentUnit.name} が {target.name} に攻撃した！");

    //            Vector3Int from = currentUnit.unitController.GetCurrentCell();
    //            Vector3Int to = target.unitController.GetCurrentCell();

    //            // 攻撃アニメーション（攻撃者）
    //            currentUnit.PlayLightAttack(from, to, tilemap);

    //            // ダメージ処理
    //            target.TakeDamage(basePower, DamageType.Normal, atkType);
    //            target.PlayHitAnim();
    //        }
    //    }
    //}

    private bool HasEnemyInArea(Vector3Int center, Vector3Int[] offsets)
    {
        foreach (var offset in offsets)
        {
            Vector3Int cell = center + offset;
            var unit = TurnManager.Instance.GetUnitAtCell(cell);
            if (unit != null && unit.owner != currentUnit.owner)
            {
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying || currentUnit == null) return;

        Vector3 from = currentUnit.unitController.transform.position;
        Vector3 to = tilemap.GetCellCenterWorld(currentCell);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(from, to);

        // 角度値の視覚化
        Vector2 dir = (to - from).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle = (angle + 360f) % 360f;
        UnityEngine.GUIStyle style = new UnityEngine.GUIStyle();
        style.normal.textColor = Color.yellow;
        UnityEditor.Handles.Label(from + Vector3.up * 0.5f, $"angle: {angle:F1}°", style);
#endif
    }

    IEnumerator WaitForDamageEffectAndEndTurn()
    {
        yield return new WaitForSeconds(1.5f);
        TurnManager.Instance.EndTurn();
    }
}
