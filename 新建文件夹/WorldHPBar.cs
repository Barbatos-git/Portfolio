using UnityEngine;
using UnityEngine.UI;

public class WorldHPBar : MonoBehaviour
{
    public Image fillImage;
    private CharaBase target;

    public void Bind(CharaBase chara)
    {
        target = chara;
    }

    void Update()
    {
        if (target == null) return;

        if (fillImage != null)
        {
            fillImage.fillAmount = (float)target.GetCurrentHP() / target.GetMaxHP();
        }
    }
}
