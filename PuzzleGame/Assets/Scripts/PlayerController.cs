using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.EventSystem;

namespace PuzzleGame
{
    public class PlayerController : MonoBehaviour
    {
        BoxCollider _collider;

        [Serializable]
        public class MovementConfig
        {
            public float speed = 1f;
        }

        [SerializeField] MovementConfig _moveConfig = new MovementConfig();
        public Vector2 curVelocity { get; private set; } = Vector2.zero;
        public bool controlEnabled { get; private set; } = true;

        Vector2 GetInput()
        {
            Vector2 moveVector = Vector2.zero;
            if(Input.GetKey(KeyCode.A))
            {
                moveVector += Vector2.left;
            }
            if (Input.GetKey(KeyCode.D))
            {
                moveVector += Vector2.right;
            }
            if (Input.GetKey(KeyCode.W))
            {
                moveVector += Vector2.up;
            }
            if (Input.GetKey(KeyCode.S))
            {
                moveVector += Vector2.down;
            }

            return moveVector;
        }

        private void Awake()
        {
            Messenger.AddListener(M_EventType.ON_BEFORE_ENTER_ROOM, (RoomEventData data) => { controlEnabled = false; });
            Messenger.AddListener(M_EventType.ON_ENTER_ROOM, (RoomEventData data) => { controlEnabled = true; });
        }

        // Start is called before the first frame update
        void Start()
        {
            _collider = GetComponent<BoxCollider>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!controlEnabled)
                return;

            Vector2 move = GetInput();
            if (move == Vector2.zero)
                return;

            curVelocity = move * _moveConfig.speed;
            transform.Translate(curVelocity * Time.deltaTime, Space.Self);
        }
    }
}