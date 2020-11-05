using System.Collections;
using System.Collections.Generic;
using UltEvents;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;

using PuzzleGame.UI;

namespace PuzzleGame
{
    public class Clock : Inspectable
    {
        [Header("Clock Puzzle")]
        [SerializeField] UltEvent _successEvent;

        // 8 triggers, trigger state is packed into a shared integer, indexed as follows
        // default state
        /*
         *           0
         *        1     7
         *    2             6
         *        3     5
         *           4
         */
        [SerializeField] AnimationClip _successClip;
        [SerializeField] AnimationClip _handsClip;
        [SerializeField] BoolVariable _isGameUnlocked;
        [SerializeField] IntVariable _switchState;
        [SerializeField] Transform _clockPlumb, _clockHands;
        [SerializeField] Sprite _lockedClock, _unlockedClock;
        [SerializeField] float _maxRotation;

        [Header("world space canvas setting")]
        [SerializeField] Button _handsTrigger;
        [SerializeField] Sprite _setSwitch, _unsetSwitch;
        [SerializeField] Transform _switchStartingAnchor;
        [SerializeField] GameObject _switchPrefab;
        [SerializeField] GameObject _unlockedGameRoot;
        [SerializeField] GameObject _lockedGameRoot;
        [SerializeField] Button _keyPickupButton;

        [Header("screen space canvas setting")]
        [SerializeField] Text _gravityButtonText;
        [SerializeField] Button _resetButton, _gravityButton;

        Image[] _switches;
        Dictionary<EdgeCollider2D, int> _switchCollider2SwitchIdx;
        float _switchRadius;
        float _handsRotation;
        bool _isPlayingClip;
        bool _isKeyPickedUp;

        const int k_numSwitches = 8;
        const int k_allsetState = 0b11111111;

        protected override void Start()
        {
            base.Start();

            _handsTrigger.onClick.AddListener(ExtendHands);
            _switchState.valueChanged += UpdateSwitches;
            _isGameUnlocked.valueChanged += UpdatePuzzleLockState;

            //generate switches
            _switches = new Image[k_numSwitches];
            int step = 360 / k_numSwitches;
            for (int i=0; i < k_numSwitches; i++)
            {
                _switches[i] = Instantiate(_switchPrefab, _switchStartingAnchor).GetComponent<Image>();
                _switches[i].transform.localPosition = _switchStartingAnchor.localPosition;
                _switches[i].transform.RotateAround(_switchStartingAnchor.parent.position, Vector3.forward, step * i);
            }

            _switchCollider2SwitchIdx = new Dictionary<EdgeCollider2D, int>();
            for (int i = 0; i < k_numSwitches; i++)
            {
                var collider = _switches[i].GetComponent<EdgeCollider2D>();
                collider.isTrigger = true;
                _switchCollider2SwitchIdx[collider] = i;
            }

            _switchRadius = Vector2.Distance(_switchStartingAnchor.position, _switchStartingAnchor.parent.position);
            _isPlayingClip = false;
            _handsRotation = 0;
            _isKeyPickedUp = false;

            //update sprite
            UpdateSwitches(_switchState.defaultValue);
            UpdatePuzzleLockState(_isGameUnlocked.defaultValue);

            //configure world UI
            _keyPickupButton.gameObject.SetActive(false);
            _keyPickupButton.onClick.AddListener(() =>
            {
                _isKeyPickedUp = true;
                _successEvent?.Invoke();
            });

            //configure screen UI
            _gravityButtonText.text = _inspectionCanvas.enableInspectCamRotation ? "Enable\nGravity View" : "Disable\nGravity View";
            _resetButton.onClick.AddListener(ResetPuzzle);
            _gravityButton.onClick.AddListener(ToggleCamRotation);
        }

        private void ExtendHands()
        {
            IEnumerator _successRoutine(float numSeconds)
            {
                AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), _successClip, out PlayableGraph graph);
                _isPlayingClip = true;

                yield return new WaitForSeconds(numSeconds);
                _isPlayingClip = false;

                graph.Destroy();
                _keyPickupButton.gameObject.SetActive(true);
            }

