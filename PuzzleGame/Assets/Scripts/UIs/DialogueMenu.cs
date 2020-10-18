using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using PuzzleGame.EventSystem;

using UltEvents;

namespace PuzzleGame.UI
{
    public class DialogueMenu : SingletonGameMenu<DialogueMenu>
    {
        class DialogueBufferEntry
        {
            public DialogueDef def;
            public int cur = 0;
        }

        [SerializeField] GameObject _dialoguePanel;
        [SerializeField] Text _dialogueText;
        [SerializeField] Button _dialogueButton;
        Text _dialogueButtonText;

        [SerializeField] GameObject _promptPanel;
        [SerializeField] GameObject _picturePromptContentRoot, _textPromptContentRoot;
        [SerializeField] Text _promptTitleText;
        [SerializeField] Text _promptText;
        [SerializeField] Text _promptImageText;
        [SerializeField] Image _promptImage;
        [SerializeField] GridLayoutGroup _promptLayoutGroup;
        Button _backButton;
        Button[] _optionButtons;

        //buffered dialogues
        PromptDef _curPrompt = null;
        DialogueBufferEntry _curDialogue = null;
        Queue<DialogueBufferEntry> _bufferedDialogues = new Queue<DialogueBufferEntry>();

        public void DisplayDialogue(DialogueDef dialogueDef)
        {
            if(dialogueDef)
            {
                //if the dialogue box is inactive, there shouldn't be any dialogues left in the buffer
                Debug.Assert(!(!_dialoguePanel.activeSelf && _bufferedDialogues.Count > 0));


                DialogueBufferEntry entry = new DialogueBufferEntry { def = dialogueDef };
                //activate the dialogue box and advance the dialogue if there is no dialogue yet
                if (!_dialoguePanel.activeSelf)
                {
                    Messenger.Broadcast(M_EventType.ON_DIALOGUE_START, new DialogueEventData(dialogueDef));

                    _dialoguePanel.SetActive(true);
                    _curDialogue = entry;
                    OnPressDialogueButton();
                }
                else
                {
                    _bufferedDialogues.Enqueue(entry);
                }

                GameContext.s_UIMgr.OpenMenu(Instance);
            }
        }
        void OnPressDialogueButton()
        {
            if (_curDialogue.cur == _curDialogue.def.dialogues.Length)
            {
                Messenger.Broadcast(M_EventType.ON_DIALOGUE_END, new DialogueEventData(_curDialogue.def));

                if (_bufferedDialogues.Count > 0)
                    _curDialogue = _bufferedDialogues.Dequeue();
            }

            if (_curDialogue.cur < _curDialogue.def.dialogues.Length)
            {
                _dialogueText.text = _curDialogue.def.dialogues[_curDialogue.cur++];

                //nothing left
                if (_curDialogue.cur == _curDialogue.def.dialogues.Length && _bufferedDialogues.Count == 0)
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

        public void DisplayPrompt(PromptDef promptDef)
        {
            if(_curPrompt)
                ClosePrompt();

            _curPrompt = promptDef;
            _promptPanel.SetActive(true);
            _promptTitleText.text = promptDef.title;
            Sprite image = promptDef.promptImage;
            string prompt = promptDef.prompt;
            var options = promptDef.options;

            _textPromptContentRoot.SetActive(!image);
            _picturePromptContentRoot.SetActive(image);

            if (!image)
            {
                _promptText.text = prompt;
            }
            else
            {
                _promptImage.sprite = image;
                _promptImageText.text = prompt;
            }

            int i = 0;
            if (options != null)
            {
                Debug.Assert(options.Length + 1 <= _optionButtons.Length);

                for (; i < options.Length; i++)
                {
                    var button = _optionButtons[i];
                    var optionDesc = options[i];

                    button.gameObject.SetActive(true);
                    button.transform.parent.SetParent(_promptLayoutGroup.transform, false);
                    button.GetComponentInChildren<Text>().text = optionDesc.optionName;

                    button.onClick = new Button.ButtonClickedEvent();
                    button.onClick.AddListener(()=> 
                    {
                        optionDesc.optionEvents?.Invoke();
                    });
                }
            }

            while (i < _optionButtons.Length)
            {
                _optionButtons[i++].gameObject.SetActive(false);
            }

            if (promptDef.backButtonName != null)
            {
                _backButton.gameObject.SetActive(true);
                _backButton.GetComponentInChildren<Text>().text = promptDef.backButtonName;
            }
            else
            {
                _backButton.gameObject.SetActive(false);
            }

            GameContext.s_UIMgr.OpenMenu(Instance);
        }

        public void DisplayPromptOneShot(string title, string prompt, Sprite image, (string, Button.ButtonClickedEvent)[] options, string back)
        {
            if (_curPrompt)
                ClosePrompt();

            _curPrompt = null;
            _promptPanel.SetActive(true);
            _promptTitleText.text = title;
            _textPromptContentRoot.SetActive(!image);
            _picturePromptContentRoot.SetActive(image);

            if (!image)
            {
                _promptText.text = prompt;
            }
            else
            {
                _promptImage.sprite = image;
                _promptImageText.text = prompt;
            }

            int i = 0;
            if (options != null)
            {
                Debug.Assert(options.Length + 1 <= _optionButtons.Length);

                for (; i < options.Length; i++)
                {
                    var button = _optionButtons[i];
                    button.gameObject.SetActive(true);
                    button.transform.parent.SetParent(_promptLayoutGroup.transform, false);
                    button.GetComponentInChildren<Text>().text = options[i].Item1;
                    button.onClick = options[i].Item2;
                }
            }

            while (i < _optionButtons.Length)
            {
                _optionButtons[i++].gameObject.SetActive(false);
            }
             
            if(back != null)
            {
                _backButton.gameObject.SetActive(true);
                _backButton.GetComponentInChildren<Text>().text = back;
            }
            else
            {
                _backButton.gameObject.SetActive(false);
            }

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

            //dialogue button
            _dialogueButtonText = _dialogueButton.GetComponentInChildren<Text>();
            _dialogueButton.onClick.AddListener(OnPressDialogueButton);

            //start with nothing
            _dialoguePanel.SetActive(false);
            _promptPanel.SetActive(false);
        }

        public void ClosePrompt()
        {
            if(_curPrompt)
            {
                GameContext.s_gameMgr.OnEndPrompt(_curPrompt);
                _curPrompt.hasPlayed = true;
                _curPrompt = null;
            }

            _promptPanel.SetActive(false);

            if (!_promptPanel.activeSelf && !_dialoguePanel.activeSelf)
            {
                OnBackPressed();
            }
        }
        public void CloseDialogue()
        {
            GameContext.s_gameMgr.OnEndDialogue(_curDialogue.def);
            _curDialogue.def.hasPlayed = true;

            _dialoguePanel.SetActive(false);

            if (!_promptPanel.activeSelf && !_dialoguePanel.activeSelf)
            {
                OnBackPressed();
            }
        }

        public override void OnEnterMenu()
        {
            base.OnEnterMenu();
        }
    }
}
