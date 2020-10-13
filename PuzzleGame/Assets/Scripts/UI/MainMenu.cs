using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class MainMenu : GameMenu<MainMenu>
    {
        [SerializeField] Button _startButton;
        private void Start()
        {
            _startButton.onClick.AddListener(StartGame);
        }

        public void StartGame()
        {
            GameContext.s_gameMgr.StartGame();
            OnBackPressed();
        }
    }
}
