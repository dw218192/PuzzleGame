using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using PuzzleGame.EventSystem;

namespace PuzzleGame.UI
{
    public class DialogueMenu : GameMenu<DialogueMenu>
    {
        [SerializeField] GameObject _dialoguePanel;
        [SerializeField] Text _dialogueText;
        [SerializeField] Button _dialogueButton;
        Text _dialogueButtonText;

        [SerializeField] GameObject _promptPanel;
        [SerializeField] Text _promptText;
        [SerializeField] GridLayoutGroup _promptLayoutGroup;
        Button _backButton;
        Button[] _optionButtons;

        //buffered dialogues
        Queue<string> _curDialogues = new Queue<string>();

        public void DisplayDialogue(params string[] dialogueSequence)
        {
            if(dialogueSequence != null)
            {
                //if the dialogue box is inactive, there shouldn't be any dialogues left in the buffer
                Debug.Assert(!(!_dialoguePanel.activeSelf && _curDialogues.Count > 0));

                foreach(var dialogue in dialogueSequence)
                {
                    _curDialogues.Enqueue(dialogue);
                }

                //activate the dialogue box and advance the dialogue if there is no dialogue yet
                if(!_dialoguePanel.activeSelf)
                {
                    Messenger.Broadcast(M_EventType.ON_DIALOGUE_START);

                    _dialoguePanel.SetActive(true);
                    OnPressDialogueButton();
                }

                GameContext.s_UIMgr.OpenMenu(Instance);
            }
        }
        void OnPressDialogueButton()
        {
            if(_curDialogues.Count > 0)
            {
                _dialogueText.text = _curDialogues.Dequeue();

                if (_curDialogues.Count == 0)
                {
                    _dialogueButtonText.text = "Close";
                }
                else
                {
                    _dialogueButtonText.text = "Next";
                }
            }
            else
            {
                CloseDialogue();
            }
        }

        public void DisplayPrompt(string prompt, (string, Button.ButtonClickedEvent)[] options, bool includeBackButton)
        {
            if(options != null)
            {
                _promptPanel.SetActive(true);

                Debug.Assert(options.Length + (includeBackButton ? 1 : 0) <= _optionButtons.Length);

                int i;
                for (i = 0; i < options.Length; i++)
                {
                    var button = _optionButtons[i];
                    button.gameObject.SetActive(true);

                    button.transform.parent = _promptLayoutGroup.transform;
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

            //prompt button pool
            _optionButtons = _promptLayoutGroup.GetComponentsInChildren<Button>();
            foreach (var button in _optionButtons)
                button.gameObject.SetActive(false);

            //config back button
            _backButton = Instantiate(_optionButtons[0]);
            _backButton.transform.SetParent(_promptLayoutGroup.transform, false);
            _backButton.transform.localScale = Vector3.one;
            _backButton.GetComponentInChildren<Text>().text = "Back";
            _backButton.onClick.AddListener(ClosePrompt);
            _backButton.gameObject.SetActive(false);

            //dialogue button
            _dialogueButtonText = _dialogueButton.GetComponentInChildren<Text>();
            _dialogueButton.onClick.AddListener(OnPressDialogueButton);

            //start with nothing
            _dialoguePanel.SetActive(false);
            _promptPanel.SetActive(false);
        }

        void ClosePrompt()
        {
            _promptPanel.SetActive(false);

            if (!_promptPanel.activeSelf && !_dialoguePanel.activeSelf)
            {
                OnBackPressed();
            }
        }
        void CloseDialogue()
        {
            _dialoguePanel.SetActive(false);

            if (!_promptPanel.activeSelf && !_dialoguePanel.activeSelf)
            {
                OnBackPressed();
            }

            Messenger.Broadcast(M_EventType.ON_DIALOGUE_END);
        }
        public override void OnEnterMenu()
        {
        }
    }
}
