using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ActionMenuUI : MonoBehaviour
{
    public static ActionMenuUI Instance;

    public GameObject menuRoot;
    public Button moveBtn, attackBtn, waitBtn;
    public GameObject selectorIndicator;

    private UnitActionHandler currentUnit;
    private Button[] buttons;
    private int selectedIndex = 0;
    private float inputDelay = 0.2f;
    private float lastInputTime = 0f;

    void Awake() => Instance = this;

    void Start()
    {
        // ボタンリスト初期化
        buttons = new Button[] { moveBtn, attackBtn, waitBtn };
    }

    void Update()
    {
        if (PauseUIManager.IsGamePaused) return;

        if (!menuRoot.activeSelf || currentUnit == null
            || TurnManager.Instance == null
            || TurnManager.Instance.currentInputDevice == null) return;

        var input = TurnManager.Instance.GetCurrentPlayerInput();
        Vector2 moveInput = input.GetMoveInput();
        float vertical = moveInput.x;

        // インデックス変更（一定時間ごとの操作を許可）
        if (Time.time - lastInputTime > inputDelay)
        {
            if (vertical < -0.5f)
            {
                selectedIndex = (selectedIndex - 1 + buttons.Length) % buttons.Length;
                lastInputTime = Time.time;
            }
            else if (vertical > 0.5f)
            {
                selectedIndex = (selectedIndex + 1) % buttons.Length;
                lastInputTime = Time.time;
            }
        }

        // インジケーターをボタンに合わせる
        if (selectorIndicator != null)
        {
            RectTransform btnRect = buttons[selectedIndex].GetComponent<RectTransform>();
            RectTransform indicatorRect = selectorIndicator.GetComponent<RectTransform>();

            // buttonのanchoredPositionにより、LayoutGroup内にアライメントする
            //Vector2 buttonAnchor = btnRect.anchoredPosition;

            // 右にずれる
            //Vector2 offset = new Vector2(270f, 0f);
            //indicatorRect.anchoredPosition = buttonAnchor + offset;

            // 選択を今のbuttonに囲まれる
            //indicatorRect.position = btnRect.position;

            RectTransform target = buttons[selectedIndex].GetComponent<RectTransform>();
            selectorIndicator.transform.DOMove(target.position, 0.1f).SetEase(Ease.OutQuad);
        }

        // Aボタン（決定）
        if (input.IsConfirmPressed())
        {
            if (buttons[selectedIndex].interactable)
                buttons[selectedIndex].onClick.Invoke();
        }

        // Bボタン（キャンセル）
        if (input.IsCancelPressed())
        {
            
        }
    }


    public void ShowMenu(UnitActionHandler unit)
    {
        if (unit == null)
        {
            Debug.LogError("❗ShowMenu代入したunitはnull！");
            return;
        }

        currentUnit = unit;
        menuRoot.SetActive(true);
        selectorIndicator.SetActive(true);

        CharaDataUI ui = FindObjectOfType<CharaDataUI>();
        if (ui != null)
            ui.ShowPreview(unit.GetCurrentCharaInfo(), unit.charaType);

        // すべてのボタンのイベントを一度クリア
        moveBtn.onClick.RemoveAllListeners();
        attackBtn.onClick.RemoveAllListeners();
        waitBtn.onClick.RemoveAllListeners();

        // イベント再登録
        moveBtn.onClick.AddListener(() => 
        {
            if (!moveBtn.interactable) return;
            UnitSelectorManager.Instance.SetSwitchEnabled(false);
            unit.ShowMoveRange();
        });
        attackBtn.onClick.AddListener(() =>
        {
            UnitSelectorManager.Instance.SetSwitchEnabled(false);
            unit.ShowAttackRange();
        });
        waitBtn.onClick.AddListener(() => unit.Wait());
        
        // button状態をリセットする
        moveBtn.interactable = !unit.hasMoved;
        attackBtn.interactable = true;
        waitBtn.interactable = true;

        // 最初に選択するボタンをリセット
        selectedIndex = 0;
    }

    public void HideMenu()
    {
        menuRoot.SetActive(false);
        selectorIndicator.SetActive(false);
        currentUnit = null;
    }
}
