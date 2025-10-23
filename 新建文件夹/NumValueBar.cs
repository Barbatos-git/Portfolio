using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumValueBar : MonoBehaviour
{
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI defText;
    public TextMeshProUGUI atsText;
    public TextMeshProUGUI adfText;
    public TextMeshProUGUI movText;

    public void SetValues(CharaInfo info)
    {
        if (info == null) return;

        if (atkText != null) atkText.text = $"Atk: {info.atk}";
        if (defText != null) defText.text = $"Def: {info.def}";
        if (atsText != null) atsText.text = $"Ats: {info.ats}";
        if (adfText != null) adfText.text = $"Adf: {info.adf}";
        if (movText != null) movText.text = $"Mov: {info.mov}";
    }
}
