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
    [RequireComponent(typeof(Player))]
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
        Player _player;
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
            _player = GetComponent<Player>();

            _groundCheckSize = new Vector2(_collider.size.x * 0.9f, 0.2f);

            _interactionTrigger.onTriggerEnter += (Collider2D collider) => 
            {
                Interactable interactable = collider.GetComponent<Interactable>();
                if (interactable && !_curInteractable)
                {
                    _curInteractable = interactable;
                    _curInteractable.OnEnterRange();
                }
            };
            _interactionTrigger.onTriggerExit += (Collider2D collider) => 
            {
                Interactable interactable = collider.GetComponent<Interactable>();
                if (interactable && Object.ReferenceEquals(interactable, _curInteractable))
                {
                    _curInteractable.OnExitRange();
                    _curInteractable = null;
                }
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
            Collider2D collider = Physics2D.OverlapBox(_groundCheckAnchor.position, _groundCheckSize, 
                Vector2.Angle(Vector2.up, GameContext.s_up),
                1 << GameConst.k_boundaryLayer | 1 << GameConst.k_propLayer);

            return collider && !collider.isTrigger && !Object.ReferenceEquals(collider, _collider);
        }

        void InteractionUpdate()
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                _curInteractable.OnInteract(_player);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawCube(_groundCheckAnchor.position, _groundCheckSize);
        }

        private void OnGUI()
        {
            GUI.color = Color.green;
            GUI.Label(new Rect(10, 50, 200, 32), $"velocity: {_rgbody.velocity.ToString()}");
            GUI.Label(new Rect(10, 80, 200, 32), $"is grounded: {CheckGrounded().ToString()}");

            if(_curInteractable)
                GUI.Label(new Rect(10, 110, 400, 32), $"cur interactable: {_curInteractable.gameObject.name}");
        }
    }
}