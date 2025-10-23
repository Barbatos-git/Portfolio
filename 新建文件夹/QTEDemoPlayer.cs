using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class QTEDemoPlayer : MonoBehaviour
{
    [Header("QTE Panels (Drag from scene)")]
    [SerializeField] private GameObject archerPanel;
    [SerializeField] private GameObject magePanel;
    [SerializeField] private GameObject assassinPanel;

    [Header("Auto Rotate Highlight")]
    [SerializeField] private float highlightInterval = 4f; // 自動切り替え間隔（秒）
    private int currentHighlightIndex = 0; // 現在のハイライトタイプ

    private Coroutine archerCo, mageCo, assassinCo, highlightCo;

    private const string ID_Archer = "QTEDemo/Archer";
    private const string ID_MageRotate = "QTEDemo/MageRotate";
    private const string ID_Mage = "QTEDemo/Mage";
    private const string ID_Assassin = "QTEDemo/Assassin";

    private readonly Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();
    private readonly Dictionary<Graphic, float> originalAlphas = new Dictionary<Graphic, float>();

    private void Awake()
    {
        CacheOriginalStates();
    }

    private void OnEnable()
    {
        ResetAllPanels();
        // UI 初期化の競合を避けるために、アニメーション コルーチンの開始を 0.1 秒遅らせます。
        StartCoroutine(DelayedStartDemos());
    }

    private IEnumerator DelayedStartDemos()
    {
        yield return new WaitForSeconds(0.1f);
        StartAllDemos();
        highlightCo = StartCoroutine(AutoHighlightLoop());

        // 子オブジェクトをすぐに表示することを確認する
        if (magePanel)
        {
            for (int i = 0; i < magePanel.transform.childCount; i++)
            {
                Transform child = magePanel.transform.GetChild(i);
                if (child.name == "MageImage" || child.name == "MageBGImage") continue;
                child.gameObject.SetActive(true);
            }
        }
    }

    private void OnDisable()
    {
        StopAllDemos();
        KillAllTweensScoped();
        if (assassinPanel)
        {
            for (int i = 0; i < assassinPanel.transform.childCount; i++)
            {
                Transform child = assassinPanel.transform.GetChild(i);
                child.gameObject.SetActive(false);
            }
        }
        if (magePanel)
        {
            for (int i = 0; i < magePanel.transform.childCount; i++)
            {
                Transform child = magePanel.transform.GetChild(i);
                if (child.name == "MageImage" || child.name == "MageBGImage") continue;
                child.gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        OnDisable();
    }

    // 初期化とクリーンアップ
    private void CacheOriginalStates()
    {
        originalScales.Clear();
        originalAlphas.Clear();

        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (!originalScales.ContainsKey(t))
                originalScales[t] = t.localScale;
        }

        foreach (Graphic g in GetComponentsInChildren<Graphic>(true))
        {
            if (!originalAlphas.ContainsKey(g))
                originalAlphas[g] = g.color.a;
        }
    }

    private void ResetAllPanels()
    {
        ResetPanelGraphics(archerPanel?.transform);
        ResetPanelGraphics(magePanel?.transform);
        ResetPanelGraphics(assassinPanel?.transform);
    }

    private void ResetPanelGraphics(Transform root)
    {
        if (root == null) return;
        foreach (var g in root.GetComponentsInChildren<Graphic>(true))
        {
            Color c = g.color;
            if (originalAlphas.ContainsKey(g))
                c.a = originalAlphas[g]; 
            else
                c.a = 1f; 
            g.color = c;
        }
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (originalScales.ContainsKey(t))
                t.localScale = originalScales[t];
        }

        // 透明な背景を強制する
        CanvasGroup cg = root.GetComponent<CanvasGroup>();
        if (cg)
        {
            cg.alpha = 1f; // コンテンツは正常に表示されます
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
    }

    private void KillAllTweensScoped()
    {
        DOTween.Kill(ID_Archer);
        DOTween.Kill(ID_MageRotate);
        DOTween.Kill(ID_Mage);
        DOTween.Kill(ID_Assassin);
    }

    private void StartAllDemos()
    {
        archerCo = StartCoroutine(PlayArcherDemoLoop());
        mageCo = StartCoroutine(PlayMageDemoLoop());
        assassinCo = StartCoroutine(PlayAssassinDemoLoop());
    }

    private void StopAllDemos()
    {
        if (archerCo != null) StopCoroutine(archerCo);
        if (mageCo != null) StopCoroutine(mageCo);
        if (assassinCo != null) StopCoroutine(assassinCo);
        if (highlightCo != null) StopCoroutine(highlightCo);
    }

    // Archer Demo
    private IEnumerator PlayArcherDemoLoop()
    {
        TMP_Text keyText = archerPanel.GetComponentInChildren<TMP_Text>(true);
        Image timerImg = archerPanel.transform.Find("TimerImage")?.GetComponent<Image>();
        Image timer2Img = archerPanel.transform.Find("Timer2Image")?.GetComponent<Image>();
        Image circleImg = archerPanel.transform.Find("CircleImage")?.GetComponent<Image>();

        Vector3 keyBase = GetBaseScale(keyText.transform);
        Vector3 t2Base = timer2Img ? GetBaseScale(timer2Img.transform) : Vector3.one;
        Vector3 cBase = circleImg ? GetBaseScale(circleImg.transform) : Vector3.one;

        float fakeTimeLimit = 4f;

        while (true)
        {
            float timer = 0f;

            if (timerImg)
                timerImg.fillAmount = 1f;

            while (timer < fakeTimeLimit)
            {
                timer += Time.deltaTime;

                // シミュレーションカウントダウン
                if (timerImg)
                    timerImg.fillAmount = Mathf.Clamp01(1f - (timer / fakeTimeLimit));

                if (Mathf.Repeat(timer, 0.4f) < Time.deltaTime)
                {
                    Sequence seq = DOTween.Sequence().SetId(ID_Archer);

                    void Pulse(Transform t, Vector3 baseScale)
                    {
                        seq.Join(t.DOScale(baseScale * 0.8f, 0.1f).SetEase(Ease.InQuad));
                        seq.Join(t.DOScale(baseScale * 1.1f, 0.15f).SetEase(Ease.OutBack));
                        seq.Join(t.DOScale(baseScale, 0.1f));
                    }

                    Pulse(keyText.transform, keyBase);
                    if (timer2Img) Pulse(timer2Img.transform, t2Base);
                    if (circleImg)
                        seq.Join(circleImg.DOFade(0.3f, 0.1f).SetLoops(2, LoopType.Yoyo));
                }
                yield return null;
            }

            // リセット
            if (timerImg) timerImg.fillAmount = 1f;
        }
    }

    // Mage Demo
    private IEnumerator PlayMageDemoLoop()
    {
        Image mageImage = magePanel.transform.Find("MageImage")?.GetComponent<Image>();
        Image mageBG = magePanel.transform.Find("MageBGImage")?.GetComponent<Image>();

        if (mageImage)
        {
            mageImage.enabled = true;
            mageImage.color = Color.white;
            mageImage.transform.DORotate(new Vector3(0, 0, -360f), 6f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1)
                .SetId(ID_MageRotate);
        }

        List<Transform> slots = new List<Transform>();
        for (int i = 0; i < magePanel.transform.childCount; i++)
        {
            Transform child = magePanel.transform.GetChild(i);
            if (child == mageImage?.transform || child == mageBG?.transform) continue;
            slots.Add(child);
        }

        while (true)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                Transform slot = slots[i];
                TMP_Text keyText = slot.GetComponentInChildren<TMP_Text>(true);
                Image timer2Img = slot.Find("Timer2Image")?.GetComponent<Image>();
                Image circleImg = slot.Find("CircleImage")?.GetComponent<Image>();

                Vector3 kBase = GetBaseScale(keyText.transform);
                Vector3 t2Base = timer2Img ? GetBaseScale(timer2Img.transform) : Vector3.one;
                Vector3 cBase = circleImg ? GetBaseScale(circleImg.transform) : Vector3.one;

                Sequence seq = DOTween.Sequence().SetId(ID_Mage);

                void AddPressAnim(Graphic g, Transform t, Vector3 baseScale)
                {
                    if (g == null || t == null) return;
                    g.DOKill(); t.DOKill();
                    seq.Join(t.DOScale(baseScale * 0.8f, 0.1f).SetEase(Ease.InQuad));
                    seq.Join(t.DOScale(baseScale * 1.1f, 0.15f).SetEase(Ease.OutBack));
                    seq.Join(t.DOScale(baseScale, 0.1f));
                    seq.Join(g.DOFade(0f, 0.3f).SetEase(Ease.OutQuad));
                }

                AddPressAnim(keyText, keyText.transform, kBase);
                AddPressAnim(timer2Img, timer2Img?.transform, t2Base);
                AddPressAnim(circleImg, circleImg?.transform, cBase);

                seq.OnComplete(() =>
                {
                    slot.gameObject.SetActive(false);
                });

                yield return new WaitForSeconds(0.6f);
            }

            // ラウンド終了: 待機 + リプレイ
            yield return new WaitForSeconds(1f);
            yield return null;

            // 現在のパネルのハイライト比率を取得します
            float factor = GetPanelHighlightFactor(magePanel);

            foreach (Transform slot in slots)
            {
                slot.localScale = Vector3.one;
                slot.gameObject.SetActive(true);

                foreach (Graphic g in slot.GetComponentsInChildren<Graphic>(true))
                {
                    if (g == null) continue;
                    if (originalAlphas.TryGetValue(g, out float baseAlpha))
                    {
                        Color c = g.color;
                        c.a = Mathf.Clamp01(baseAlpha * factor);
                        g.color = c;
                    }
                }
            }
        }
    }

    // Assassin Demo
    private IEnumerator PlayAssassinDemoLoop()
    {
        List<Transform> slots = new List<Transform>();
        for (int i = 0; i < assassinPanel.transform.childCount; i++)
            slots.Add(assassinPanel.transform.GetChild(i));

        while (true)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                Transform slot = slots[i];
                slot.gameObject.SetActive(true);

                TMP_Text keyText = slot.GetComponentInChildren<TMP_Text>(true);
                Image timerImg = slot.Find("TimerImage")?.GetComponent<Image>();
                Image timer2Img = slot.Find("Timer2Image")?.GetComponent<Image>();
                Image circleImg = slot.Find("CircleImage")?.GetComponent<Image>();

                Vector3 kBase = GetBaseScale(keyText.transform);
                Vector3 tBase = timerImg ? GetBaseScale(timerImg.transform) : Vector3.one;
                Vector3 t2Base = timer2Img ? GetBaseScale(timer2Img.transform) : Vector3.one;
                Vector3 cBase = circleImg ? GetBaseScale(circleImg.transform) : Vector3.one;

                // 最初に表示されるときに小さなポップイン効果を作成します
                slot.localScale = Vector3.zero;
                slot.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
                yield return new WaitForSeconds(0.2f); // 間隔

                // カウントダウンステージ
                float fakeTimeLimit = 1.2f; // シミュレーション反応時間
                float timer = 0f;

                if (timerImg)
                    timerImg.fillAmount = 1f;

                while (timer < fakeTimeLimit)
                {
                    timer += Time.deltaTime;
                    if (timerImg)
                        timerImg.fillAmount = Mathf.Clamp01(1f - timer / fakeTimeLimit);
                    yield return null;
                }

                Sequence seq = DOTween.Sequence().SetId(ID_Assassin);

                void AddPressAnim(Graphic g, Transform t, Vector3 baseScale)
                {
                    if (g == null || t == null) return;
                    g.DOKill(); t.DOKill();
                    seq.Join(t.DOScale(baseScale * 0.8f, 0.1f).SetEase(Ease.InQuad));
                    seq.Join(t.DOScale(baseScale * 1.1f, 0.15f).SetEase(Ease.OutBack));
                    seq.Join(t.DOScale(baseScale, 0.1f));
                    seq.Join(g.DOFade(0f, 0.3f).SetEase(Ease.OutQuad));
                }

                AddPressAnim(keyText, keyText.transform, kBase);
                AddPressAnim(timer2Img, timer2Img?.transform, t2Base);
                AddPressAnim(circleImg, circleImg?.transform, cBase);
                AddPressAnim(timerImg, timerImg?.transform, tBase);

                // 押した後にスロットを非表示にする
                seq.OnComplete(() =>
                {
                    slot.gameObject.SetActive(false);
                    if (timerImg) timerImg.fillAmount = 1f; // 次のサイクルのために塗りをリセットする
                });

                yield return new WaitForSeconds(0.6f); // 各スロットが押された後の間隔
            }

            // すべて完了したら、少し休憩してから再度実行します
            yield return new WaitForSeconds(1f);

            // すべてのスロットを初期状態にリセットします
            foreach (Transform slot in slots)
            {
                slot.localScale = Vector3.one;
                slot.gameObject.SetActive(false);
                Image timerImg = slot.Find("TimerImage")?.GetComponent<Image>();
                if (timerImg) timerImg.fillAmount = 1f;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    // カルーセルハイライトの自動切り替え
    private IEnumerator AutoHighlightLoop()
    {
        CanvasGroup[] groups = new CanvasGroup[3];
        groups[0] = archerPanel.GetComponent<CanvasGroup>();
        groups[1] = assassinPanel.GetComponent<CanvasGroup>();
        groups[2] = magePanel.GetComponent<CanvasGroup>();

        for (int i = 0; i < 3; i++)
        {
            float alphaFactor = (i == currentHighlightIndex) ? 1f : 0.5f;
            groups[i].alpha = 1f; // CanvasGroupを1のままにして、内部グラフィックの透明度のみを変更します
            ApplyPanelAlphaFactor(GetPanelByIndex(i), alphaFactor);
        }

        while (true)
        {
            yield return new WaitForSeconds(highlightInterval);

            int next = (currentHighlightIndex + 1) % 3;
            // アニメーショントランジション： 1 → 0.5
            StartCoroutine(SmoothPanelAlphaFactor(GetPanelByIndex(currentHighlightIndex), 1f, 0.5f, 0.5f));
            //  0.5 → 1
            StartCoroutine(SmoothPanelAlphaFactor(GetPanelByIndex(next), 0.5f, 1f, 0.5f));
            currentHighlightIndex = next;
        }
    }

    // すべての子要素の透明度をスムーズに遷移します
    private IEnumerator SmoothPanelAlphaFactor(GameObject panel, float factorStart, float factorEnd, float duration)
    {
        if (!panel) yield break;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float currentFactor = Mathf.Lerp(factorStart, factorEnd, t);
            ApplyPanelAlphaFactor(panel, currentFactor);
            yield return null;
        }
        ApplyPanelAlphaFactor(panel, factorEnd);
    }

    // パネル上のすべての画像とテキストの透明度を「元の透明度 × 係数」に設定します
    private void ApplyPanelAlphaFactor(GameObject panel, float factor)
    {
        if (!panel) return;
        foreach (Graphic g in panel.GetComponentsInChildren<Graphic>(true))
        {
            if (originalAlphas.TryGetValue(g, out float baseAlpha))
            {
                Color c = g.color;
                c.a = Mathf.Clamp01(baseAlpha * factor);
                g.color = c;
            }
        }
    }

    private GameObject GetPanelByIndex(int index)
    {
        switch (index)
        {
            case 0: return archerPanel;
            case 1: return assassinPanel;
            case 2: return magePanel;
            default: return null;
        }
    }

    // ツール
    private Vector3 GetBaseScale(Transform t)
    {
        if (t == null) return Vector3.one;
        return originalScales.ContainsKey(t) ? originalScales[t] : t.localScale;
    }

    private float GetPanelHighlightFactor(GameObject panel)
    {
        if (panel == GetPanelByIndex(currentHighlightIndex))
            return 1f;   // 現在ハイライトされているパネル
        else
            return 0.5f; // 非ハイライトパネル
    }
}
