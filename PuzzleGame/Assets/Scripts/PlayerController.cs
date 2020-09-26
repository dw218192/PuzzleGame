using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.EventSystem;

using Object = UnityEngine.Object;

namespace PuzzleGame
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        #region Ground Check
        [SerializeField] Transform _groundCheckAnchor;
        Vector2 _groundCheckSize;
        bool _grounded = false;
        float _groundCheckTimer;
        #endregion

        #region Interaction
        [SerializeField] GenericTrigger _interactionTrigger;
        Interactable _curInteractable;
        float _interactionTriggerX;
        #endregion

        #region Components
        Rigidbody2D _rgbody;
        BoxCollider2D _collider;
        Animator _animator;
        SpriteRenderer _spriteRenderer;
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
        public bool controlEnabled { get; private set; } = true;

        private void Awake()
        {
            Messenger.AddListener(M_EventType.ON_BEFORE_ENTER_ROOM, (RoomEventData data) => { controlEnabled = false; });
            Messenger.AddListener(M_EventType.ON_ENTER_ROOM, (RoomEventData data) => { controlEnabled = true; });
        }

        // Start is called before the first frame update
        void Start()
        {
            _rgbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            _groundCheckSize = new Vector2(_collider.size.x * 0.9f, 0.2f);

            _interactionTrigger.onTriggerEnter += (Collider2D collider) => 
            {
                if(!_curInteractable)
                    _curInteractable = collider.GetComponent<Interactable>();
            };
            _interactionTrigger.onTriggerExit += (Collider2D collider) => 
            {
                if(Object.ReferenceEquals(collider.GetComponent<Interactable>(), _curInteractable))
                    _curInteractable = null;
            };

            _interactionTriggerX = _interactionTrigger.transform.localPosition.x;
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
            if (!controlEnabled)
                return;

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

            _animator.SetBool(GameConst.k_PlayerAirborne_AnimParam, !_grounded);
            _animator.SetBool(GameConst.k_PlayerWalking_AnimParam, !Mathf.Approximately(0, horizontal));

            _animator.SetFloat(GameConst.k_PlayerXSpeed_AnimParam, horizontal);
            _animator.SetFloat(GameConst.k_PlayerYSpeed_AnimParam, _rgbody.velocity.y);

            TurnAround(horizontal);
        }

        bool CheckGrounded()
        {
            return Physics2D.OverlapBox(_groundCheckAnchor.position, _groundCheckSize, 
                Vector2.Angle(Vector2.up, GameContext.s_up),
                1 << GameConst.k_boundaryLayer | 1 << GameConst.k_propLayer);
        }

        void InteractionUpdate()
        {

        }

        //NOTE: TODO: the below code is only for demo
#if false
        Interactable _paintingInteractable=null, _key=null;
        bool _showPrompt = false;
        int _demoPhase = 0;
        void InteractionUpdate()
        {
            if (_demoPhase == 0)
            {
                if (!_paintingInteractable || !_key)
                {
                    Transform child = GameContext.s_gameMgr.curRoom.contentRoot.Find("PaintingTrigger");
                    _paintingInteractable = child.GetComponent<Interactable>();

                    child = GameContext.s_gameMgr.curRoom.next.contentRoot.Find("Key");
                    _key = child.GetComponent<Interactable>();
                }

                if (_grounded)
                {
                    if (Vector2.Distance(transform.position, _paintingInteractable.transform.position) < 1f)
                    {
                        _showPrompt = true;

                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            GameContext.s_gameMgr.curRoom.GoToNext();
                            _showPrompt = false;
                            _demoPhase = 1;
                        }
                    }
                    else
                    {
                        _showPrompt = false;
                    }
                }
            }
            else if (_demoPhase == 1)
            {
                if (Vector2.Distance(transform.position, _key.transform.position) < 1f)
                {
                    _showPrompt = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        Destroy(_key.gameObject);
                        _showPrompt = false;
                        _demoPhase = 2;

                        StartCoroutine(_endGameRoutine());
                    }
                }
                else
                {
                    _showPrompt = false;
                }
            }

            IEnumerator _endGameRoutine()
            {
                yield return new WaitForSecondsRealtime(4f);
                Application.Quit();
            }
        }
#endif

        private void OnDrawGizmos()
        {
            Gizmos.DrawCube(_groundCheckAnchor.position, _groundCheckSize);
        }

        private void OnGUI()
        {
            GUI.color = Color.green;
            GUI.Label(new Rect(10, 50, 200, 32), $"velocity: {_rgbody.velocity.ToString()}");
            GUI.Label(new Rect(10, 90, 200, 32), $"is grounded: {CheckGrounded().ToString()}");

            if(_curInteractable)
                GUI.Label(new Rect(10, 130, 200, 32), $"cur interactable: {_curInteractable.gameObject.name}");

#if false
            if(_showPrompt)
            {
                GUIStyle style = new GUIStyle();
                style.richText = true;
                GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 400, 32), "<size=20><color=red>Press E to interact</color></size>");
            }

            if(_demoPhase == 2)
            {
                GUIStyle style = new GUIStyle();
                style.richText = true;
                GUI.Label(new Rect(Screen.width / 2 - 70, Screen.height / 2 - 16, 400, 32), "<size=20><color=red>You Win! End of Demo!</color></size>");
            }

#endif
        }
    }
}