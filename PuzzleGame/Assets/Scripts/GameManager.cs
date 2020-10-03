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
            TestRoomSpawn();
            curRoom.SetCurrent();
            //TestRoomTransition();
            TestRoomRotation();
        }

        //messenger events
        private void OnEnterRoom(RoomEventData data)
        {
            if (!GameContext.s_player)
                GameContext.s_player = Instantiate(playerPrefab, curRoom.playerSpawnPos, Quaternion.identity);
            else
                GameContext.s_player.transform.position = curRoom.playerSpawnPos;
            
            //TODO: gravity changes, etc.

            curRoom = data.room;
        }

        void TestRoomSpawn()
        {
            curRoom = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity).GetComponent<Room>();
            curRoom.roomIndex = 0;
            curRoom.SpawnNext();
            curRoom.next.SpawnNext();
            curRoom.next.next.SpawnNext();
            curRoom.next.next.next.SpawnNext();
            curRoom.SpawnPrev();
            curRoom.prev.SpawnPrev();
            curRoom.prev.prev.SpawnPrev();
            curRoom.prev.prev.prev.SpawnPrev();
        }

        void TestRoomRotation()
        {
            curRoom.RotateNext(0);
            //curRoom.next.next.RotateNext(-180);
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
    }
}
