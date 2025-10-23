using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class CharacterSelectorByDevice : MonoBehaviour
{
    [Header("Player 1")]
    public RectTransform[] player1Options;
    public RectTransform player1Selector;
    private int player1Index = 0;
    private float p1Cooldown = 0f;
    public List<int> player1Selections = new List<int>();
    private bool player1Locked = false;
    private float p1LockDelayTimer = 0f;
    public TextMeshProUGUI p1LockedText;

    [Header("Player 2")]
    public RectTransform[] player2Options;
    public RectTransform player2Selector;
    private int player2Index = 0;
    private float p2Cooldown = 0f;
    public List<int> player2Selections = new List<int>();
    private bool player2Locked = false;
    private float p2LockDelayTimer = 0f;
    public TextMeshProUGUI p2LockedText;

    [Header("UI Canvas")]
    public GameObject characterSelectUI; // Canvas
    public GameObject deploySystem;      // DeploymentManager
    [SerializeField] private BurnableUIController burnUIController;

    [Header("Settings")]
    public float inputCooldown = 0.3f; // 速すぎる動きを防ぐ
    public int columns = 3;

    [Header("プレビュープレハブ表示")]
    public GameObject[] previewPrefabs;

    // ホバープレビューコントロール（公式インスタンスではカウントされません）
    private GameObject player1HoverPreview;
    private GameObject player2HoverPreview;

    public Transform[] player1Slots; // Slot0/1/2 的Transform
    public Transform[] player2Slots;

    private GameObject[] player1PreviewInstances = new GameObject[3];
    private GameObject[] player2PreviewInstances = new GameObject[3];

    private bool deployedStarted = false;

    private int randomIndex = 5;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (TurnManager.Instance == null) return;

        HandlePlayerInput(TurnManager.Instance.player1Input, ref player1Index, player1Options, player1Selector, ref p1Cooldown, player1Selections, 1);
        HandlePlayerInput(TurnManager.Instance.player2Input, ref player2Index, player2Options, player2Selector, ref p2Cooldown, player2Selections, 2);

        // 双方が選択したかどうかを確認する
        if (player1Selections.Count == 3 && !player1Locked)
        {
            p1LockDelayTimer += Time.deltaTime;
            if (p1LockDelayTimer > 0.2f && TurnManager.Instance.player1Input.IsConfirmPressed())
            {
                player1Locked = true;
                UILockTextAnimator.AnimateLockIn(p1LockedText, 1);
                Debug.Log("Player1 ロック！");
            }  
        }

        if (player2Selections.Count == 3 && !player2Locked)
        {
            p2LockDelayTimer += Time.deltaTime;
            if (p2LockDelayTimer > 0.2f && TurnManager.Instance.player2Input.IsConfirmPressed())
            {
                player2Locked = true;
                UILockTextAnimator.AnimateLockIn(p2LockedText, 2);
                Debug.Log("Player2 ロック！");
            }  
        }

        // プレイヤー1のロック解除（キャンセル）
        if (player1Locked && TurnManager.Instance.player1Input.IsCancelPressed())
        {
            player1Locked = false;
            p1LockDelayTimer = 0f;
            UILockTextAnimator.AnimateLockOut(p1LockedText, 1);
            Debug.Log("Player1 ロック解除");
        }

        // プレイヤー2のロック解除（キャンセル）
        if (player2Locked && TurnManager.Instance.player2Input.IsCancelPressed())
        {
            player2Locked = false;
            p2LockDelayTimer = 0f;
            UILockTextAnimator.AnimateLockOut(p2LockedText, 2);
            Debug.Log("Player2 ロック解除");
        }

        // 両者がロックした後→フォーメーションシステムを開始
        if (player1Locked && player2Locked && !deployedStarted)
        {
            deployedStarted = true;
            PlayerDeployData.Instance.player1Selections = new List<int>(player1Selections);
            PlayerDeployData.Instance.player2Selections = new List<int>(player2Selections);

            Debug.Log("両者がロックオンしました → 位置を選ぶ開始");

            burnUIController.StartBurn(() =>
            {
                if (characterSelectUI != null)
                    characterSelectUI.SetActive(false);

                if (deploySystem != null)
                    deploySystem.SetActive(true);
            });

            // 重複アクティベーションを防止する
            enabled = false;
        }
    }

    void HandlePlayerInput(
        PlayerInputConfig input, 
        ref int index, 
        RectTransform[] options, 
        RectTransform selector, 
        ref float cooldown, 
        List<int> selections, 
        int playerNum)
    {
        cooldown -= Time.deltaTime;

        Vector2 moveInput = input.GetMoveInput();
        float x = moveInput.x;
        float y = moveInput.y;
        bool confirm = input.IsConfirmPressed();
        bool cancel = input.IsCancelPressed();

        if (cooldown <= 0f)
        {
            int newIndex = index;

            //// 水平方向の動き
            //if (playerNum == 1)
            //{
            //    // プレイヤー1: 右から左へ
            //    if (x > 0.5f) newIndex = Mathf.Max(index - 1, 0); // → 左に行く
            //    else if (x < -0.5f) newIndex = Mathf.Min(index + 1, options.Length - 1); // ← 右に行く
            //}
            //else
            //{
            //    // プレイヤー2: 左から右へ
            //    if (x > 0.5f) newIndex = Mathf.Min(index + 1, options.Length - 1);
            //    else if (x < -0.5f) newIndex = Mathf.Max(index - 1, 0);
            //}

            //// 垂直方向に移動（列単位でジャンプ）
            //if (y > 0.5f) newIndex = Mathf.Max(index - columns, 0);
            //else if (y < -0.5f) newIndex = Mathf.Min(index + columns, options.Length - 1);
            
            int rows = Mathf.CeilToInt((float)options.Length / columns);
            int row = index / columns;
            int col = index % columns;

            // 入力方向
            int horizontal = 0;
            int vertical = 0;
            if (x > 0.5f) horizontal = 1;
            else if (x < -0.5f) horizontal = -1;
            if (y > 0.5f) vertical = 1;
            else if (y < -0.5f) vertical = -1;

            // プレイヤー1は左右反転
            if (playerNum == 1) horizontal *= -1;

            col = (col + horizontal + columns) % columns;
            row = (row + vertical + rows) % rows;

            int candidate = row * columns + col;

            // 境界外の防止
            if (candidate >= options.Length)
            {
                // 最後の行がいっぱいでない場合は、
                // 有効な項目が見つかるまで列を左にシフトします
                while (candidate >= options.Length && col > 0)
                {
                    col--;
                    candidate = row * columns + col;
                }

                if (candidate >= options.Length)
                {
                    // まだ無効です。この操作をスキップしてください
                    candidate = index;
                }
            }

            newIndex = candidate;

            if (newIndex != index)
            {
                index = newIndex;
                selector.position = options[index].position;
                cooldown = inputCooldown;
            }
        }

        // 選択内容を確認する
        if (confirm && selections.Count < 3)
        {
            int selectedIndex = index;

            bool isRandom = (index == randomIndex);

            if (isRandom)
            {
                if (previewPrefabs.Length == 0)
                {
                    Debug.LogWarning("ランダム選択できるキャラがいません！");
                    return;
                }

                selectedIndex = Random.Range(0, previewPrefabs.Length); // 0～4
                Debug.Log($"[ランダム選択] → {previewPrefabs[selectedIndex].name}");
            }

            if (selectedIndex < 0 || selectedIndex >= previewPrefabs.Length)
            {
                Debug.LogError($"選択されたインデックス {selectedIndex} が previewPrefabs の範囲外です");
                return;
            }

            selections.Add(selectedIndex);

            // ホバーを正式なインスタンスに変換し、参照をクリアします
            int slotIndex = selections.Count - 1;
            Transform[] slots = playerNum == 1 ? player1Slots : player2Slots;
            GameObject[] instances = playerNum == 1 ? player1PreviewInstances : player2PreviewInstances;
            GameObject hover = playerNum == 1 ? player1HoverPreview : player2HoverPreview;

            GameObject finalPreview = null;

            if (hover != null)
            {
                var cg = hover.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;

                finalPreview = hover;

                if (playerNum == 1) player1HoverPreview = null;
                else player2HoverPreview = null;
            }
            else
            {
                // 極端なケースでのホバー損失を防ぎ、正式なインスタンスを再生成する
                GameObject prefab = previewPrefabs[selectedIndex];
                if (prefab != null && slotIndex < slots.Length)
                {
                    finalPreview = Instantiate(prefab, slots[slotIndex]);
                    finalPreview.transform.localPosition = Vector3.zero;
                }
            }

            instances[slotIndex] = finalPreview;

            // 上位シリアル番号を設定する
            if (finalPreview != null)
            {
                var text = finalPreview.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = (selections.Count).ToString();
            }
        }

        // 選択解除（戻る）
        if (cancel && selections.Count > 0)
        {
            int lastIndex = selections[selections.Count - 1];
            selections.RemoveAt(selections.Count - 1);

            Transform[] slots = playerNum == 1 ? player1Slots : player2Slots;
            GameObject[] instances = playerNum == 1 ? player1PreviewInstances : player2PreviewInstances;
            GameObject hover = playerNum == 1 ? player1HoverPreview : player2HoverPreview;

            int slotIndex = selections.Count;

            // 公式インスタンスを破壊する
            if (slotIndex < slots.Length && instances[slotIndex] != null)
            {
                Destroy(instances[slotIndex]);
                instances[slotIndex] = null;
            }

            // シーケンス番号テキストをクリアする（公式プレビュー）
            if (hover != null)
            {
                var text = hover.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = "";
            }

            // 前の位置に戻る
            selector.position = options[lastIndex].position;
            index = lastIndex;

            // キャンセル後にホバーを更新します（インデックス更新後に配置する必要があります）
            if (playerNum == 1)
            {
                RefreshHoverPreview(1, player1Index, player1Selections, player1Slots, previewPrefabs, ref player1HoverPreview);
            }
            else
            {
                RefreshHoverPreview(2, player2Index, player2Selections, player2Slots, previewPrefabs, ref player2HoverPreview);
            }
        }

        if (playerNum == 1)
        {
            RefreshHoverPreview(
                1,
                player1Index,
                player1Selections,
                player1Slots,
                previewPrefabs,
                ref player1HoverPreview
            );
        }
        else if (playerNum == 2)
        {
            RefreshHoverPreview(
                2,
                player2Index,
                player2Selections,
                player2Slots,
                previewPrefabs,
                ref player2HoverPreview
            );
        }
    }

    void RefreshHoverPreview(
         int playerNum,
         int index,
         List<int> selections,
         Transform[] slots,
         GameObject[] previewPrefabs,
         ref GameObject hoverInstance)
    {
        // 最初に古いホバーを破棄します
        // （新しいプレビューを生成するための条件を満たしているかどうかに関係なく）
        if (hoverInstance != null)
        {
            Destroy(hoverInstance);
            hoverInstance = null;
        }

        // 条件付き制限: 現在の文字が選択されていないか、
        // 選択されていない場合にのみプレビューします
        if (selections.Count >= 3) return;
        if (index < 0 || index >= previewPrefabs.Length) return;

        int slotIndex = selections.Count;
        if (slotIndex >= slots.Length) return;

        // 古いホバーを破壊する
        if (hoverInstance != null)
        {
            Destroy(hoverInstance);
            hoverInstance = null;
        }

        // 新しいホバープレビューを作成する
        GameObject prefab = previewPrefabs[index];
        if (prefab != null)
        {
            hoverInstance = Instantiate(prefab, slots[slotIndex]);
            hoverInstance.transform.localPosition = Vector3.zero;

            // オプション: プレビュー状態を示すために透明度を設定します
            var canvasGroup = hoverInstance.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0.5f; // 半透明
            }
        }
    }

    public int GetPlayer1Selection() => player1Index;
    public int GetPlayer2Selection() => player2Index;
}
