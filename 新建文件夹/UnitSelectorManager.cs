using DG.Tweening.Core.Easing;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectorManager : MonoBehaviour
{
    public static UnitSelectorManager Instance;
    public GridSelector selector;

    private List<UnitActionHandler> allUnits = new();
    private List<UnitActionHandler> activeUnits = new();
    private int currentIndex = 0;
    public static bool disableInput = false;
    private bool canSwitchUnit = true;

    private bool unitHasActedThisTurn = false; // このターンで既に誰か行動したか

    void Awake()
    {
        Instance = this;
    }

    public void StartTurn(UnitOwner owner)
    {
        unitHasActedThisTurn = false;
        canSwitchUnit = true;
        //allUnits = new List<UnitActionHandler>(FindObjectsOfType<UnitActionHandler>());
        //activeUnits = allUnits.FindAll(u => u.owner == owner && !u.isDone);
        allUnits = new List<UnitActionHandler>(
           FindObjectsOfType<UnitActionHandler>().Where(u => u != null && !u.isDead).ToList()
       );

        bool anyAlive = allUnits.Any(u => u.owner == owner);
        activeUnits = allUnits.FindAll(u => u.owner == owner && !u.isDone);

        if (activeUnits.Count == 0 && anyAlive)
        {
            Debug.Log($"{owner} has no available units. Skipping turn.");
            //TurnManager.Instance.EndTurn();
            //return;
            foreach (var unit in allUnits)
            {
                if (unit.owner == owner)
                    unit.ResetTurnFlags();
            }

            activeUnits = allUnits.FindAll(u => u.owner == owner && !u.isDone);
        }

        if (activeUnits.Count == 0)
        {
            TurnManager.Instance.EndTurn();
            return;
        }

        currentIndex = 0;
        FocusCurrentUnit();
    }

    void Update()
    {
        if (InputLock.IsLocked) return;

        if (disableInput || activeUnits.Count == 0
            || TurnManager.Instance == null 
            || TurnManager.Instance.currentInputDevice == null) return;

        var input = TurnManager.Instance.GetCurrentPlayerInput();

        // プレイヤーに応じたデバイスからの入力でユニット切替
        bool next = false, prev = false;

        // 右にユニットを変える（ eキー / R1）左二ユニットを変える（qキー / L1）
        if (input.controlScheme == ControlScheme.Keyboard && input.device is Keyboard kb)
        {
            next = kb.eKey.wasPressedThisFrame;
            prev = kb.qKey.wasPressedThisFrame;
        }
        else if (input.controlScheme == ControlScheme.Gamepad && input.device is Gamepad gp)
        {
            next = gp.rightShoulder.wasPressedThisFrame;
            prev = gp.leftShoulder.wasPressedThisFrame;
        }

        if (canSwitchUnit && (next || prev))
        {
            int direction = next ? 1 : -1;
            //int originalIndex = currentIndex;

            // 行動していない次のキャラクターを前方または後方に検索します
            for (int i = 1; i <= activeUnits.Count; i++)
            {
                int newIndex = (currentIndex + direction * i + activeUnits.Count) % activeUnits.Count;
                if (!activeUnits[newIndex].isDone)
                {
                    currentIndex = newIndex;
                    FocusCurrentUnit();
                    break;
                }
            }
        }

        //if (next)
        //{
        //    currentIndex = (currentIndex + 1) % activeUnits.Count;
        //    FocusCurrentUnit();
        //}
        //else if (prev)
        //{
        //    currentIndex = (currentIndex - 1 + activeUnits.Count) % activeUnits.Count;
        //    FocusCurrentUnit();
        //}
    }

    void FocusCurrentUnit()
    {
        if (activeUnits.Count == 0)
        {
            Debug.Log("No active units remaining.");
            return;
        }

        for (int i = 0; i < activeUnits.Count; i++)
        {
            int tryIndex = (currentIndex + i) % activeUnits.Count;
            var unit = activeUnits[tryIndex];

            if (unit != null && !unit.isDone && unit.unitController != null && unit.unitController.gameObject)
            {
                currentIndex = tryIndex;
                unit.PrepareTurn();

                Vector3Int cell = unit.unitController.GetCurrentCell();
                selector.SetToCell(cell);

                // UI更新
                var ui = FindObjectOfType<CharaDataUI>();
                if (ui != null)
                    ui.ShowPreview(unit.GetComponent<CharaBase>());

                ActionMenuUI.Instance.ShowMenu(unit);

                Debug.Log($"[UnitSelectorManager] フォーカス: {unit.name}");
                return;
            }
        }

        Debug.Log("[UnitSelectorManager] 行動可能なユニットがいません。ターン終了");
        TurnManager.Instance.EndTurn();
    }

    public void NotifyUnitActedThisTurn()
    {
        unitHasActedThisTurn = true;
    }

    public void SetSwitchEnabled(bool enabled)
    {
        canSwitchUnit = enabled;
    }

    public void OnUnitDeath(UnitActionHandler deadUnit)
    {
        if (activeUnits.Contains(deadUnit))
        {
            int indexToRemove = activeUnits.IndexOf(deadUnit);
            //activeUnits.Remove(deadUnit);
            activeUnits.RemoveAll(u => u.uniqueID == deadUnit.uniqueID);

            // 現在のロールが削除された場合、currentIndexを処理する
            if (indexToRemove <= currentIndex && currentIndex > 0)
                currentIndex--;
        }

        //allUnits.Remove(deadUnit);
        allUnits.RemoveAll(u => u.uniqueID == deadUnit.uniqueID);

        CharaDataUI ui = FindObjectOfType<CharaDataUI>();
        if (ui != null)
            ui.Hide();

        var remaining = FindObjectsOfType<UnitActionHandler>()
            .Where(u => u.owner == TurnManager.Instance.currentTurn && !u.isDead && !u.isDone)
            .ToList();

        if (remaining.Count == 0)
        {
            Debug.Log("行動可能なユニットがいないためターン終了");
            TurnManager.Instance.EndTurn();
        }
        else
        {
            activeUnits = remaining;
            currentIndex = 0;
            FocusCurrentUnit(); // まだ誰か残っているならフォーカスし直す
        }
    }
}
