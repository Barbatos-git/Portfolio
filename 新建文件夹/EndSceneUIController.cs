using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class EndSceneUIController : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI text;
    public Transform[] charaSlots;

    [System.Serializable]
    public class CharaIDAndPrefab
    {
        public string charaID;
        public GameObject prefab;
    }

    [Header("UI Prefab Mapping")]
    public List<CharaIDAndPrefab> charaIDPrefabs;

    public Button mainmenuButton;
    public Button quitButton;
    public Button mainFirstSelectedButton;

    void Start()
    {
        SetupResultUI();
        SetupButtons();
    }

    void SetupResultUI()
    {
        var result = GameResultManager.Instance;
        if (result == null)
        {
            resultText.text = "No Result";
            text.text = resultText.text;
            return;
        }

        resultText.text = result.winner == UnitOwner.Player1 ? "Player1 Win" : "Player2 Win";
        text.text = resultText.text;

        // 从 PlayerDeployData 获取胜者的角色列表
        List<DeployUnitInfo> winningUnits = (result.winner == UnitOwner.Player1)
            ? PlayerDeployData.Instance.player1Units
            : PlayerDeployData.Instance.player2Units;

        Debug.Log($"[EndScene] 勝利者: {result.winner}, ユニット数: {winningUnits.Count}");

        for (int i = 0; i < winningUnits.Count && i < charaSlots.Length; i++)
        {
            var unit = winningUnits[i];
            string id = unit.charaID;

            GameObject prefab = GetPreviewByID(id);
            if (prefab == null)
            {
                Debug.LogWarning($"[EndScene] prefab not found for id: {id}");
                continue;
            }

            // 清空旧内容（保险）
            foreach (Transform child in charaSlots[i])
            {
                Destroy(child.gameObject);
            }

            // 放到 slot[i] 下
            GameObject go = Instantiate(prefab, charaSlots[i]);
            go.transform.localPosition = Vector3.zero;
            go.SetActive(true);

            // 不显示编号（可选）
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = "";
        }
    }

    void SetupButtons()
    {
        // 注册按钮供 UISelectionManager 使用（非必须，但安全）
        UISelectionManager.Instance.RegisterButton(mainmenuButton);
        UISelectionManager.Instance.RegisterButton(quitButton); 

        // 清空当前选中，确保不会继承旧按钮
        EventSystem.current.SetSelectedGameObject(null);

        // 设置初始选中按钮
        UISelectionManager.Instance.SetInitialSelectedDelayed(mainFirstSelectedButton.gameObject);
    }

    GameObject GetPreviewByID(string id)
    {
        foreach (var entry in charaIDPrefabs)
        {
            if (entry != null && entry.charaID == id)
                return entry.prefab;
        }
        return null;
    }

    public void ReturnToMainMenu()
    {
        GameResetUtility.ResetGame();
        SceneManager.LoadScene("TitleScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
