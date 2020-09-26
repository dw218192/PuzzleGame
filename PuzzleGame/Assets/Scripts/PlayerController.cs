using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.EventSystem;

using Object = UnityEngine.Object;

namespace PuzzleGame
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] Transform _groundCheckAnchor;

        Rigidbody2D _rgbody;
        BoxCollider2D _collider;
        float _jumpStartY;
        float _groundCheckThreshold;

        bool _airBorne = false, _grounded = false, _lastGrounded;

        [Serializable]
        public class MovementConfig
        {
            public float speed = 1f;
            public float jumpThrust = 20f;
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
            _groundCheckThreshold = _groundCheckAnchor.localPosition.magnitude; //_collider.bounds.size.x;
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
            _lastGrounded = _grounded;
            _grounded = CheckGrounded();

            if (!_lastGrounded && _grounded)
            {
                _airBorne = false;
            }
            if (_grounded)
            {
                if(!_airBorne)
                {
                    float vertical = 0, horizontal = 0;

                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        vertical = _moveConfig.jumpThrust;
                        _airBorne = true;
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
            }
        }

        bool CheckGrounded()
        {
            RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, new Vector2(_collider.size.x, 0.2f), 0, -GameContext.s_up, _groundCheckThreshold, 
                1 << GameConst.k_boundaryLayer | 1 << GameConst.k_propLayer);

            if(hits.Length > 0)
            {
                foreach(var hit in hits)
                {
                    if (!Object.ReferenceEquals(hit.collider, _collider))
                        return true;
                }
            }

            return false;
        }
        

        //NOTE: TODO: the below code is only for demo
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

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 50, 200, 32), $"velocity: {_rgbody.velocity.ToString()}");
            GUI.Label(new Rect(10, 90, 200, 32), $"is grounded: {CheckGrounded().ToString()}");

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
        }
    }
}