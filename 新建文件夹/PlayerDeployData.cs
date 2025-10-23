using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DeployUnitInfo
{
    public string charaID;
    public Vector3Int tilePos;
    public int playerNum;
}

public class PlayerDeployData : MonoBehaviour
{
    public static PlayerDeployData Instance;

    public List<DeployUnitInfo> player1Units = new();
    public List<DeployUnitInfo> player2Units = new();

    public List<int> player1Selections = new();
    public List<int> player2Selections = new();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Clear()
    {
        player1Units.Clear();
        player2Units.Clear();
        player1Selections.Clear();
        player2Selections.Clear();
    }
}
