using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public enum UnitOwner { Player1, Player2 }
public class UnitActionHandler : CharaBase
{
    //public UnitOwner owner;   // 所属プレイヤー

    //public int moveRange = 3;
    //public int attackRange = 2;

    public RangeVisualizer rangeVisualizer;
    public UnitController unitController;
    private CharaAnimator animator;

    public bool hasMoved = false;
    public bool isDone = false;
    public bool isDead = false;

    private Vector3Int initialCellThisTurn; // ターン開始時に記録
    private int dir;
    private bool hasPreparedThisTurn = false;
    public bool IsMoving { get; set; } = false;

    public string uniqueID;

    void Awake()
    {
        if (unitController == null)
            unitController = GetComponent<UnitController>();

        if(rangeVisualizer == null)
            rangeVisualizer = GameObject.Find("RangeTilemap").GetComponent<RangeVisualizer>();

        if (animator == null)
            animator = GetComponent<CharaAnimator>();

        uniqueID = System.Guid.NewGuid().ToString();
    }

    void Start()
    {
        //InitFromCharaData();

        // 方向を初期化する
        //dir = owner == UnitOwner.Player1 ? 1 : 5;
        //animator.InitializeDirection(dir);

        //TurnManager.Instance.RegisterUnit(this);
    }

    public void ShowMoveRange()
    {
        Vector3Int cell = unitController.GetCurrentCell();
        Debug.Log($"Showing move range from cell {cell}");
        //rangeVisualizer.ShowRange(cell, moveRange, true);

        MoveTargetSelector.Instance.StartMoveSelection(this, cell, GetSpd());
        ActionMenuUI.Instance.HideMenu();
    }

    public void ShowAttackRange()
    {
        if (isDone) return;

        Vector3Int cell = unitController.GetCurrentCell();
        //rangeVisualizer.ShowRange(cell, attackRange, false);

        // メニューを閉じる
        ActionMenuUI.Instance.HideMenu();

        AttackTargetSelector.Instance.StartAttackSelection(this, cell, GetAtkRange());
    }

    public void Wait()
    {
        Debug.Log($"{name} is waiting.");

        int recoverAmount = 60;
        if (GetCharaType() == CharaType.Physical) RecoverSTA(recoverAmount);
        else if (GetCharaType() == CharaType.Magical) RecoverMP(recoverAmount);

        isDone = true;
        hasMoved = true;

        ActionMenuUI.Instance.HideMenu();
        RangeVisualizer.Instance.ClearPath();

        // 自動的に次のユニットへ移動
        UnitSelectorManager.Instance.NotifyUnitActedThisTurn();
        TurnManager.Instance.EndTurn();
    }

    public void PrepareTurn()
    {
        if (hasPreparedThisTurn) return;

        initialCellThisTurn = unitController.GetCurrentCell();
        dir = animator.GetCurrentDirection();
        Debug.Log($"{name} PrepareTurn → dir = {dir}, instance = {GetInstanceID()}");
        hasPreparedThisTurn = true;
        hasMoved = false;
        Debug.Log($"{name} PrepareTurn → 初期位置: {initialCellThisTurn}");
    }

    public void ResetTurnFlags()
    {
        hasPreparedThisTurn = false;
        isDone = false;
        hasMoved = false;
    }

    public void CancelMove()
    {
        if (!hasMoved) return;

        unitController.SnapToGrid(initialCellThisTurn);
        hasMoved = false;

        Debug.Log($"{name} CancelMove → dir = {dir}, instance = {GetInstanceID()}");
        animator.InitializeDirection(dir);

        Debug.Log("移動を取り消しました。");
        ActionMenuUI.Instance.ShowMenu(this);
    }

    public void MarkAsMoved()
    {
        hasMoved = true;
        TurnManager.Instance.RegisterUnit(this);
        Debug.Log($"{name} moved to {unitController.GetCurrentCell()}");
    }

    public bool CanActThisTurn(UnitOwner currentTurnOwner)
    {
        return owner == currentTurnOwner && !isDone;
    }

    public void PlayLightAttack(Vector3Int from, Vector3Int to, Tilemap tilemap)
    {
        animator?.PlayLightAttack(from, to, tilemap);
        hasMoved = true;
        isDone = true;
    }

    public void PlayHeavyAttack(Vector3Int from, Vector3Int to, Tilemap tilemap)
    {
        animator?.PlayHeavyAttack(from, to, tilemap);
        hasMoved = true;
        isDone = true;
    }

    public void PlayHitAnim()
    {
        animator?.PlayHit();
    }

    public void OnDeath()
    {
        isDead = true;

        Debug.Log($"{name} は戦闘不能になった！");

        ActionMenuUI.Instance.HideMenu();

        TurnManager.Instance.UnregisterUnit(this);

        // 死亡後チェック：全部ユニットが死亡られるかどうか
        var remaining = FindObjectsOfType<UnitActionHandler>()
            .Where(u => u.owner == owner && !u.isDead)
            .ToList();

        if (remaining.Count == 0)
        {
            UnitOwner winner = owner == UnitOwner.Player1 ? UnitOwner.Player2 : UnitOwner.Player1;


            var units = (winner == UnitOwner.Player1) ? PlayerDeployData.Instance.player1Units
                                               : PlayerDeployData.Instance.player2Units;

            GameResultManager.Instance.SetResult(winner, units);
            SceneManager.LoadScene("EndScene");
        }

        // 死んだら → 自動的に次のユニットへ移動
        //if (UnitSelectorManager.Instance != null)
        //    UnitSelectorManager.Instance.SelectNextAvailableUnit();

        UnitSelectorManager.Instance.OnUnitDeath(this);
        Destroy(gameObject);
    }

    public bool UnSupportsHeavyAttack()
    {
        string id = GetCharaID();
        return id == "Archer_Prefab";
    }
}
