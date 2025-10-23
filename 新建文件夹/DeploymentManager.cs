using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class DeploymentManager : MonoBehaviour
{
    public PlayerGridSelector player1GridSelector;
    public PlayerGridSelector player2GridSelector;

    public GameObject[] characterPrefabs;

    private int p1Index = 0, p2Index = 0;
    private bool p1Locked = false, p2Locked = false;

    private List<DeployUnitInfo> p1Data => PlayerDeployData.Instance.player1Units;
    private List<DeployUnitInfo> p2Data => PlayerDeployData.Instance.player2Units;

    private List<int> p1Selections => PlayerDeployData.Instance.player1Selections;
    private List<int> p2Selections => PlayerDeployData.Instance.player2Selections;

    public TextMeshProUGUI p1LockedText;
    public TextMeshProUGUI p2LockedText;

    void Start()
    {
        player1GridSelector.Initialize(TurnManager.Instance.player1Input, false);
        player2GridSelector.Initialize(TurnManager.Instance.player2Input, false);
    }

    void Update()
    {
        // Player1確認
        if (!p1Locked && TurnManager.Instance.player1Input.IsConfirmPressed())
        {
            if (p1Index < 3)
            {
                TryDeploy(1, player1GridSelector.GetCurrentCell(), ref p1Index);
            }
            else
            {
                p1Locked = true;
                UILockTextAnimator.AnimateLockIn(p1LockedText, 1);
                Debug.Log("Player1 ロック");
            }
        }

        // Player2確認
        if (!p2Locked && TurnManager.Instance.player2Input.IsConfirmPressed())
        {
            if (p2Index < 3)
            {
                TryDeploy(2, player2GridSelector.GetCurrentCell(), ref p2Index);
            }
            else
            {
                p2Locked = true;
                UILockTextAnimator.AnimateLockIn(p2LockedText, 2);
                Debug.Log("Player2 ロック");
            }
        }

        // プレイヤー1 最後の一人の派遣を撤回する
        if (!p1Locked && TurnManager.Instance.player1Input.IsCancelPressed())
        {
            if (p1Index > 0)
            {
                p1Index--;
                var last = p1Data[p1Data.Count - 1];
                p1Data.RemoveAt(p1Data.Count - 1);

                // マップ上のインスタンスを削除する
                var pos = player1GridSelector.tilemap.GetCellCenterWorld(last.tilePos);
                foreach (var unit in GameObject.FindGameObjectsWithTag("Unit"))
                {
                    if (Vector3.Distance(unit.transform.position, pos) < 0.1f)
                    {
                        Destroy(unit);
                        break;
                    }
                }

                Debug.Log("Player1 撤回する");
            }
        }

        // プレイヤー2 同様
        if (!p2Locked && TurnManager.Instance.player2Input.IsCancelPressed())
        {
            if (p2Index > 0)
            {
                p2Index--;
                var last = p2Data[p2Data.Count - 1];
                p2Data.RemoveAt(p2Data.Count - 1);

                var pos = player2GridSelector.tilemap.GetCellCenterWorld(last.tilePos);
                foreach (var unit in GameObject.FindGameObjectsWithTag("Unit"))
                {
                    if (Vector3.Distance(unit.transform.position, pos) < 0.1f)
                    {
                        Destroy(unit);
                        break;
                    }
                }

                Debug.Log("Player2 撤回する");
            }
        }

        // Player1 ロック解除
        if (p1Locked && TurnManager.Instance.player1Input.IsCancelPressed())
        {
            p1Locked = false;
            UILockTextAnimator.AnimateLockOut(p1LockedText, 1);
            Debug.Log("Player1 布陣ロック解除");
        }

        // Player2 ロック解除
        if (p2Locked && TurnManager.Instance.player2Input.IsCancelPressed())
        {
            p2Locked = false;
            UILockTextAnimator.AnimateLockOut(p2LockedText, 2);
            Debug.Log("Player2 布陣ロック解除");
        }

        if (p1Locked && p2Locked)
        {
            Debug.Log("両者がロックオン→戦闘開始");
            StartCoroutine(StartGameIfBothLocked());
        }
    }

    void TryDeploy(int playerNum, Vector3Int cell, ref int index)
    {
        Tilemap map = playerNum == 1 ? player1GridSelector.tilemap : player2GridSelector.tilemap;
        var selections = playerNum == 1 ? p1Selections : p2Selections;
        var deployed = playerNum == 1 ? p1Data : p2Data;

        if (!map.HasTile(cell)) return;
        if (index >= selections.Count) return;

        // 重複したデプロイメントの防止
        bool occupied = PlayerDeployData.Instance.player1Units.Exists(u => u.tilePos == cell)
                     || PlayerDeployData.Instance.player2Units.Exists(u => u.tilePos == cell);
        if (occupied)
        {
            Debug.LogWarning($"すでにこのマス {cell} にユニットがいます！");
            return;
        }

        int prefabIndex = selections[index];
        if (prefabIndex < 0 || prefabIndex >= characterPrefabs.Length)
        {
            Debug.LogError($"無効なキャラ index: {prefabIndex}");
            return;
        }

        string charaID = characterPrefabs[selections[index]].name;
        Vector3 worldPos = map.GetCellCenterWorld(cell);

        var go = Instantiate(characterPrefabs[selections[index]], worldPos, Quaternion.identity);
        go.GetComponent<Animator>()?.SetFloat("Direction",(playerNum == 1 ? 1 : 5));

        deployed.Add(new DeployUnitInfo
        {
            charaID = charaID,
            tilePos = cell,
            playerNum = playerNum
        });

        index++;
    }

    IEnumerator StartGameIfBothLocked()
    {
        yield return new WaitForSeconds(0.8f);
        SceneManager.LoadScene("GameScene");
    }
}
