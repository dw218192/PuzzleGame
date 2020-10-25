using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;

using UltEvents;

namespace PuzzleGame
{
    /*
        general structure of a puzzle:
        puzzle --> inspectable --> interactable --> actor
            world space canvas (the inspected object, any UI on the object itself)
            screen space canvas (any world-space independent UI)
    */
   
    public class Chandelier : Inspectable
    {
        [Header("Chandelier Puzzle")]
        [SerializeField] UltEvent _successEvent;
        [SerializeField] AnimationClip _codeClip;
        [SerializeField] string _correctSequence = "3142";

        [Header("world space canvas setting")]
        [SerializeField] Sprite _litSprite, _unlitSprite, _acceptedSprite;
        [SerializeField] Button _light1, _light2, _light3, _light4;

        [Header("screen space canvas setting")]
        [SerializeField] Text _prompt;
        StringBuilder _userSequence = new StringBuilder();

        AnimationClipPlayable _codeClipPlayable;
        bool _viewCodeMode = false;
        Button[] _lightButtons;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            _viewCodeMode = room.roomIndex == GameConst.k_startingRoomIndex;

            //puzzle mode
            if (!_viewCodeMode)
            {
                _prompt.text = "<color=red>Locked</color>";
                _lightButtons = new Button[] { _light1, _light2, _light3, _light4 };

                //configure static "void" calls here
                _light1.onClick.AddListener(() => { UserInput(0); });
                _light2.onClick.AddListener(() => { UserInput(1); });
                _light3.onClick.AddListener(() => { UserInput(2); });
                _light4.onClick.AddListener(() => { UserInput(3); });
            }
            //view code mode
            else
            {
                _codeClipPlayable = AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), _codeClip, out PlayableGraph graph);
                _prompt.gameObject.SetActive(false);
                _codeClipPlayable.SetSpeed(0.2);
            }
        }

        public override void BeginInspect()
        {
            base.BeginInspect();
            
            if (_viewCodeMode)
            {
                _codeClipPlayable.SetTime(0);
            }
        }

        public override void EndInspect()
        {
            base.EndInspect();

            if (!_viewCodeMode)
            {
                ResetAll();
            }
        }

        void UserInput(int lightId)
        {
            Debug.Assert(canInspect);

            _lightButtons[lightId].image.sprite = _litSprite;

            _userSequence.Append((lightId + 1).ToString());
            int curLen = _userSequence.Length;

            if (curLen < _correctSequence.Length)
            {
                if (_userSequence.ToString() != _correctSequence.Substring(0, curLen))
                {
                    ResetAll();
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
                }
                else
                {
                    SetSprites(_acceptedSprite);
                    _prompt.text = "<color=green>Unlocked</color>";
                    canInspect = false;
                    _successEvent?.Invoke();
                }
            }
        }

        private void ResetAll()
        {
            _userSequence.Clear();
            SetSprites(_unlitSprite);
        }

        private void SetSprites(Sprite sprite)
        {
            foreach (var button in _lightButtons)
            {
                button.image.sprite = sprite;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (_viewCodeMode)
            {
                //loop the clip
                if (Mathf.Abs((float)_codeClipPlayable.GetTime() - _codeClip.length) < 0.01f)
                {
                    _codeClipPlayable.SetTime(0);
                }
            }
        }

        private void OnDestroy()
        {
            if(_viewCodeMode)
                _codeClipPlayable.GetGraph().Destroy();
        }
    }
}