            IEnumerator _setSwitchRoutine(float numSeconds)
            {
                AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), _handsClip, out PlayableGraph graph);
                _isPlayingClip = true;

                yield return new WaitForSeconds(numSeconds);

                graph.Destroy();

                int flags = GetTargetSwitches();
                //flip the values
                _switchState.val = _switchState.val ^ flags;

                if (_switchState.val == k_allsetState)
                {
                    canInspect = false;
                    GameContext.s_gameMgr.StartCoroutine(_successRoutine(_successClip.length));
                }

                _isPlayingClip = false;
            }

            if(!canInspect || _isPlayingClip)
            {
                return;
            }

            GameContext.s_gameMgr.StartCoroutine(_setSwitchRoutine(_handsClip.length));
        }
        private void UpdatePuzzleLockState(bool isUnlocked)
        {
            _lockedGameRoot.SetActive(!isUnlocked);
            _unlockedGameRoot.SetActive(isUnlocked);

            if(isUnlocked)
            {
                _resetButton.gameObject.SetActive(true);
                _gravityButton.gameObject.SetActive(true);
                _screenBackButton.gameObject.SetActive(true);

                _clockHands.gameObject.SetActive(true);
                spriteRenderer.sprite = _unlockedClock;
            }
            else
            {
                _clockHands.gameObject.SetActive(false);
                _resetButton.gameObject.SetActive(false);
                _gravityButton.gameObject.SetActive(false);
                _screenBackButton.gameObject.SetActive(true);

                _clockHands.gameObject.SetActive(false);
                spriteRenderer.sprite = _lockedClock;
            }
        }
        public void ToggleCamRotation()
        {
            _inspectionCanvas.enableInspectCamRotation = !_inspectionCanvas.enableInspectCamRotation;
            _gravityButtonText.text = _inspectionCanvas.enableInspectCamRotation ? "Enable\nGravity View" : "Disable\nGravity View";
        }
        public void RotateHandsCW()
        {
            _handsRotation = Mathf.Max(_handsRotation - 45, -_maxRotation);
            _handsTrigger.transform.rotation = Quaternion.Euler(0, 0, _handsRotation);
        }
        public void RotateHandsCCW()
        {
            _handsRotation = Mathf.Min(_handsRotation + 45, _maxRotation);
            _handsTrigger.transform.rotation = Quaternion.Euler(0, 0, _handsRotation);
        }
        public void ResetPuzzle()
        {
            _switchState.val = 0;
        }

        private void UpdateSwitches(int newValue)
        {
            for(int i=0; i<k_numSwitches; i++)
            {
                if((newValue & (1 << i)) != 0)
                {
                    _switches[i].sprite = _setSwitch;
                }
                else
                {
                    _switches[i].sprite = _unsetSwitch;
                }
            }
        }

        /// <summary>
        /// which switches will be triggered given the room's global orientation?
        /// </summary>
        private int GetTargetSwitches()
        {
            /*
             * default hands state
             * 
             *   2 --- x --- 6
             *         |
             *         4
             */

            void DoHandRaycast(ref int returnValue, ref int totalOverlap, Vector2 dir)
            {
                var results = Physics2D.RaycastAll(_handsTrigger.transform.position, dir, _switchRadius, 1 << GameConst.k_UILayer);
                foreach (var hit in results)
                {
                    if (hit.collider is EdgeCollider2D edgeCollider)
                    {
                        if (_switchCollider2SwitchIdx.ContainsKey(edgeCollider))
                        {
                            returnValue |= 1 << _switchCollider2SwitchIdx[edgeCollider];
                            totalOverlap++;
                        }
                    }
                }
            }

            int ret = 0, total = 0;
            DoHandRaycast(ref ret, ref total, _handsTrigger.transform.right);
            DoHandRaycast(ref ret, ref total, -_handsTrigger.transform.right);
            DoHandRaycast(ref ret, ref total, -_handsTrigger.transform.up);

            //should have exactly 3 bits set
            Debug.Assert(total == 3);

            return ret;
        }

        protected override void Update()
        {
            base.Update();

            //make sure the hands are always pointing downwards
            if (_clockPlumb.rotation != Quaternion.identity)
            {
                _clockPlumb.rotation = Quaternion.identity;
            }

            _clockHands.rotation = Quaternion.Euler(0, 0, _handsRotation);
            _handsTrigger.transform.rotation = Quaternion.Euler(0, 0, _handsRotation);
        }

        public override void BeginInspect()
        {
            if (_isPlayingClip)
                return;

            base.BeginInspect();
        }

        public override void EndInspect()
        {
            base.EndInspect();

            //if the player succeeded in cracking the puzzle but forgot to pick up the key
            if(!canInspect && !_isKeyPickedUp)
            {
                _keyPickupButton.onClick.Invoke();
            }
        }
    }
}
