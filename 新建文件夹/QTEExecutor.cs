using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public enum QTERank
{
    Failed,
    Good,
    Great
}

public class QTEExecutor : MonoBehaviour
{
    [Header("Main QTE Root Panel")]
    public GameObject qtePanel;

    [Header("Archer QTE Panel")]
    public GameObject archerPanel;
    public TMP_Text archerKeyText;
    public Image archerTimerImage;
    public Image archerTimer2Image;
    public Image archerCircleImage;

    [Header("Mage QTE Panel")]
    public GameObject magePanel;
    public Image mageImage;
    public Image mageBGImage;

    [Header("Assassin QTE Panel")]
    public GameObject assassinPanel;

    [Header("Result Text")]
    public TMP_Text resultText;

    [Header("QTE Config")]
    public int ArcherNumber;
    public float ArcherTime;

    public int MageNumber;
    public float MageTime;

    public int AssinNumber;
    public float AssinTime;

    [Header("QTE Keys")]
    public KeyCode[] keyboardKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
    private KeyCode[] gamepadKeys = { KeyCode.JoystickButton0, KeyCode.JoystickButton1, KeyCode.JoystickButton2, KeyCode.JoystickButton3 };

    private System.Random rng = new System.Random();
    private ControlScheme currentScheme;
    public bool isover = false;

    public IEnumerator ExecuteQTE(QTEType type, UnitOwner owner, Action<QTERank> onComplete)
    {
        qtePanel.SetActive(true);
        isover = false;
        InputLock.IsLocked = true;

        // 全部先关
        archerPanel.SetActive(false);
        magePanel.SetActive(false);
        assassinPanel.SetActive(false);

        //if (timerImage != null)
        //    timerImage.fillAmount = 1f;

        resultText.text = "";
        //instructionText.text = "";
        //keyIconText.text = "";

        currentScheme = (owner == UnitOwner.Player1)
           ? TurnManager.Instance.player1Input.controlScheme
           : TurnManager.Instance.player2Input.controlScheme;

        KeyCode[] pool = (currentScheme == ControlScheme.Gamepad) ? gamepadKeys : keyboardKeys;

        QTERank resultRank = QTERank.Failed;

        switch (type)
        {
            case QTEType.Archer:
                archerPanel.SetActive(true);
                yield return StartCoroutine(ArcherQTE(ArcherNumber, ArcherTime, pool, rank => resultRank = rank));
                break;
            case QTEType.Mage:
                magePanel.SetActive(true);
                yield return StartCoroutine(MageQTE(MageNumber, MageTime, pool, rank => resultRank = rank));
                break;
            case QTEType.Assassin:
                assassinPanel.SetActive(true);
                yield return StartCoroutine(AssassinQTE(AssinNumber, AssinTime, 0.5f, pool, rank => resultRank = rank));
                break;
        }

        //if (timerImage != null)
        //    timerImage.fillAmount = 0f;      

        qtePanel.SetActive(false);

        yield return StartCoroutine(ShowResultText(resultRank));

        isover = true;
        InputLock.IsLocked = false;
        onComplete?.Invoke(resultRank);
    }

