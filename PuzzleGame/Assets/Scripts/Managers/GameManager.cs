using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.EventSystem;

namespace PuzzleGame
{
    public class GameManager : MonoBehaviour
    {
        public Room roomPrefab = null;
        public Player playerPrefab = null;

        public Room curRoom { get; set; } = null;

        private void Awake()
        {
            if (GameContext.s_gameMgr != null)
                Destroy(this);
            else
                GameContext.s_gameMgr = this;

            Messenger.AddListener<RoomEventData>(M_EventType.ON_ENTER_ROOM, OnEnterRoom);
        }

        private void Start()
        {
            curRoom = Room.SpawnChain(10, 4);
            _startRoom = curRoom;
            TestRoomCutscene();
        }

        //messenger events
        private void OnEnterRoom(RoomEventData data)
        {
            curRoom = data.room;

            if (!GameContext.s_player)
                GameContext.s_player = Instantiate(playerPrefab, curRoom.playerSpawnPos, Quaternion.identity);
            else
                GameContext.s_player.transform.position = curRoom.playerSpawnPos;
            
            //TODO: gravity changes, etc.
            /*
            Physics2D.gravity = - Physics2D.gravity.magnitude * data.room.contentRoot.up;
            GameContext.s_right = data.room.contentRoot.right;
            GameContext.s_up = data.room.contentRoot.up;
            */
        }

        void TestRoomCutscene()
        {
            IEnumerator _innerRoutine()
            {
                yield return new WaitForSecondsRealtime(5f);
                curRoom.PlayCutScene(0);
            }

            StartCoroutine(_innerRoutine());
        }

        void TestRoomTransition()
        {
            Room outermost = curRoom.prev.prev.prev.prev;

            IEnumerator _gotoRoomRoutine()
            {
                while(outermost)
                {
                    yield return new WaitForSecondsRealtime(2f);
                    outermost.GoToNext();
                    outermost = outermost.next;
                }
            }

            StartCoroutine(_gotoRoomRoutine());
        }

        Room _startRoom;
        private void OnGUI()
        {
            GUI.color = Color.red;
            GUI.contentColor = Color.red;
            GUI.Label(new Rect(Screen.width - 150f, 20f, 100f, 20f), "Prototype Build");
            GUI.Label(new Rect(Screen.width - 150f, 40f, 150f, 100f), "Hold Q -- out of painting\nE -- interact \nWASD -- Walk\nSpace -- Jump");

            if (GUI.Button(new Rect(Screen.width - 150f, 170f, 150f, 50f), "Quit Game"))
            {
                Application.Quit();
            }
            if (GUI.Button(new Rect(Screen.width - 150f, 230f, 150f, 50f), "Teleport To\nStarting Room\nNOTE:will cause bugs"))
            {
                Messenger.Broadcast(M_EventType.ON_BEFORE_ENTER_ROOM, new RoomEventData(_startRoom));
            }
        }
    }
}
