using UnityEngine;
using UnityEngine.UI;

namespace AIScripts
{
    /// <summary>
    /// 胜负判定与重置（第 11 步）：一方死亡显示结果文本，按 R 重置对局。
    /// 挂在空物体 GameManager 上。
    /// 参考实现：阅读后在 MyScripts 中亲手敲一份自己的版本。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public Health player;
        public Health enemy;
        public Text resultText;

        Vector3 playerStart, enemyStart;

        void Awake()
        {
            Instance = this;
            playerStart = player.transform.position;
            enemyStart = enemy.transform.position;
            player.OnDeath += () => ShowResult("你输了");
            enemy.OnDeath += () => ShowResult("你赢了");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) ResetGame();
        }

        void ShowResult(string text)
        {
            if (resultText == null) return;
            resultText.text = text;
            resultText.gameObject.SetActive(true);
        }

        public void ResetGame()
        {
            // 双方回到出生点、满血、恢复可行动
            player.transform.position = playerStart;
            enemy.transform.position = enemyStart;
            player.Restore();
            enemy.Restore();
            if (resultText != null) resultText.gameObject.SetActive(false);
        }
    }
}