    private IEnumerator ArcherQTE(int targetCount, float timeLimit, KeyCode[] pool, Action<QTERank> onComplete)
    {
        KeyCode key = GetRandomKey(pool);
        //instructionText.text = "連打せよ！";
        archerKeyText.text = GetKeyDisplayName(key);

        // 初始化位置与大小
        Vector3 keyBase = archerKeyText.transform.localScale;
        Vector3 timerBase = archerTimer2Image.transform.localScale;
        Vector3 circleBase = archerCircleImage.transform.localScale;

        float moveY = 5f;
        float duration = 0.25f;

        // 启动持续上下律动动画
        void StartPressLoop()
        {
            DOTween.Kill("ArcherPressLoop");

            Sequence loop = DOTween.Sequence().SetId("ArcherPressLoop");
            loop.Append(MoveGroupY(-moveY, duration));
            loop.Append(MoveGroupY(0f, duration));
            loop.SetLoops(-1);
        }

        Tween MoveGroupY(float y, float dur)
        {
            Sequence s = DOTween.Sequence();
            if (archerKeyText != null)
                s.Join(archerKeyText.rectTransform.DOAnchorPosY(y, dur).SetEase(Ease.InOutSine));
            if (archerTimer2Image != null)
                s.Join(archerTimer2Image.rectTransform.DOAnchorPosY(y, dur).SetEase(Ease.InOutSine));
            if (archerCircleImage != null)
                s.Join(archerCircleImage.rectTransform.DOAnchorPosY(y, dur).SetEase(Ease.InOutSine));
            return s;
        }

        StartPressLoop();

        int count = 0;
        float timer = 0f;

        while (timer < timeLimit)
        {
            if (Input.GetKeyDown(key))
            {
                count++;

                // 停止当前律动动画
                DOTween.Kill("ArcherPressLoop");

                // 所有对象缩放 + 压下反馈
                Sequence pressSeq = DOTween.Sequence();

                void AddPressAnim(Transform t, Vector3 baseScale)
                {
                    if (t == null) return;

                    t.DOKill();

                    Sequence pressSeq = DOTween.Sequence();
                    pressSeq.Append(t.DOScale(baseScale * 0.8f, 0.1f).SetEase(Ease.InQuad));
                    pressSeq.Append(t.DOScale(baseScale * 1.1f, 0.15f).SetEase(Ease.OutBack));
                    pressSeq.Append(t.DOScale(baseScale, 0.1f));
                }

                AddPressAnim(archerKeyText.transform, keyBase);
                if (archerTimer2Image != null) AddPressAnim(archerTimer2Image.transform, timerBase);
                if (archerCircleImage != null) AddPressAnim(archerCircleImage.transform, circleBase);

                pressSeq.OnComplete(() => StartPressLoop());
            }

            timer += Time.deltaTime;

            if (archerTimerImage != null)
                archerTimerImage.fillAmount = Mathf.Clamp01(1f - (timer / timeLimit));

            yield return null;
        }

        // 停止动画
        DOTween.Kill("ArcherPressLoop");

        // 还原状态
        archerKeyText.transform.localScale = keyBase;
        archerKeyText.rectTransform.anchoredPosition = Vector2.zero;
        if (archerTimer2Image != null)
        {
            archerTimer2Image.transform.localScale = timerBase;
            archerTimer2Image.rectTransform.anchoredPosition = Vector2.zero;
        }
        if (archerCircleImage != null)
        {
            archerCircleImage.transform.localScale = circleBase;
            archerCircleImage.rectTransform.anchoredPosition = Vector2.zero;
        }

        QTERank rank = count switch
        {
            >= 8 => QTERank.Great,
            >= 4 => QTERank.Good,
            _ => QTERank.Failed
        };

        onComplete?.Invoke(rank);
    }

