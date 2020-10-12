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
        [Serializable]
        public class PromptDesc
        {
            public PromptDesc(string prompt, Sprite image, (string, Button.ButtonClickedEvent)[] options, bool includeBackButton)
            {
                this.prompt = prompt;
                this.image = image;
                this.options = options;
                this.includeBackButton = includeBackButton;
            }
            public string prompt;
            public Sprite image;
            public (string, Button.ButtonClickedEvent)[] options;
            public bool includeBackButton;
        }

        class DialogueBufferEntry
        {
            public Constant id;
            public string[] dialogues;
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
        DialogueBufferEntry _curDialogue = null;
        Queue<DialogueBufferEntry> _bufferedDialogues = new Queue<DialogueBufferEntry>();

        public void DisplayDialogue(string[] dialogueSequence, Constant id)
        {
            if(dialogueSequence != null)
            {
                //if the dialogue box is inactive, there shouldn't be any dialogues left in the buffer
                Debug.Assert(!(!_dialoguePanel.activeSelf && _bufferedDialogues.Count > 0));


                DialogueBufferEntry entry = new DialogueBufferEntry { dialogues = dialogueSequence, id = id };
                //activate the dialogue box and advance the dialogue if there is no dialogue yet
                if (!_dialoguePanel.activeSelf)
                {
                    Messenger.Broadcast(M_EventType.ON_DIALOGUE_START, new DialogueEventData(id));

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
            if (_curDialogue.cur == _curDialogue.dialogues.Length)
            {
                Messenger.Broadcast(M_EventType.ON_DIALOGUE_END, new DialogueEventData(_curDialogue.id));

                if (_bufferedDialogues.Count > 0)
                    _curDialogue = _bufferedDialogues.Dequeue();
            }

            if (_curDialogue.cur < _curDialogue.dialogues.Length)
            {
                _dialogueText.text = _curDialogue.dialogues[_curDialogue.cur++];

                //nothing left
                if (_curDialogue.cur == _curDialogue.dialogues.Length && _bufferedDialogues.Count == 0)
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

        public void DisplayPrompt(string title, string prompt, Sprite image, (string, Button.ButtonClickedEvent)[] options, string back)
        {
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

            _backButton.gameObject.SetActive(true);
            _backButton.GetComponentInChildren<Text>().text = back;

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
        }

        public override void OnEnterMenu()
        {
        }
    }
}
