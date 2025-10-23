using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UITextBreathEffect : MonoBehaviour
{
    [Header("任意のテキストコンポーネント")]
    public Text uiText;
    public TextMeshProUGUI tmpText;

    [Header("透明度設定")]
    public float fadeMin = 0.3f;
    public float fadeMax = 1f;
    public float fadeDuration = 1.2f;

    [Header("ズーム設定")]
    public float scaleAmount = 1.05f;
    public float scaleDuration = 1.2f;

    void Start()
    {
        // 呼吸アニメーションを有効にする（透明）
        if (uiText != null)
        {
            Color color = uiText.color;
            color.a = fadeMax;
            uiText.color = color;

            uiText.DOFade(fadeMin, fadeDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }
        else if (tmpText != null)
        {
            Color color = tmpText.color;
            color.a = fadeMax;
            tmpText.color = color;

            tmpText.DOFade(fadeMin, fadeDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }

        // 呼吸アニメーション開始（ズーム）
        transform.DOScale(scaleAmount, scaleDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true);
    }
}