    private IEnumerator MageQTE(int length, float timeLimit, KeyCode[] pool, Action<QTERank> onComplete)
    {
        DOTween.Kill("MageQTE");
        DOTween.Kill("MageRotate");

        mageImage.color = new Color(1f, 1f, 1f, 1f);
        mageImage.transform.localScale = Vector3.one * 2.7f;
        mageImage.transform.localEulerAngles = Vector3.zero;
        mageImage.enabled = true;
        mageBGImage.enabled = true;

        Sequence mageSequence = DOTween.Sequence().SetId("MageQTE");

        // 1. 先放大 & 显现
        mageSequence.Append(mageImage.DOFade(1f, 0.3f));
        mageSequence.Join(mageImage.transform.DOScale(mageImage.transform.localScale * 1.1f, 0.3f));

        // 2. 顺时针旋转 360度，持续 timeLimit 秒
        mageSequence.Append(
            mageImage.transform
            .DORotate(new Vector3(0, 0, -180f), timeLimit - 1.5f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear));

        // 3. 最后缩小 & 渐隐
        mageSequence.Append(
            mageImage.transform.DOScale(0.3f, 1.5f).SetEase(Ease.InQuad)
        );
        mageSequence.Join(
            mageImage.DOFade(0f, 1.5f).SetEase(Ease.OutQuad)
        );

        // 自动关闭
        mageSequence.OnComplete(() =>
        {
            mageImage.enabled = false;
            mageBGImage.enabled = false;
        });

        // QTE 开始前彻底重置所有子对象
        for (int i = 0; i < magePanel.transform.childCount; i++)
        {
            Transform child = magePanel.transform.GetChild(i);
            if (child == mageImage.transform || child == mageBGImage.transform)
                continue;

            child.gameObject.SetActive(true);
            DOTween.Kill(child, complete: true);  // 终止并清理该对象所有 tween

            foreach (Graphic g in child.GetComponentsInChildren<Graphic>(true))
            {
                Color c = g.color;
                c.a = 1f;
                g.color = c;
            }

            if (child.Find("Timer2Image"))
                child.Find("Timer2Image").localScale = new Vector3(0.69f, 0.69f, 0.69f);
            if (child.Find("CircleImage"))
                child.Find("CircleImage").localScale = new Vector3(0.55f, 0.55f, 0.55f);
            if (child.Find("KeysText"))
                child.Find("KeysText").localScale = Vector3.one;
        }

        // 4个方向的槽对象
        List<Transform> slots = new List<Transform>();
        for (int i = 0; i < magePanel.transform.childCount; i++)
        {
            Transform child = magePanel.transform.GetChild(i);
            if (child == mageImage.transform || child == mageBGImage.transform)
                continue;
            slots.Add(child);
        }

        // 按顺时针方向依次按下
        KeyCode[] sequence = GetRandomKeySequence(slots.Count, pool);
        int successCount = 0;
        float timer = 0f;
        bool[] pressed = new bool[slots.Count];
        int currentIndex = -1;

        // 预初始化所有槽
        for (int i = 0; i < slots.Count; i++)
        {
            Transform slot = slots[i];
            slot.gameObject.SetActive(true);
            DOTween.Kill(slot);

            TMP_Text keyText = slot.GetComponentInChildren<TMP_Text>(true);
            Image timerImg = slot.Find("Timer2Image")?.GetComponent<Image>();
            Image circleImg = slot.Find("CircleImage")?.GetComponent<Image>();

            KeyCode key = sequence[i];
            keyText.text = GetKeyDisplayName(key);
            foreach (Graphic g in slot.GetComponentsInChildren<Graphic>(true))
            {
                Color c = g.color; c.a = 1f; g.color = c;
            }
        }

        //instructionText.text = "順番に押して！";
        //sequenceText.text = "";

        //foreach (var key in sequence)
        //sequenceText.text += key.ToString() + " ";

        //mageKeyText.text = GetKeyDisplayName(sequence[index]);

        while (timer < timeLimit)
        {
            timer += Time.deltaTime;

            // 检测玩家输入
            foreach (KeyCode key in pool)
            {
                if (Input.GetKeyDown(key))
                {
                    // 找出匹配的槽
                    int targetIndex = -1;
                    for (int j = 0; j < slots.Count; j++)
                    {
                        if (pressed[j]) continue; // 已按过的跳过
                        TMP_Text txt = slots[j].GetComponentInChildren<TMP_Text>(true);
                        if (txt.text == GetKeyDisplayName(key))
                        {
                            targetIndex = j;
                            break;
                        }
                    }

                    if (targetIndex == -1) continue;

                    // 第一次按 → 确定起点
                    if (currentIndex == -1)
                    {
                        currentIndex = targetIndex;
                    }
                    else
                    {
                        // 必须顺时针按下一个
                        int expectedNext = (currentIndex + 1) % slots.Count;
                        if (targetIndex != expectedNext) continue;
                        currentIndex = targetIndex;
                    }

                    // 执行按压动画
                    Transform slot = slots[targetIndex];
                    TMP_Text keyText = slot.GetComponentInChildren<TMP_Text>(true);
                    Image timer2Img = slot.Find("Timer2Image")?.GetComponent<Image>();
                    Image circleImg = slot.Find("CircleImage")?.GetComponent<Image>();

                    Vector3 keyBase = keyText.transform.localScale;
                    Vector3 timer2Base = timer2Img ? timer2Img.transform.localScale : Vector3.one * 0.69f;
                    Vector3 circleBase = circleImg ? circleImg.transform.localScale : Vector3.one * 0.55f;

                    Sequence pressSeq = DOTween.Sequence();

                    void AddPressAnim(Graphic g, Transform t, Vector3 baseScale)
                    {
                        if (g == null || t == null) return;
                        g.DOKill(); t.DOKill();
                        pressSeq.Join(t.DOScale(baseScale * 0.8f, 0.1f).SetEase(Ease.InQuad));
                        pressSeq.Join(t.DOScale(baseScale * 1.1f, 0.15f).SetEase(Ease.OutBack));
                        pressSeq.Join(t.DOScale(baseScale, 0.1f));
                        pressSeq.Join(g.DOFade(0f, 0.3f).SetEase(Ease.OutQuad));
                    }

                    AddPressAnim(keyText, keyText.transform, keyBase);
                    AddPressAnim(timer2Img, timer2Img?.transform, timer2Base);
                    AddPressAnim(circleImg, circleImg?.transform, circleBase);

                    pressSeq.OnComplete(() =>
                    {
                        slot.gameObject.SetActive(false);
                        pressed[targetIndex] = true;
                    });

                    successCount++;
                }
            }

            yield return null;
        }

        // 超时后未按的槽渐隐消失
        for (int i = 0; i < slots.Count; i++)
        {
            if (!pressed[i])
            {
                Transform slot = slots[i];
                slot.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                    .OnComplete(() => slot.gameObject.SetActive(false));
            }
        }

        DOTween.Kill("MageQTE");
        mageImage.enabled = false;
        mageBGImage.enabled = false;
        foreach (Transform slot in slots)
        {
            slot.localScale = Vector3.one;
            slot.gameObject.SetActive(false);
        }

        QTERank rank = successCount switch
        {
            4 => QTERank.Great,
            2 or 3 => QTERank.Good,
            _ => QTERank.Failed
        };

        DOTween.Kill("MageRotate");
        DOTween.Kill("MageQTE");

        foreach (Transform slot in slots)
        {
            DOTween.Kill(slot);
            slot.localScale = Vector3.one;
            slot.gameObject.SetActive(false);

            foreach (Graphic g in slot.GetComponentsInChildren<Graphic>(true))
            {
                Color c = g.color;
                c.a = 1f;
                g.color = c;
            }
        }

        onComplete?.Invoke(rank);
    }

