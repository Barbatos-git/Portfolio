using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class TurnBannerUI : MonoBehaviour
{
    public static TurnBannerUI Instance;

    public RectTransform bannerTransform;
    public TextMeshProUGUI bannerText;
    public Image image;

    public Color player1Color = Color.red;
    public Color player2Color = Color.blue;

    public float bannerTime = 0.2f;

    private void Awake()
    {
        Instance = this;
        bannerTransform.gameObject.SetActive(false);
        image.gameObject.SetActive(false);
    }

    public void ShowTurn(UnitOwner owner)
    {
        if (owner == UnitOwner.Player1)
        {
            bannerText.text = "player1 turn";
            bannerText.color = player1Color;
            Color color = player1Color;
            color.a = 0f;
            image.color = color;
        }
        else
        {
            bannerText.text = "player2 turn";
            bannerText.color = player2Color;
            Color color = player2Color;
            color.a = 0f;
            image.color = color;
        }

        bannerTransform.gameObject.SetActive(true);
        image.gameObject.SetActive(true);

        //スライドイン
        bannerTransform.anchoredPosition = new Vector2(-1000, 0); // 開始位置
        Sequence testSeq = DOTween.Sequence();
        testSeq.Append(bannerTransform.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack))  // スライドイン
           .AppendInterval(bannerTime)                                                             // 滞在する
           .Append(bannerTransform.DOAnchorPos(new Vector2(1000, 0), 0.5f).SetEase(Ease.InBack)) // スライドアウト
           .OnComplete(() => bannerTransform.gameObject.SetActive(false));

        //フェードインとフェードアウト
        Sequence imageSeq = DOTween.Sequence();
        imageSeq.Append(image.DOFade(0.5f, 0.5f))  // 50%にフェード
               .AppendInterval(bannerTime * 5f)    // テキストで長く滞在
               .Append(image.DOFade(0f, 0.5f));    // フェードアウト
    }
}
