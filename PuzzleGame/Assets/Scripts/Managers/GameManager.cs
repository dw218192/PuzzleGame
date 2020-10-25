using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

using PuzzleGame.EventSystem;
using PuzzleGame.UI;
using UnityEngine.Playables;
using UnityEngine.UI;

using UltEvents;

namespace PuzzleGame
{
    public class GameManager : MonoBehaviour
    {
        enum EGameState
        {
            NONE,
            RUNNING,
        }

        public Room roomPrefab = null;
        public Player playerPrefab = null;

        class ObjectEvents<T>
        {
            public T objectRef;
            public UltEvent events;
        }

        #region Game Audio
        [Header("Audio Configuration")]
        [SerializeField] AudioClip _mainMenuClip;
        [SerializeField] AudioClip _gameClip;
        [SerializeField] float _bgmVolume;
        #endregion

        #region Game Logic
        [Serializable]
        class DialogueEvents : ObjectEvents<DialogueDef> { }

        [Serializable]
        class CutSceneEvents : ObjectEvents<TimelineAsset> { }

        [Serializable]
        class PromptEvents : ObjectEvents<PromptDef> { }

        [Serializable]
        class RoomEvents
        {
            public bool isOneTime;
            public UltEvent events;
        }

        [Header("Game Logic Configuration")]
        [SerializeField]
        DialogueEvents[] _dialogueEvents;

        [SerializeField]
        CutSceneEvents[] _cutSceneEvents;

        [SerializeField]
        PromptEvents[] _promptEvents;

        [SerializeField]
        RoomEvents[] _enterRoomEvents;
        #endregion

        EGameState _gameState = EGameState.NONE;
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
            //play main menu theme
            GameContext.s_audioMgr.PlayConstantSound(_mainMenuClip, _bgmVolume);
        }

        public void StartGame()
        {
            curRoom = Room.SpawnChain(GameConst.k_totalNumRooms, GameConst.k_startingRoomIndex);
            _startRoom = curRoom;

            _gameState = EGameState.RUNNING;

            //play game theme
            GameContext.s_audioMgr.PlayConstantSound(_gameClip, _bgmVolume);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        #region Messenger Events
        private void OnEnterRoom(RoomEventData data)
        {
            curRoom = data.room;

            if (!GameContext.s_player)
                GameContext.s_player = Instantiate(playerPrefab, curRoom.playerSpawnPos, Quaternion.identity);
            else
                GameContext.s_player.transform.position = curRoom.playerSpawnPos;

            foreach(var evts in _enterRoomEvents)
            {
                evts.events?.Invoke();

                if(evts.isOneTime)
                {
                    evts.events.Clear();
                }
            }
        }
        public void OnEndCutScene(TimelineAsset cutScene)
        {
            foreach (var eventlist in _cutSceneEvents)
            {
                if (ReferenceEquals(cutScene, eventlist.objectRef))
                {
                    eventlist.events?.Invoke();
                }
            }
        }
        public void OnEndDialogue(DialogueDef dialogue)
        {
            foreach (var eventlist in _dialogueEvents)
            {
                if (ReferenceEquals(dialogue, eventlist.objectRef))
                {
                    eventlist.events?.Invoke();
                }
            }
        }
        public void OnEndPrompt(PromptDef prompt)
        {
            foreach (var eventlist in _promptEvents)
            {
                if (ReferenceEquals(prompt, eventlist.objectRef))
                {
                    eventlist.events?.Invoke();
                }
            }
        }

        public void DestroyActor(int actorId)
        {
            Room room = curRoom;

            while (room.roomIndex != 0)
            {
                room = room.prev;
            }

            while(room != null)
            {
                room.DestroyActor(actorId);
                room = room.next;
            }
        }

        /// <summary>
        /// return all instances of the same actor in each room
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        public Actor[] GetAllActorsById(int actorId)
        {
            Actor[] ret = new Actor[GameConst.k_totalNumRooms];
            Room room = curRoom;

            while (room.roomIndex != 0)
            {
                room = room.prev;
            }

            while (room != null)
            {
                ret[room.roomIndex] = room.GetActor(actorId);
                room = room.next;
            }

            return ret;
        }


        public void DestroyActorRange(int actorId, int startRoomIdx, int endRoomIdx)
        {
            Debug.Assert(startRoomIdx <= endRoomIdx && startRoomIdx >= 0 && startRoomIdx <= GameConst.k_totalNumRooms-1);

            Room startRoom = curRoom;
            if(curRoom.roomIndex > startRoomIdx)
            {
                do
                {
                    startRoom = startRoom.prev;
                }
                while (startRoom.roomIndex != startRoomIdx);
            }
            else if(curRoom.roomIndex < startRoomIdx)
            {
                do
                {
                    startRoom = startRoom.next;
                }
                while (startRoom.roomIndex != startRoomIdx);
            }

            int numRooms = endRoomIdx - startRoomIdx + 1;
            for(int i=0; i < numRooms; i++)
            {
                startRoom.DestroyActor(actorId);
                startRoom = startRoom.next;
            }
        }
        #endregion

        #region DEBUG
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
            if(_gameState == EGameState.RUNNING)
            {
                GUI.color = Color.red;
                GUI.contentColor = Color.red;
                GUI.Label(new Rect(Screen.width - 150f, 20f, 100f, 20f), "Debug Menu");

                if (GUI.Button(new Rect(Screen.width - 150f, 50f, 150f, 50f), "Quit Game"))
                {
                    Application.Quit();
                }
                if (GUI.Button(new Rect(Screen.width - 150f, 110f, 150f, 50f), "Teleport To\nStarting Room\nNOTE:may cause bugs"))
                {
                    Messenger.Broadcast(M_EventType.ON_BEFORE_ENTER_ROOM, new RoomEventData(_startRoom));
                }
                if (GUI.Button(new Rect(Screen.width - 150f, 170f, 150f, 50f), "Go to Next Room"))
                {
                    curRoom.GoToNext();
                }
            }
        }
        #endregion
    }
}