    private IEnumerator AssassinQTE(int rounds, float initialTime, float decay, KeyCode[] pool, Action<QTERank> onComplete)
    {
        //instructionText.text = "素早く押せ！";

        // 先隐藏所有 Slot
        for (int i = 0; i < assassinPanel.transform.childCount; i++)
            assassinPanel.transform.GetChild(i).gameObject.SetActive(false);

        int successCount = 0;
        // 记录原始缩放
        //Vector3 baseScale = assassinKeyText.transform.localScale;

        for (int i = 0; i < rounds; i++)
        {
            if (i >= assassinPanel.transform.childCount)
                break; // 防止 rounds 超出 slot 数量

            Transform slot = assassinPanel.transform.GetChild(i);
            slot.gameObject.SetActive(true);

            // 新增复位
            DOTween.Kill(slot);
            foreach (Graphic g in slot.GetComponentsInChildren<Graphic>(true))
            {
                Color c = g.color;
                c.a = 1f;
                g.color = c;
            }

            // 找出各组件
            TMP_Text keyText = slot.GetComponentInChildren<TMP_Text>(true);
            Image timerImg = slot.Find("TimerImage").GetComponent<Image>();
            Image timer2Img = slot.Find("Timer2Image").GetComponent<Image>();
            Image circleImg = slot.Find("CircleImage").GetComponent<Image>();

            KeyCode key = GetRandomKey(pool);
            keyText.text = GetKeyDisplayName(key);

            // 初始化
            Vector3 keyBase = keyText.transform.localScale;
            Vector3 tBase = timerImg.transform.localScale;
            Vector3 t2Base = timer2Img.transform.localScale;
            Vector3 cBase = circleImg.transform.localScale;
            float timeLimit = Mathf.Max(0.5f, initialTime - i * decay);
            float timer = 0f;
            bool pressed = false;
            string loopId = $"AssassinLoop_Slot{i}_{Time.frameCount}";

            //assassinKeyText.text = GetKeyDisplayName(key);
            //sequenceText.text = $"残り時間: {timeLimit:F1}s";

            // 重置计时条
            if (timerImg != null)
                timerImg.fillAmount = 1f;

            while (timer < timeLimit)
            {
                if (Input.GetKeyDown(key))
                {
                    pressed = true;
                    successCount++;

                    DOTween.Kill(loopId);

                    // 每次按下时先重置缩放，防止叠加
                    keyText.transform.localScale = keyBase;
                    timer2Img.transform.localScale = t2Base;
                    circleImg.transform.localScale = cBase;
                    timerImg.transform.localScale = tBase;

                    // 按压反馈动画
                    Sequence pressSeq = DOTween.Sequence();

                    void AddPressAnim(Graphic g, Transform t, Vector3 baseScale)
                    {
                        if (g == null || t == null) return;
                        g.DOKill();
                        t.DOKill();

                        // 缩放回弹
                        pressSeq.Join(t.DOScale(baseScale * 0.8f, 0.1f).SetEase(Ease.InQuad));
                        pressSeq.Join(t.DOScale(baseScale * 1.1f, 0.15f).SetEase(Ease.OutBack));
                        pressSeq.Join(t.DOScale(baseScale, 0.1f));

                        // 淡出
                        pressSeq.Join(g.DOFade(0f, 0.3f).SetEase(Ease.OutQuad));
                    }

                    AddPressAnim(keyText, keyText.transform, keyBase);
                    AddPressAnim(timer2Img, timer2Img.transform, t2Base);
                    AddPressAnim(circleImg, circleImg.transform, cBase);
                    AddPressAnim(timerImg, timerImg.transform, tBase);

                    pressSeq.OnComplete(() =>
                    {
                        // 淡出消失
                        slot.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
                            .OnComplete(() => slot.gameObject.SetActive(false));

                        // 还原透明度（供下次使用）
                        if (keyText != null) keyText.color = new Color(keyText.color.r, keyText.color.g, keyText.color.b, 1);
                        if (timerImg != null) timerImg.color = new Color(timerImg.color.r, timerImg.color.g, timerImg.color.b, 1);
                        if (timer2Img != null) timer2Img.color = new Color(timer2Img.color.r, timer2Img.color.g, timer2Img.color.b, 1);
                        if (circleImg != null) circleImg.color = new Color(circleImg.color.r, circleImg.color.g, circleImg.color.b, 1);

                        keyText.transform.localScale = keyBase;
                        timer2Img.transform.localScale = t2Base;
                        circleImg.transform.localScale = cBase;
                        timerImg.transform.localScale = tBase;
                    });


                    break;
                }
                timer += Time.deltaTime;

                if (timerImg != null)
                    timerImg.fillAmount = Mathf.Clamp01(1f - (timer / timeLimit));

                //sequenceText.text = $"残り時間: {Mathf.Max(0f, timeLimit - timer):F1}s";
                yield return null;
            }

            if (!pressed)
            {
                // 没按中也淡出隐藏
                DOTween.Kill($"AssassinLoop{i}");
                slot.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
                    .OnComplete(() => slot.gameObject.SetActive(false));
            }

            yield return new WaitForSeconds(0.3f);
        }

        // 重置
        for (int i = 0; i < assassinPanel.transform.childCount; i++)
        {
            Transform slot = assassinPanel.transform.GetChild(i);
            slot.localScale = Vector3.one;
            slot.gameObject.SetActive(false);
            DOTween.Kill(slot);
        }

        //keyIconText.text = "";

        QTERank rank = successCount switch
        {
            3 => QTERank.Great,
            2 => QTERank.Good,
            _ => QTERank.Failed
        };

        onComplete?.Invoke(rank);
    }

