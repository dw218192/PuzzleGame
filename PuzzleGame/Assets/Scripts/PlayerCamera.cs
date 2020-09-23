using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.EventSystem;

namespace PuzzleGame
{
    [RequireComponent(typeof(Camera))]
    public class PlayerCamera : MonoBehaviour
    {
        Camera _cam;
        [SerializeField] Vector2 _followOffset = Vector2.zero;
        bool _inTransition = false;
        Vector2 _camMin, _camMax;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            Vector3 pos = transform.position;
            pos.z = -10;
            transform.position = pos;

            Messenger.AddListener<RoomEventData>(M_EventType.ON_BEFORE_ENTER_ROOM, OnBeforeEnterRoom);
            Messenger.AddListener<RoomEventData>(M_EventType.ON_EXIT_ROOM, OnExitRoom);
        }

        private void Start()
        {

        }

        private void LateUpdate()
        {
            if (_inTransition || !GameContext.s_player)
                return;

            Vector2 moveOffset = GameContext.s_player.transform.position - transform.position;
            moveOffset += _followOffset;

            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x + moveOffset.x, _camMin.x, _camMax.x),
                Mathf.Clamp(transform.position.y + moveOffset.y, _camMin.y, _camMax.y),
                transform.position.z);
        }

        //event system handlers
        void OnBeforeEnterRoom(RoomEventData data)
        {
            IEnumerator _enterRoutine()
            {
                //TODO: check room size
                _inTransition = true;

                float initSize = _cam.orthographicSize;
                float targetSize = data.room.cameraViewDist / 2f;

                Vector3 initPos = transform.position;
                Vector3 targetPos = data.room.viewCenterPos;
                targetPos.z = initPos.z;

                float lerpT = 0.0f;

                while (lerpT <= 1f)
                {
                    lerpT += Time.deltaTime;
                    _cam.orthographicSize = Mathf.Lerp(initSize, targetSize, lerpT);
                    _cam.transform.position = Vector3.Lerp(initPos, targetPos, lerpT);

                    yield return new WaitForEndOfFrame();
                }

                //post transition
                _cam.orthographicSize = targetSize;
                Messenger.Broadcast(M_EventType.ON_ENTER_ROOM, data);

                //note: viewport space, top right is (1,1)
                Vector2 camExtent = _cam.ViewportToWorldPoint(Vector2.one) - transform.position;
                Rect roomArea = data.room.roomArea;
                Vector2 roomMax = roomArea.position + roomArea.width * Vector2.right;
                Vector2 roomMin = roomArea.position + roomArea.height * Vector2.up;

                _camMax = roomMax - camExtent;
                _camMin = roomMin + camExtent;

                _inTransition = false;
            }

            StartCoroutine(_enterRoutine());
        }

        void OnExitRoom(RoomEventData data)
        {

        }

        private void OnDrawGizmos()
        {
            if(GameContext.s_gameMgr)
            {
                Gizmos.DrawLine(_camMin, _camMax);
                Gizmos.color = Color.red;

                Rect roomArea = GameContext.s_gameMgr.curRoom.roomArea;
                Vector2 roomMax = roomArea.position + roomArea.width * Vector2.right;
                Vector2 roomMin = roomArea.position + roomArea.height * Vector2.up;
                Gizmos.DrawLine(roomMin, roomMax);
            }
        }
    }
}
