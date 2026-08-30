using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyScripts
{

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public Health player;
        public Health enemy;
        public Text resultText;

        private Vector3 playerStart, enemyStart;

        private void Awake()
        {
            Instance = this;
            playerStart = player.transform.position;
            enemyStart = enemy.transform.position;
            player.OnDeath += () => ShowResult("you lose !");
            enemy.OnDeath += () => ShowResult("you win !");
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.R)) ResetGame();
        }

        private void ShowResult(string text)
        {
            if(resultText == null) return;
            resultText.text = text;
            resultText.gameObject.SetActive(true);
        }

        public void ResetGame()
        {
            player.transform.position = playerStart;
            enemy.transform.position = enemyStart;
            player.Restore();
            enemy.Restore();
            if(resultText != null)resultText.gameObject.SetActive(false);
        }
    }
}