    private IEnumerator ShowResultText(QTERank rank)
    {
        string result = rank switch
        {
            QTERank.Failed => "FAILED",
            QTERank.Good => "GOOD!",
            QTERank.Great => "GREAT!!",
            _ => "?"
        };

        Color displayColor = rank switch
        {
            QTERank.Failed => new Color(1f, 0.2f, 0.2f, 0),
            QTERank.Good => new Color(1f, 1f, 0.3f, 0),
            QTERank.Great => new Color(0.3f, 1f, 0.3f, 0),
            _ => Color.white
        };

        resultText.text = result;
        displayColor.a = 0f;
        resultText.color = displayColor;

        resultText.transform.localScale = Vector3.zero;

        Sequence s = DOTween.Sequence();
        s.Append(resultText.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
        s.Join(resultText.DOFade(1f, 0.2f));
        s.AppendInterval(1f);
        s.Append(resultText.DOFade(0f, 0.4f));
        s.Join(resultText.transform.DOScale(0.8f, 0.4f));
        s.OnComplete(() => resultText.text = "");

        yield return s.WaitForCompletion();
    }

    private KeyCode GetRandomKey(KeyCode[] pool)
    {
        return pool[rng.Next(pool.Length)];
    }

    private KeyCode[] GetRandomKeySequence(int length, KeyCode[] pool)
    {
        KeyCode[] seq = new KeyCode[length];
        for (int i = 0; i < length; i++)
            seq[i] = pool[rng.Next(pool.Length)];
        return seq;
    }

    private string GetKeyDisplayName(KeyCode key)
    {
        if (currentScheme == ControlScheme.Gamepad)
        {
            return key switch
            {
                KeyCode.JoystickButton0 => "A",
                KeyCode.JoystickButton1 => "B",
                KeyCode.JoystickButton2 => "X",
                KeyCode.JoystickButton3 => "Y",
                KeyCode.JoystickButton4 => "LB",
                KeyCode.JoystickButton5 => "RB",
                KeyCode.JoystickButton6 => "Back",
                KeyCode.JoystickButton7 => "Start",
                KeyCode.JoystickButton8 => "LS",
                KeyCode.JoystickButton9 => "RS",
                _ => "?"
            };
        }
        else
        {
            return key.ToString(); // 键盘时显示 W/A/S/D
        }
    }
}
