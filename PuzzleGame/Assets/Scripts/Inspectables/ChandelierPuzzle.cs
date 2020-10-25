using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using PuzzleGame.UI;
using UnityEngine.Playables;
using UnityEngine.Animations;
using PuzzleGame.EventSystem;

namespace PuzzleGame
{
    [Obsolete("use inspectable-based chandelier puzzle")]
    public class ChandelierPuzzle : MonoBehaviour
    {
        //4 lights, hardcoded
        [SerializeField] Interactable _chandelier;
        [SerializeField] AnimationClip _codeClip;
        AnimationClipPlayable _codeClipPlayable;

        [SerializeField] InventoryItemDef _reward;

        [SerializeField] Camera _puzzleCamera;
        [SerializeField] Canvas _canvas;
        [SerializeField] Sprite _litSprite, _unlitSprite, _acceptedSprite;
        [SerializeField] Button _light1, _light2, _light3, _light4;
        [SerializeField] Button _backButton;
        [SerializeField] Text _prompt;

        Button[] _buttons;

        //23 1 34 42
        [SerializeField] string _correctSequence = "3142";
        StringBuilder _userSequence = new StringBuilder();

        bool _viewCodeMode = false;
        bool _finished = false;

        private void Start()
        {
            _puzzleCamera.orthographicSize *= _chandelier.room.roomScale;
            _viewCodeMode = _chandelier.room.roomIndex == GameConst.k_startingRoomIndex;

            _puzzleCamera.gameObject.SetActive(false);
            _canvas.gameObject.SetActive(false);

            if(!_viewCodeMode)
            {
                _prompt.text = "<color=red>Locked</color>";
                _buttons = new Button[] { _light1, _light2, _light3, _light4 };

                _light1.onClick.AddListener(() => { UserInput(0); });
                _light2.onClick.AddListener(() => { UserInput(1); });
                _light3.onClick.AddListener(() => { UserInput(2); });
                _light4.onClick.AddListener(() => { UserInput(3); });
            }
            else
            {
                _codeClipPlayable = AnimationPlayableUtilities.PlayClip(_chandelier.GetComponent<Animator>(), _codeClip, out PlayableGraph graph);
                _prompt.gameObject.SetActive(false);
                _codeClipPlayable.SetSpeed(0.2);
            }

            _backButton.onClick.AddListener(ExitPuzzle);
        }

        void UserInput(int lightId)
        {
            _buttons[lightId].image.sprite = _litSprite;

            _userSequence.Append((lightId+1).ToString());
            int curLen = _userSequence.Length;

            if (curLen < _correctSequence.Length)
            {
                if (_userSequence.ToString() != _correctSequence.Substring(0, curLen))
                {
                    ResetAll();
                    OnFail();
                }
                else
                {

                }
            }
            else
            {
                if (_userSequence.ToString() != _correctSequence)
                {
                    ResetAll();
                    OnFail();
                }
                else
                {
                    OnSuccess();
                }
            }
        }

        public void ExitPuzzle()
        {
            IEnumerator _sucessRoutine()
            {
                yield return new WaitForSecondsRealtime(1f);
                GameContext.s_player.AddToInventory(_reward, 1, 1, GameContext.s_gameMgr.curRoom);
            }

            _puzzleCamera.gameObject.SetActive(false);
            _canvas.gameObject.SetActive(false);

            if (!_viewCodeMode)
            {
                ResetAll();

                if (_finished)
                {
                    GameContext.s_gameMgr.StartCoroutine(_sucessRoutine());
                }
            }
        }

        public void EnterPuzzle()
        {
            _chandelier.OnExitRange();

            _puzzleCamera.gameObject.SetActive(true);
            _canvas.gameObject.SetActive(true);

            if (!_viewCodeMode)
            {
                if (_finished)
                {
                    return;
                }
            }
            else
            {
                _codeClipPlayable.SetTime(0);
            }
        }

        void OnFail()
        {

        }

        void OnSuccess()
        {
            SetSprites(_acceptedSprite);
            _prompt.text = "<color=green>Unlocked</color>";
            _finished = true;
        }

        private void ResetAll()
        {
            _userSequence.Clear();
            SetSprites(_unlitSprite);
        }

        private void SetSprites(Sprite sprite)
        {
            foreach (var button in _buttons)
            {
                button.image.sprite = sprite;
            }
        }

        private void Update()
        {
            if(_viewCodeMode)
            {
                //loop the clip
                if(Mathf.Abs((float)_codeClipPlayable.GetTime() - _codeClip.length) < 0.01f)
                {
                    _codeClipPlayable.SetTime(0);
                }
            }
        }
    }
}

