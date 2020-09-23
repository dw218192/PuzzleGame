using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.EventSystem;

namespace PuzzleGame
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        enum EControlState
        {
            NORMAL,
            JUMP
        }

        Rigidbody2D _rgbody;
        Collider2D _collider;
        float _jumpStartY;

        [Serializable]
        public class MovementConfig
        {
            public float speed = 1f;
        }

        [SerializeField] MovementConfig _moveConfig = new MovementConfig();
        EControlState _state = EControlState.NORMAL;
        Vector2 _curMoveVector = Vector2.zero;
        
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
            _collider = GetComponent<Collider2D>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!controlEnabled)
                return;

            //TODO: demo forbids 2.5D movements
            NormalUpdate();

            /*
            switch (_state)
            {
                case EControlState.NORMAL:
                    NormalUpdate();
                    break;
                case EControlState.JUMP:
                    JumpUpdate();
                    break;
            }
            */
        }

        void GoToState(EControlState state)
        {
            _state = state;

            switch(state)
            {
                case EControlState.NORMAL:
                    OnNormal();
                    break;
                case EControlState.JUMP:
                    OnEnterJump();
                    break;
            }
        }


        public void StopFalling()
        {
            if (_rgbody.velocity.y < 0)
            {
                Vector2 vel = _rgbody.velocity;
                vel.y = 0;
                _rgbody.velocity = vel;
            }
        }

        void OnEnterJump()
        {
            gameObject.layer = GameConst.k_playerJumpingLayer;
            _jumpStartY = transform.position.y;

            Vector2 right = Vector3.Cross(Vector3.forward, Physics2D.gravity).normalized;
            //kill vertical velocity
            _rgbody.velocity = right * _rgbody.velocity.x;
            _rgbody.AddForce(-Physics2D.gravity, ForceMode2D.Impulse);
        }

        void OnNormal()
        {
            gameObject.layer = GameConst.k_playerLayer;
        }

        void JumpUpdate()
        {
            if(transform.position.y < _jumpStartY && _rgbody.velocity.y < 0)
            {
                Vector2 pos = transform.position;
                pos.y = _jumpStartY;
                transform.position = pos;

                Vector2 vel = _rgbody.velocity;
                vel.y = 0;
                _rgbody.velocity = vel;

                GoToState(EControlState.NORMAL);
                return;
            }

            //we must have landed on something
            if(Mathf.Approximately(_rgbody.velocity.y, 0.0f))
            {
                GoToState(EControlState.NORMAL);
                return;
            }

            //apply gravity
            _rgbody.AddForce(Physics2D.gravity, ForceMode2D.Force);
        }

        bool CheckGrounded()
        {
            return Physics2D.Raycast(transform.position, -GameContext.s_up, _collider.bounds.extents.y + 1e-2f);
        }

        Vector2 GetInput()
        {
            Vector2 moveVector = Vector2.zero;
            if (Input.GetKey(KeyCode.A))
            {
                moveVector += Vector2.left;
            }
            if (Input.GetKey(KeyCode.D))
            {
                moveVector += Vector2.right;
            }

            //TODO: demo forbids 2.5D movements
            /*
            if (Input.GetKey(KeyCode.W))
            {
                moveVector += Vector2.up;
            }
            if (Input.GetKey(KeyCode.S))
            {
                moveVector += Vector2.down;
            }
            */

            return moveVector;
        }

        void DampenExtraSpeed()
        {
            //TODO: 2.5d movement is not implemented for now
        }

        void NormalUpdate()
        {
            if (CheckGrounded() && Input.GetKeyDown(KeyCode.Space))
                _rgbody.AddForce(-Physics2D.gravity, ForceMode2D.Impulse);

            _curMoveVector = GetInput();
            //TODO: 2.5d movement is not implemented for now
            float horizontalMove = _curMoveVector.x;

            if(Vector2.Dot(GameContext.s_right, _rgbody.velocity) < _moveConfig.speed)
                _rgbody.AddForce(GameContext.s_right * horizontalMove, ForceMode2D.Force);

            DampenExtraSpeed();
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 100, 32), _state.ToString());
            GUI.Label(new Rect(10, 50, 100, 32), _rgbody.velocity.ToString());
        }
    }
}