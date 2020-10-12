using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

using PuzzleGame.EventSystem;
using PuzzleGame.UI;
using UnityEngine.Playables;

namespace PuzzleGame
{
    public class GameManager : MonoBehaviour
    {
        public Room roomPrefab = null;
        public Player playerPrefab = null;

        [Serializable]
        class GameResources
        {
            public Sprite paintingRotationTutorialImage;
        }

        [Serializable]
        class GameProgressStats
        {
            //can the player go out of the starting room?
            public BoolVariable canGoOutOfStartingRoom;
            //can the painting be rotated?
            public BoolVariable canRotatePainting;
        }

        /// <summary>
        /// NOTE: cutscenes are identified by PlayableDirector
        /// dialogues are identified by Constant SOs
        /// </summary>
        [Serializable]
        class CutSceneAndDialogues
        {
            public TimelineAsset puzzle2CutScene;
            public Constant rotationUnlockDialogueId;
        }

        [SerializeField]
        GameProgressStats _progessStats = new GameProgressStats();
        [SerializeField]
        CutSceneAndDialogues _cutSceneAndDialogues = new CutSceneAndDialogues();
        [SerializeField]
        GameResources _resources = new GameResources();

        public Room curRoom { get; set; } = null;

        private void Awake()
        {
            if (GameContext.s_gameMgr != null)
                Destroy(this);
            else
                GameContext.s_gameMgr = this;

            Messenger.AddListener<RoomEventData>(M_EventType.ON_ENTER_ROOM, OnEnterRoom);
            Messenger.AddListener<CutSceneEventData>(M_EventType.ON_CUTSCENE_END, OnEndCutScene);
            Messenger.AddListener<DialogueEventData>(M_EventType.ON_DIALOGUE_END, OnEndDialogue);
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

        //these game progression stuff are hardcoded for now, maybe forever
        private void OnEndCutScene(CutSceneEventData data)
        {
            if(ReferenceEquals(data.cutScene, _cutSceneAndDialogues.puzzle2CutScene))
            {
                //unlock painting rotation
                _progessStats.canRotatePainting.val = true;
            }
        }

        private void OnEndDialogue(DialogueEventData data)
        {
            if (ReferenceEquals(data.dialogueID, _cutSceneAndDialogues.rotationUnlockDialogueId))
            {
                DialogueMenu.Instance.DisplayPrompt("New Interaction", "You may now interact with the painting on its sides", _resources.paintingRotationTutorialImage, null, "OK");
            }
        }
        #endregion

        #region DEBUG
        void TestRoomCutscene()
        {
            IEnumerator _innerRoutine()
            {
                yield return new WaitForSecondsRealtime(5f);
                curRoom.PlayCutScene(_cutSceneAndDialogues.puzzle2CutScene);
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
