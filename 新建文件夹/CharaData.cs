using UnityEngine;
using System.Collections.Generic;

public enum DamageType { Low, Normal, Crit }
public enum CharaType { Physical, Magical }
public enum QTEType { Archer, Mage, Assassin }

[System.Serializable]
public class CharaInfo
{
    public string id;
    public CharaType charaType;
    public QTEType qteType;
    public int maxHP;
    public int currentHP;
    public int maxMP;
    public int currentMP;
    public int maxSTA;
    public int currentSTA;
    public int atk; // 攻撃力
    public int atkRange; //攻撃範囲
    public AttackPattern attackPattern;
    public int def; // 防衛力
    public int ats; // 魔法攻撃力
    public int adf; // 魔法防衛力
    public int mov; // 移動範囲
}

[CreateAssetMenu(fileName = "CharaData", menuName = "Database/CharaData")]
public class CharaData : ScriptableObject
{
    public List<CharaInfo> charaList;

    private static Dictionary<string, CharaInfo> dataMap;

    public void Init()
    {
        dataMap = new Dictionary<string, CharaInfo>();
        foreach (var c in charaList)
        {
            dataMap[c.id] = c;
        }

        Debug.Log($"[CharaData] 初期化が完了しました！キャラーの数: {charaList.Count}");
    }

    public static CharaInfo Get(string id)
    {
        if (dataMap == null)
        {
            Debug.LogError("CharaData not initialized!");
            return null;
        }

        return dataMap.ContainsKey(id) ? dataMap[id] : null;
    }
}