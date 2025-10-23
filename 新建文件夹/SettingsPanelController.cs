using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using DG.Tweening;

public class SettingsPanelController : MonoBehaviour
{
    [Header("説明ページ集（Page1/2=操作説明、Page3=QTE説明など)")]
    public List<GameObject> pages = new List<GameObject>();
    public TMPro.TextMeshProUGUI pageIndicatorText; // ページ番号表示「1 / N」
    public GameObject settingsPanel;

    [Header("滑动动画设置")]
    public float slideDistance = 1920f;  // ページ幅
    public float slideDuration = 0.4f;   // スライドタイム
    public Ease slideEase = Ease.OutQuad;

    private int currentPage = 0;
    private bool isAnimating = false;

    void OnEnable()
    {
        currentPage = 0;
        ShowPageInstant(currentPage);
    }

    void Update()
    {
        if (!settingsPanel.activeInHierarchy) return;
        if (pages.Count <= 1) return;

        // 次のページ: キーボード →
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            NextPage();
        }
        // 前のページ: キーボード ←
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            PreviousPage();
        }

        // コントローラー
        if (Gamepad.current != null)
        {
            if (Gamepad.current.rightShoulder.wasPressedThisFrame)
                NextPage();
            if (Gamepad.current.leftShoulder.wasPressedThisFrame)
                PreviousPage();
        }
    }

    public void NextPage()
    {
        int next = (currentPage + 1) % pages.Count;
        SlidePage(next, +1);
    }

    public void PreviousPage()
    {
        int prev = (currentPage - 1 + pages.Count) % pages.Count;
        SlidePage(prev, -1);
    }

    void SlidePage(int targetIndex, int direction)
    {
        if (isAnimating) return;
        isAnimating = true;

        RectTransform current = pages[currentPage].GetComponent<RectTransform>();
        RectTransform next = pages[targetIndex].GetComponent<RectTransform>();
        next.gameObject.SetActive(true);

        // 新しいページは右（または左）から入ります
        next.anchoredPosition = new Vector2(direction * slideDistance, 0);

        // 2ページを同時に移動する
        Sequence seq = DOTween.Sequence();
        seq.Join(current.DOAnchorPos(new Vector2(-direction * slideDistance, 0), slideDuration).SetEase(slideEase).SetUpdate(true));
        seq.Join(next.DOAnchorPos(Vector2.zero, slideDuration).SetEase(slideEase).SetUpdate(true));

        seq.OnComplete(() =>
        {
            current.gameObject.SetActive(false);
            currentPage = targetIndex;
            isAnimating = false;
            UpdatePageIndicator();
        });
    }

    void ShowPageInstant(int index)
    {
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == index);
                pages[i].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
        UpdatePageIndicator();
    }

    void UpdatePageIndicator()
    {
        if (pageIndicatorText != null)
            pageIndicatorText.text = $"{currentPage + 1} / {pages.Count}";
    }
}
