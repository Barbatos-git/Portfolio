using System.Collections;
using UnityEngine;

public static class GameResetUtility
{
    public static void ResetGame()
    {
        Time.timeScale = 1f;

        var cleaner = new GameObject("SceneCleanupRunner").AddComponent<ResetHelper>();
        Object.DontDestroyOnLoad(cleaner.gameObject);
        cleaner.StartCoroutine(cleaner.CleanupAfterSceneLoad());
    }

    // 内部補助 MonoBehaviour
    private class ResetHelper : MonoBehaviour
    {
        public IEnumerator CleanupAfterSceneLoad()
        {
            yield return null; // 1フレーム待ってから破棄（シーンロードの競合を回避）

            CleanupSceneSpecificObjects();
            ResetStaticInstances();

            Debug.Log("[GameResetUtility] リセット完了");

            Destroy(gameObject); // 自分も削除
        }

        private void ResetStaticInstances()
        {
            Debug.Log("[GameResetUtility] 静的なインスタンス参照をクリア中...");

            GameResultManager.Instance = null;
            TurnManager.Instance = null;
            UISelectionManager.Instance = null;
            PlayerDeployData.Instance = null;
            UnitSelectorManager.Instance = null;
            AttackTargetSelector.Instance = null;
            MoveTargetSelector.Instance = null;
            DamageTextManager.Instance = null;
            GridSelector.Instance = null;
            RangeVisualizer.Instance = null;
            TurnBannerUI.Instance = null;
            PauseUIManager.Instance = null;
            ActionMenuUI.Instance = null;

            UnitSelectorManager.disableInput = false;
        }

        private void CleanupSceneSpecificObjects()
        {
            string[] cleanupTags = new[] {
                     "CleanupOnReset",
            };

            foreach (string tag in cleanupTags)
            {
                GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
                foreach (var go in targets)
                {
                    Destroy(go);
                    Debug.Log($"[GameResetUtility] タグ {tag} のオブジェクト {go.name} を削除しました");
                }
            }
        }
    }
}
