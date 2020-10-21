using PuzzleGame.EventSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class PauseMenu : SingletonGameMenu<PauseMenu>
    {
        [SerializeField] Button _resumeButton;
        [SerializeField] Button _quitGameButton;

        protected override void Start()
        {
            base.Start();

            _resumeButton.onClick.AddListener(ResumeGame);
            _quitGameButton.onClick.AddListener(QuitGame);
        }

        public override void OnEnterMenu()
        {
            base.OnEnterMenu();
            Messenger.Broadcast(M_EventType.ON_GAME_PAUSED);
        }

        void QuitGame()
        {
            GameContext.s_gameMgr.QuitGame();
        }

        void ResumeGame()
        {
            Messenger.Broadcast(M_EventType.ON_GAME_RESUMED);
            OnBackPressed();
        }
    }
}
