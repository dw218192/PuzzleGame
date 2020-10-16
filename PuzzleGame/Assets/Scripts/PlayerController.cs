using PuzzleGame.EventSystem;
using PuzzleGame.UI;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PuzzleGame
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Player))]
    public class PlayerController : MonoBehaviour
    {
        #region Movement
        [SerializeField] Transform _groundCheckAnchor;
        Vector2 _groundCheckSize;
        bool _grounded = false;
        float _groundCheckTimer;
        #endregion

        #region Interaction
        [SerializeField] GenericTrigger _interactionTrigger;
        [SerializeField] [Range(0.2f, 5f)] float _exitPaintInteractTime = 2f;
        float _exitPaintkeyHoldTimer = 0;
        bool _exitPaintkeyDownLastFrame;
        Interactable _curInteractable;
        float _interactionTriggerX;
        #endregion

        #region Components
        Rigidbody2D _rgbody;
        BoxCollider2D _collider;
        Animator _animator;
        SpriteRenderer _spriteRenderer;
        Player _player;
        #endregion

        #region Game Specific
        [SerializeField] BoolVariable _canExitStartingRoom;
        [SerializeField] BoolVariable _canExitSmallRoom;
        int _controlLockCnt = 0;
        #endregion
        
        [Serializable]
        public class MovementConfig
        {
            public float speed = 1f;
            public float jumpThrust = 20f;
            public float airBorneSpeed = 0.5f;
        }

        [SerializeField] MovementConfig _moveConfig = new MovementConfig();
        
        public Vector2 curVelocity { get { return _rgbody.velocity; } }

        bool _controlEnabled = true;
        public bool controlEnabled
        {
            get => _controlEnabled;
            private set
            {
                if(_controlEnabled != value)
                {
                    ClearState();
                }
                _controlEnabled = value;
            }
        }

        private void ClearState()
        {
            _exitPaintkeyHoldTimer = 0;
            _exitPaintkeyDownLastFrame = false;
            _curInteractable = null;

        }

        private void Awake()
        {
            Messenger.AddListener(M_EventType.ON_BEFORE_ENTER_ROOM, (RoomEventData data) => 
            {
                ++_controlLockCnt;
                controlEnabled = false; 
            });

            Messenger.AddListener(M_EventType.ON_ENTER_ROOM, (RoomEventData data) => 
            {
                if (--_controlLockCnt == 0)
                    controlEnabled = true;
            });

            Messenger.AddListener(M_EventType.ON_CUTSCENE_START, (CutSceneEventData data) => 
            {
                ++_controlLockCnt;
                controlEnabled = false; 
            });

            Messenger.AddListener(M_EventType.ON_CUTSCENE_END, (CutSceneEventData data) => 
            {
                if(--_controlLockCnt == 0)
                    controlEnabled = true; 
            });

            Messenger.AddListener(M_EventType.ON_DIALOGUE_START, (DialogueEventData data) => 
            {
                ++_controlLockCnt;
                controlEnabled = false; 
            });

            Messenger.AddListener(M_EventType.ON_DIALOGUE_END, (DialogueEventData data) => 
            {
                if (--_controlLockCnt == 0)
                    controlEnabled = true;
            });

            Messenger.AddListener(M_EventType.ON_PUZZLE_START, (PuzzleEventData data) =>
            {
                ++_controlLockCnt;
                controlEnabled = false;

                _interactionTrigger.gameObject.SetActive(false);
            });


            Messenger.AddListener(M_EventType.ON_PUZZLE_END, (PuzzleEventData data) =>
            {
                if (--_controlLockCnt == 0)
                    controlEnabled = true;

                _interactionTrigger.gameObject.SetActive(true);
            });
        }

        // Start is called before the first frame update
        void Start()
        {
            _rgbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _player = GetComponent<Player>();

            _groundCheckSize = new Vector2(_collider.size.x * 0.9f, 0.2f);

            _interactionTrigger.onTriggerEnter += OnTriggerEnterInteractable;
            _interactionTrigger.onTriggerStay += OnTriggerEnterInteractable;
            _interactionTrigger.onTriggerExit += OnTriggerExitInteractable;

            _interactionTriggerX = _interactionTrigger.transform.localPosition.x;
        }

        void OnTriggerEnterInteractable(Collider2D collider)
        {
            if (_curInteractable)
                return;

            Interactable interactable = collider.GetComponent<Interactable>();
            if (interactable && interactable.canInteract)
            {
                _curInteractable = interactable;
                _curInteractable.OnEnterRange();
            }
        }

        void OnTriggerExitInteractable(Collider2D collider)
        {
            Interactable interactable = collider.GetComponent<Interactable>();

            if (interactable && Object.ReferenceEquals(interactable, _curInteractable))
            {
                _curInteractable.OnExitRange();
                _curInteractable = null;
            }
        }

        void TurnAround(float horizontalVelocity)
        {
            if (!Mathf.Approximately(0, horizontalVelocity))
            {
                float sign = Mathf.Sign(horizontalVelocity);
                _spriteRenderer.flipX = sign < 0;

                Vector2 pos = _interactionTrigger.transform.localPosition;
                pos.x = sign * _interactionTriggerX;
                _interactionTrigger.transform.localPosition = pos;
            }
        }

        // Update is called once per frame
        void Update()
        {
            MovementUpdate();
            InteractionUpdate();
        }

        void MovementUpdate()
        {
            _groundCheckTimer = Mathf.Max(_groundCheckTimer - Time.deltaTime, 0);

            if(Mathf.Approximately(0, _groundCheckTimer))
            {
                _grounded = CheckGrounded();
            }

            float vertical = 0, horizontal = 0;

            if(controlEnabled)
            {
                if (_grounded)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        vertical = _moveConfig.jumpThrust;

                        _grounded = false;
                        _groundCheckTimer = Time.deltaTime * 10f;
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        horizontal = -_moveConfig.speed;
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        horizontal = _moveConfig.speed;
                    }

                    _rgbody.velocity = GameContext.s_right * horizontal + GameContext.s_up * vertical;
                }
                else
                {
                    if (Input.GetKey(KeyCode.A))
                    {
                        horizontal = -_moveConfig.airBorneSpeed;
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        horizontal = _moveConfig.airBorneSpeed;
                    }

                    _rgbody.velocity = GameContext.s_right * horizontal + GameContext.s_up * Vector2.Dot(GameContext.s_up, _rgbody.velocity);
                }
            }
            TurnAround(horizontal);

            _animator.SetBool(GameConst.k_PlayerAirborne_AnimParam, !_grounded);
            _animator.SetBool(GameConst.k_PlayerWalking_AnimParam, !Mathf.Approximately(0, horizontal));
            _animator.SetFloat(GameConst.k_PlayerXSpeed_AnimParam, horizontal);
            _animator.SetFloat(GameConst.k_PlayerYSpeed_AnimParam, _rgbody.velocity.y);
        }

        bool CheckGrounded()
        {
            Collider2D collider = Physics2D.OverlapBox(_groundCheckAnchor.position, _groundCheckSize, 
                Vector2.Angle(Vector2.up, GameContext.s_up),
                1 << GameConst.k_boundaryLayer | 1 << GameConst.k_propLayer);

            return collider && !collider.isTrigger && !Object.ReferenceEquals(collider, _collider);
        }

        void InteractionUpdate()
        {
            if (!controlEnabled)
                return;

            if(Input.GetKeyDown(KeyCode.E))
            {
                if(_curInteractable)
                    _curInteractable.OnInteract();
            }

            bool canGoOut = true;
            if (GameContext.s_gameMgr.curRoom.roomIndex == GameConst.k_startingRoomIndex)
            {
                canGoOut = _canExitStartingRoom.val;
            }
            else if (GameContext.s_gameMgr.curRoom.roomIndex > GameConst.k_startingRoomIndex)
            {
                canGoOut = _canExitSmallRoom.val;
            }

            if(canGoOut)
            {
                bool exitPaintingkeyDown = Input.GetKey(KeyCode.Q);
                //go out of the painting
                if (_exitPaintkeyDownLastFrame && exitPaintingkeyDown)
                {
                    _exitPaintkeyHoldTimer = Mathf.Min(_exitPaintInteractTime, _exitPaintkeyHoldTimer + Time.deltaTime);
                    GameContext.s_effectMgr.SetProgress(_exitPaintkeyHoldTimer / _exitPaintInteractTime);

                    if (Mathf.Approximately(_exitPaintkeyHoldTimer, _exitPaintInteractTime))
                    {
                        _exitPaintkeyHoldTimer = 0;
                        GameContext.s_effectMgr.HideProgressBar();

                        //TODO redo this check here
                        if (GameContext.s_gameMgr.curRoom.roomIndex == 2)
                        {
                            DialogueMenu.Instance.DisplayPromptOneShot("Message", "I will fall to death", null, null, "Ok then");
                        }
                        else
                        {
                            GameContext.s_gameMgr.curRoom.GoToPrev();
                        }
                    }
                }
                else if (!_exitPaintkeyDownLastFrame && exitPaintingkeyDown)
                {
                    _exitPaintkeyHoldTimer = 0;
                    GameContext.s_effectMgr.ShowProgressBar((Vector2)transform.position + 0.5f * Vector2.down,
                        Quaternion.Euler(0, 0, -90), transform);
                    GameContext.s_effectMgr.SetProgress(0);
                }
                else if (_exitPaintkeyDownLastFrame && !exitPaintingkeyDown)
                {
                    GameContext.s_effectMgr.HideProgressBar();
                }

                _exitPaintkeyDownLastFrame = exitPaintingkeyDown;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawCube(_groundCheckAnchor.position, _groundCheckSize);
        }

        private void OnGUI()
        {
#if DEVELOPMENT_BUILD
            GUI.color = Color.green;
            GUI.Label(new Rect(10, 50, 200, 32), $"velocity: {_rgbody.velocity.ToString()}");
            GUI.Label(new Rect(10, 80, 200, 32), $"is grounded: {CheckGrounded().ToString()}");

            if(_curInteractable)
                GUI.Label(new Rect(10, 110, 400, 32), $"cur interactable: {_curInteractable.gameObject.name}, can pickup = {((bool)_curInteractable.itemDef).ToString()}");
#endif
        }
    }
}