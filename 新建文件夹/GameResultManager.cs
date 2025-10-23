using System.Collections.Generic;
using UnityEngine;

public class GameResultManager : MonoBehaviour
{
    public static GameResultManager Instance;

    public UnitOwner winner;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetResult(UnitOwner win, List<DeployUnitInfo> units)
    {
        winner = win;
    }
}
