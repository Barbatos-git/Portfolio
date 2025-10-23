using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
public class DamageText : MonoBehaviour
{
    public GameObject digitPrefab;
    public Transform digitParent;
    private CanvasGroup canvasGroup;
    public float digitSpacing = 0.6f;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show(int damage)
    {
        StartCoroutine(PlayDamageSequence(damage));
    }

    private IEnumerator PlayDamageSequence(int damage)
    {
        canvasGroup.alpha = 1;
        string str = "-" + damage.ToString();
        List<Transform> digits = new();

        // 奺悢帤傪嶌惉偡傞
        for (int i = 0; i < str.Length; i++)
        {
            GameObject digitObj = Instantiate(digitPrefab, digitParent);
            var tmp = digitObj.GetComponent<TextMeshProUGUI>();
            tmp.text = str[i].ToString();
            tmp.alpha = 0f;

            digits.Add(digitObj.transform);
        }

        yield return null;

        //偡傋偰偺悢帤偺埵抲傪婰榐偡傞
        List<Vector3> digitPositions = new();
        foreach (var t in digits)
        {
            digitPositions.Add(t.localPosition);
        }

        //僜乕僩偝傟偨埵抲偵儕僙僢僩
        for (int i = 0; i < digits.Count; i++)
        {
            digits[i].localPosition = digitPositions[i];
        }

        // 儗僀傾僂僩傪柍岠偵偟偰丄屻懕偺傾僯儊乕僔儑儞傪桳岠偵偟傑偡
        var layout = digitParent.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
            layout.enabled = false;

        // 奺悢帤傪傾僯儊乕僔儑儞壔偡傞
        Sequence digitSeq = DOTween.Sequence();
        for (int i = 0; i < digits.Count; i++)
        {
            Transform t = digits[i];
            TextMeshProUGUI txt = t.GetComponent<TextMeshProUGUI>();

            digitSeq.AppendCallback(() =>
            {
                txt.alpha = 1f;
                t.localScale = Vector3.zero;

                Sequence bounce = DOTween.Sequence();
                bounce.Append(t.DOScale(1.2f, 0.1f).SetEase(Ease.OutBack));
                bounce.Append(t.DOScale(1f, 0.1f));
                bounce.Join(t.DOLocalMoveY(20f, 0.5f).SetRelative().SetLoops(2, LoopType.Yoyo));
            });

            digitSeq.AppendInterval(0.08f);
        }

        // 嵟屻偵丄慡懱偑晜偐傃忋偑傝丄徚偊偰偄偔
        digitSeq.AppendInterval(0.5f);
        digitSeq.Append(transform.DOLocalMoveY(1.5f, 1f).SetRelative());
        digitSeq.Join(canvasGroup.DOFade(0f, 1f));
        digitSeq.OnComplete(() => Destroy(gameObject));
    }
}
