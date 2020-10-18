using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class MainMenu : SingletonGameMenu<MainMenu>
    {
        [SerializeField] Button _startButton;

        protected override void Start()
        {
            base.Start();

            _startButton.onClick.AddListener(StartGame);
            GameContext.s_UIMgr.OpenMenu(this);
        }

        public void StartGame()
        {
            GameContext.s_gameMgr.StartGame();
            OnBackPressed();
        }
    }
}
