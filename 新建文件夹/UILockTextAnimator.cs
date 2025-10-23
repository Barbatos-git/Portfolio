using UnityEngine;
using TMPro;
using DG.Tweening;

public static class UILockTextAnimator
{
    public static void AnimateLockIn(TextMeshProUGUI targetText, int playerNum)
    {
        if (targetText == null) return;

        RectTransform rect = targetText.GetComponent<RectTransform>();
        if (rect == null) return;

        float startX = playerNum == 1 ? -600f : 600f;
        float endX = playerNum == 1 ? 200f : -200f;

        rect.anchoredPosition = new Vector2(startX, 0);
        rect.localScale = Vector3.one;
        targetText.alpha = 0f;
        targetText.text = playerNum == 1 ? "P1ロック!" : "P2ロック!";
        targetText.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOAnchorPosX(endX, 0.4f).SetEase(Ease.OutExpo));
        seq.Join(targetText.DOFade(1f, 0.3f));
        seq.Join(rect.DOScale(1.1f, 0.2f).SetLoops(2, LoopType.Yoyo));
        seq.SetUpdate(true);
    }

    public static void AnimateLockOut(TextMeshProUGUI targetText, int playerNum)
    {
        if (targetText == null) return;
        RectTransform rect = targetText.GetComponent<RectTransform>();
        if (rect == null) return;

        float exitX = playerNum == 1 ? -600f : 600f;

        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOAnchorPosX(exitX, 0.3f).SetEase(Ease.InBack));
        seq.Join(targetText.DOFade(0f, 0.25f));
        seq.OnComplete(() => targetText.gameObject.SetActive(false));
        seq.SetUpdate(true);
    }
}
