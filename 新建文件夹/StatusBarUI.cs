using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusBarUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image portraitImage;
    public Image hpBarFill;
    public Image mpBarFill;
    public Image STABarFill;

    public TextMeshProUGUI hpText;
    public TextMeshProUGUI mpText;
    public TextMeshProUGUI STAText;

    [Header("头像 Sprite（按角色 ID 指定）")]
    public Sprite warriorPortrait;
    public Sprite frostMagePortrait;
    public Sprite archerPortrait;
    public Sprite assassinPortrait;
    public Sprite fireMagePortrait;

    private Sprite FindPortraitByID(string id)
    {
        switch (id)
        {
            case "Warrior_Prefab": return warriorPortrait;
            case "FrostMage_Prefab": return frostMagePortrait;
            case "Archer_Prefab": return archerPortrait;
            case "Assassin_Prefab": return assassinPortrait;
            case "FireMage_Prefab": return fireMagePortrait;
            default:
                Debug.LogWarning($"[FindPortraitByID] 未知角色ID: {id}");
                return null;
        }
    }

    public void SetStatus(CharaInfo info, CharaType charaType)
    {
        if (info == null) return;

        // 设置头像（通过 ID 匹配）
        if (portraitImage != null)
        {
            Sprite matchedPortrait = FindPortraitByID(info.id);
            portraitImage.sprite = matchedPortrait;
            if (matchedPortrait == null)
            {
                Debug.LogWarning($"未找到头像资源: {info.id}");
            }
            portraitImage.enabled = true;
        }

        // HP
        if (hpBarFill != null && info.maxHP > 0)
            hpBarFill.fillAmount = (float)info.currentHP / info.maxHP;
        else if (hpBarFill != null)
            hpBarFill.fillAmount = 0f;
        if (hpText != null)
            hpText.text = $"{info.currentHP} / {info.maxHP}";

        // 类型判断：MP 或 STA 显示
        if (charaType == CharaType.Physical)
        {
            if (STABarFill != null)
            {
                STABarFill.gameObject.SetActive(true);
                STABarFill.fillAmount = info.maxSTA > 0 ? (float)info.currentSTA / info.maxSTA : 0f;
            }
            if (STAText != null)
            {
                STAText.gameObject.SetActive(true);
                STAText.text = $"{info.currentSTA} / {info.maxSTA}";
            }

            if (mpBarFill != null) mpBarFill.gameObject.SetActive(false);
            if (mpText != null) mpText.gameObject.SetActive(false);
        }
        else if (charaType == CharaType.Magical)
        {
            if (mpBarFill != null)
            {
                mpBarFill.gameObject.SetActive(true);
                mpBarFill.fillAmount = info.maxMP > 0 ? (float)info.currentMP / info.maxMP : 0f;
            }
            if (mpText != null)
            {
                mpText.gameObject.SetActive(true);
                mpText.text = $"{info.currentMP} / {info.maxMP}";
            }

            if (STABarFill != null) STABarFill.gameObject.SetActive(false);
            if (STAText != null) STAText.gameObject.SetActive(false);
        }
    }
}
