using UnityEngine;
using TMPro;

public class CharaDataUI : MonoBehaviour
{
    [Header("UI Elements")]
    public StatusBarUI statusBar;
    public NumValueBar numValueBar;

    private CharaBase currentChara;

    public void ShowPreview(CharaInfo info, CharaType charaType)
    {
        if (info == null) return;

        // 状態
        if (statusBar != null)
        {
            statusBar.gameObject.SetActive(true);
            statusBar.SetStatus(info, charaType);
        }
        // 値
        if (numValueBar != null)
        {
            numValueBar.gameObject.SetActive(true);
            numValueBar.SetValues(info);
        }   
    }

    public void ShowPreview(CharaBase chara)
    {
        if (chara == null) return;

        currentChara = chara;
        ShowPreview(chara.GetCurrentCharaInfo(), chara.GetCharaType());
    }

    public void Hide()
    {
        if (statusBar != null) statusBar.gameObject.SetActive(false);
        if (numValueBar != null) numValueBar.gameObject.SetActive(false);
    }
}
