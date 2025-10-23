using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GameSceneInitializer : MonoBehaviour
{
    public Tilemap tilemap; // 地図
    public GameObject[] characterPrefabs; // キャラクター prefab
    public CharaData charaDataSO; // データベース ScriptableObject
    public GameObject hpBarPrefab_Red;
    public GameObject hpBarPrefab_Yellow;

    void Start()
    {
        TurnManager.Instance.charaDataSO = charaDataSO;
        DeployAll(PlayerDeployData.Instance.player1Units);
        DeployAll(PlayerDeployData.Instance.player2Units);

        TurnManager.Instance.BeginGame();
    }

    void DeployAll(List<DeployUnitInfo> units)
    {
        foreach (var unit in units)
        {
            GameObject prefab = GetPrefabByID(unit.charaID);
            if (prefab == null)
            {
                Debug.LogError($"該当Prefabが見つかりません: {unit.charaID}");
                continue;
            }

            Vector3 worldPos = tilemap.GetCellCenterWorld(unit.tilePos);
            GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity);

            var baseComp = obj.GetComponent<CharaBase>();
            if (baseComp != null)
            {
                baseComp.Initialize(unit.charaID, unit.playerNum); // 数値初期化
            }

            var handler = obj.GetComponent<UnitActionHandler>();
            if (handler != null)
            {
                handler.owner = (unit.playerNum == 1) ? UnitOwner.Player1 : UnitOwner.Player2;
                TurnManager.Instance.RegisterUnit(handler);
            }
            CreateHPBar(obj, unit.playerNum, baseComp);
        }
    }

    GameObject GetPrefabByID(string id)
    {
        foreach (var prefab in characterPrefabs)
        {
            if (prefab.name == id)
                return prefab;
        }
        return null;
    }

    void CreateHPBar(GameObject character, int playerNum, CharaBase baseComp)
    {
        GameObject hpBarRoot = new GameObject("HPBarRoot");
        hpBarRoot.transform.SetParent(character.transform);
        hpBarRoot.transform.localPosition = new Vector3(0, 1.2f, 0); // 可调高度

        // 选择对应的血条 prefab
        GameObject prefab = (playerNum == 1) ? hpBarPrefab_Red : hpBarPrefab_Yellow;
        if (prefab == null)
        {
            Debug.LogWarning($"HPBar prefab not assigned for player {playerNum}");
            return;
        }

        GameObject bar = Instantiate(prefab, hpBarRoot.transform);
        bar.transform.localPosition = Vector3.zero;

        var hpBar = bar.GetComponent<WorldHPBar>();
        if (hpBar != null && baseComp != null)
        {
            hpBar.Bind(baseComp);
        }
    }
}
