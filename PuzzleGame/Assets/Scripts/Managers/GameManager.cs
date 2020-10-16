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

        public enum EventType
        {
            ON_START,
            ON_END,
        }


        class ObjectEvents<T>
        {
            public T objectRef;
            public UltEvent events;
        }

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

        [SerializeField]
        DialogueEvents[] _dialogueEvents;

        [SerializeField]
        CutSceneEvents[] _cutSceneEvents;

        [SerializeField]
        PromptEvents[] _promptEvents;

        [SerializeField]
        RoomEvents[] _enterRoomEvents;

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

        public void StartGame()
        {
            curRoom = Room.SpawnChain(GameConst.k_totalNumRooms, GameConst.k_startingRoomIndex);
            _startRoom = curRoom;

            _gameState = EGameState.RUNNING;
        }

        public void QuitGame()
        {
            IEnumerator _routine()
            {
                yield return new WaitForSeconds(2);
                Application.Quit();
            }

            DialogueMenu.Instance.DisplayPromptOneShot("Congrats", "You have beat the game (for now)!!", null, null, null);
            StartCoroutine(_routine());
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
            }
        }
        #endregion
    }
}
