using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class DialogueMenu : GameMenu<DialogueMenu>
    {
        [SerializeField] Button _option1Button;
        Text _option1ButtonText;
        [SerializeField] Button _option2Button;
        Text _option2ButtonText;
        [SerializeField] Text _promptText;

        public static void Display(string prompt, string option1, string option2, Button.ButtonClickedEvent opt1Evt, Button.ButtonClickedEvent opt2Evt)
        {
            Instance._option1ButtonText.text = option1;
            Instance._option2ButtonText.text = option2;
            Instance._option1Button.onClick = opt1Evt;
            Instance._option2Button.onClick = opt2Evt;
            Instance._promptText.text = prompt;

            GameContext.s_UIMgr.OpenMenu(Instance);
        }
        protected override void Awake()
        {
            base.Awake();
            _option1ButtonText = _option1Button.GetComponentInChildren<Text>();
            _option2ButtonText = _option2Button.GetComponentInChildren<Text>();
        }
        public override void OnEnterMenu()
        {
        }

        #region VERTICAL_SLICE DEMO
        void VerticalSliceQuitApp()
        {
            IEnumerator _quitRoutine()
            {
                yield return new WaitForSeconds(5f);
                Application.Quit();
            }

            _option1Button.gameObject.SetActive(false);
            _option2Button.gameObject.SetActive(false);
            _promptText.text = "Vertical Slice Finished!!!";

            GameContext.s_UIMgr.StartCoroutine(_quitRoutine());
        }
        public void VerticalSliceNoooKeyPrompt()
        {
            _option2Button.gameObject.SetActive(false);
            _option1ButtonText.text = "Yes";
            _option1Button.onClick.AddListener(OnBackPressed);
            _promptText.text = "I need a key";
        }
        public void VerticalSliceKeyPrompt()
        {
            _option2Button.gameObject.SetActive(true);
            _option1ButtonText.text = "Yes";
            _option2ButtonText.text = "No";
            _option1Button.onClick.AddListener(VerticalSliceQuitApp);
            _option2Button.onClick.AddListener(OnBackPressed);
            _promptText.text = "Insert Key?";
        }
        public void VerticalSliceWrongRoomPrompt()
        {
            _option2Button.gameObject.SetActive(false);
            _option1ButtonText.text = "Yes";
            _option1Button.onClick.AddListener(OnBackPressed);
            _promptText.text = "You must go to the original Room to unlock the door\nWhy? Because this is a vertical slice.";
        }
        #endregion
    }
}
