using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.EventSystem;
using PuzzleGame.UI;

namespace PuzzleGame
{
    public class GameManager : MonoBehaviour
    {
        public Room roomPrefab = null;
        public Player playerPrefab = null;

        [Serializable]
        class GameProgressStats
        {
            //can the player go out of the starting room?
            public BoolVariable canGoOutOfStartingRoom;
            //can the painting be rotated?
            public BoolVariable canRotatePainting;
        }

        [SerializeField]
        GameProgressStats _progessStats = new GameProgressStats();

        public Room curRoom { get; set; } = null;

        private void Awake()
        {
            if (GameContext.s_gameMgr != null)
                Destroy(this);
            else
                GameContext.s_gameMgr = this;

            Messenger.AddListener<RoomEventData>(M_EventType.ON_ENTER_ROOM, OnEnterRoom);
            Messenger.AddListener<CutSceneEventData>(M_EventType.ON_CUTSCENE_END, OnEndCutScene);
        }

        private void Start()
        {
            curRoom = Room.SpawnChain(GameConst.k_totalNumRooms, GameConst.k_startingRoomIndex);
            _startRoom = curRoom;
            TestRoomCutscene();
        }

        #region Messenger Events
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

        private void OnEndCutScene(CutSceneEventData data)
        {
            //narrative
            switch(data.cutSceneId)
            {
                case 0:
                    _progessStats.canRotatePainting.val = true;
                    break;
            }
        }
        #endregion

        #region DEBUG
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
        #endregion
    }
}
