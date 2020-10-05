using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class DialogueMenu : GameMenu<DialogueMenu>
    {
        [SerializeField] Text _promptText;
        [SerializeField] GridLayoutGroup _layoutGroup;
        Button _backButton;
        Button[] _optionButtons;

        public void Display(string prompt, (string, Button.ButtonClickedEvent)[] options, bool includeBackButton)
        {
            if(options != null)
            {
                Debug.Assert(options.Length + (includeBackButton ? 1 : 0) <= _optionButtons.Length);

                int i;
                for (i = 0; i < options.Length; i++)
                {
                    var button = _optionButtons[i];
                    button.gameObject.SetActive(true);

                    button.transform.parent = _layoutGroup.transform;
                    button.GetComponentInChildren<Text>().text = options[i].Item1;
                    button.onClick = options[i].Item2;
                }

                while (i < _optionButtons.Length)
                {
                    _optionButtons[i++].gameObject.SetActive(false);
                }
            }

            if(includeBackButton || options == null || options.Length == 0)
            {
                _backButton.gameObject.SetActive(true);
            }
            else
            {
                _backButton.gameObject.SetActive(false);
            }

            _promptText.text = prompt;

            GameContext.s_UIMgr.OpenMenu(Instance);
        }
        protected override void Awake()
        {
            base.Awake();
            _optionButtons = _layoutGroup.GetComponentsInChildren<Button>();
            foreach (var button in _optionButtons)
                button.gameObject.SetActive(false);

            _backButton = Instantiate(_optionButtons[0]);
            _backButton.transform.parent = _layoutGroup.transform;
            _backButton.transform.localScale = Vector3.one;
            _backButton.GetComponentInChildren<Text>().text = "Back";
            _backButton.onClick.AddListener(OnBackPressed);
            _backButton.gameObject.SetActive(false);
        }
        public override void OnEnterMenu()
        {
        }
    }
}
