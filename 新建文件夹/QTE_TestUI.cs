using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QTE_TestUI : MonoBehaviour
{
    public QTEExecutor executor;
    public Button archerBtn, mageBtn, assassinBtn;
    public TMP_Dropdown ownerDropdown;

    void Start()
    {
        archerBtn.onClick.AddListener(() =>
        {
            UnitOwner owner = GetSelectedOwner();
            executor.StartCoroutine(executor.ExecuteQTE(QTEType.Archer, UnitOwner.Player1, result =>
            {
                Debug.Log("Archer QTE result: " + result);
            }));
        });

        mageBtn.onClick.AddListener(() =>
        {
            UnitOwner owner = GetSelectedOwner();
            executor.StartCoroutine(executor.ExecuteQTE(QTEType.Mage, UnitOwner.Player1, result =>
            {
                Debug.Log("Mage QTE result: " + result);
            }));
        });

        assassinBtn.onClick.AddListener(() =>
        {
            UnitOwner owner = GetSelectedOwner();
            executor.StartCoroutine(executor.ExecuteQTE(QTEType.Assassin, UnitOwner.Player1, result =>
            {
                Debug.Log("Assassin QTE result: " + result);
            }));
        });
    }

    private UnitOwner GetSelectedOwner()
    {
        return ownerDropdown.value == 0 ? UnitOwner.Player1 : UnitOwner.Player2;
    }
}
