using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    // グリッドの対応ユニット
    public Dictionary<Vector3Int, UnitActionHandler> unitMap = new();
    public List<UnitActionHandler> allUnits = new List<UnitActionHandler>();

    public UnitOwner currentTurn;
    public InputDevice currentInputDevice;

    public PlayerInputConfig player1Input = new PlayerInputConfig();
    public PlayerInputConfig player2Input = new PlayerInputConfig();

    public CharaData charaDataSO;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        charaDataSO.Init();
    }

    void Start()
    {
        
    }

    public void RegisterUnit(UnitActionHandler unit)
    {
        Vector3Int cell = unit.unitController.GetCurrentCell();
        unitMap[cell] = unit;
        //if (!allUnits.Contains(unit)) allUnits.Add(unit);

        if (!allUnits.Exists(u => u.uniqueID == unit.uniqueID))
        {
            allUnits.Add(unit);
        }
    }

    public void UnregisterUnit(UnitActionHandler unit)
    {
        Vector3Int cell = unit.unitController.GetCurrentCell();
        if (unitMap.ContainsKey(cell) && unitMap[cell] == unit)
            unitMap.Remove(cell);
        allUnits.Remove(unit);
    }

    public UnitActionHandler GetUnitAtCell(Vector3Int cell)
    {
        unitMap ??= new Dictionary<Vector3Int, UnitActionHandler>();

        foreach (var unit in allUnits)
        {
            if (!unit.isActiveAndEnabled) continue;
            Vector3Int pos = GridSelector.Instance.tilemap.WorldToCell(unit.transform.position);
            if (pos == cell) return unit;
        }
        return null;
    }

    public void EndTurn()
    {
        currentTurn = (currentTurn == UnitOwner.Player1) ? UnitOwner.Player2 : UnitOwner.Player1;
        Debug.Log($"Turn changed to {currentTurn}");
        currentInputDevice = GetInputDeviceForPlayer(currentTurn);

        // すべてのユニットの状態をリセット
        //foreach (var unit in FindObjectsOfType<UnitActionHandler>())
        //{
        //    if (unit.owner == currentTurn)
        //    {
        //        unit.hasMoved = false;
        //        unit.hasActed = false;
        //    }
        //}

        TurnBannerUI.Instance.ShowTurn(currentTurn);
        // プレイヤーのユニット選択開始
        UnitSelectorManager.Instance.StartTurn(currentTurn);
    }

    void AssignDevicesAutomatically()
    {
        var gamepads = Gamepad.all;

        if (gamepads.Count >= 2)
        {
            // ２つゲームパッド
            player1Input.controlScheme = ControlScheme.Gamepad;
            player1Input.device = gamepads[0];
            InputSystem.EnableDevice(gamepads[0]);

            player2Input.controlScheme = ControlScheme.Gamepad;
            player2Input.device = gamepads[1];
            InputSystem.EnableDevice(gamepads[1]);
        }
        else if (gamepads.Count == 1)
        {
            // ゲームパッド + キーボード
            player1Input.controlScheme = ControlScheme.Gamepad;
            player1Input.device = gamepads[0];
            InputSystem.EnableDevice(gamepads[0]);

            player2Input.controlScheme = ControlScheme.Keyboard;
            player2Input.device = Keyboard.current;
        }
        else
        {
            // キーボード
            player1Input.controlScheme = ControlScheme.Keyboard;
            player1Input.device = Keyboard.current;

            player2Input.controlScheme = ControlScheme.Keyboard;
            player2Input.device = Keyboard.current;
        }

        InputSystem.Update();
    }

    public InputDevice GetInputDeviceForPlayer(UnitOwner owner)
    {
        return owner == UnitOwner.Player1 ? player1Input.device : player2Input.device;
    }

    public void BeginGame()
    {
        if (player1Input.device == null || player2Input.device == null)
        {
            AssignDevicesAutomatically();
        }

        // 追加: すべてのユニットを登録
        allUnits.Clear();
        allUnits.AddRange(FindObjectsOfType<UnitActionHandler>());

        // ランダムに先攻を決定
        currentTurn = (Random.value < 0.5f) ? UnitOwner.Player1 : UnitOwner.Player2;
        Debug.Log($"Coin toss result: {currentTurn} starts!");
        currentInputDevice = GetInputDeviceForPlayer(currentTurn);

        TurnBannerUI.Instance.ShowTurn(currentTurn);

        UnitSelectorManager.Instance.StartTurn(currentTurn);
    }

    public PlayerInputConfig GetCurrentPlayerInput()
    {
        return currentTurn == UnitOwner.Player1 ? player1Input : player2Input;
    }
}
