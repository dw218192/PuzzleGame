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
        enum EGameState
        {
            NONE,
            RUNNING,
        }

        public Room roomPrefab = null;
        public Player playerPrefab = null;

        [Serializable]
        class GameResources
        {
            public Sprite exitPaintingTutorialImage;
            public Sprite paintingRotationTutorialImage;
        }

        [Serializable]
        class DialogueUnlockPair
        {
            public DialogueDef dialogue;
            public BoolVariable unlockVariable;
        }

        /// <summary>
        /// NOTE: cutscenes are identified by PlayableDirector
        /// dialogues are identified by Constant SOs
        /// </summary>
        [Serializable]
        class CutSceneAndDialogues
        {
            public DialogueDef key1Dialogue;
            public DialogueDef key1Dialogue2;

            public TimelineAsset puzzle2CutScene;
            public DialogueDef rotationUnlockDialogue;

            public DialogueDef puzzle2StartDialogue;
            public DialogueDef puzzle2SuccessDialogue;
        }

        [SerializeField]
        DialogueUnlockPair[] _dialogueUnlocks;
        [SerializeField]
        CutSceneAndDialogues _cutSceneAndDialogues = new CutSceneAndDialogues();
        [SerializeField]
        GameResources _resources = new GameResources();
        EGameState _gameState = EGameState.NONE;
        HashSet<ScriptableObject> _hasPlayed = new HashSet<ScriptableObject>();

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
            Messenger.AddListener<PuzzleEventData>(M_EventType.ON_PUZZLE_START, OnStartPuzzle);
            Messenger.AddListener<PuzzleEventData>(M_EventType.ON_PUZZLE_END, OnEndPuzzle);
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

            DialogueMenu.Instance.DisplayPrompt("Congrats", "You have beat the game (for now)!!", null, null, null);
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

            //playtest build
            if (curRoom.roomIndex == GameConst.k_startingRoomIndex &&
                !_hasPlayed.Contains(_cutSceneAndDialogues.puzzle2CutScene) &&
                !_hasPlayed.Contains(_cutSceneAndDialogues.key1Dialogue2) &&
                _hasPlayed.Contains(_cutSceneAndDialogues.key1Dialogue))
            {
                DialogueMenu.Instance.DisplayDialogue(_cutSceneAndDialogues.key1Dialogue2);
            }
        }

        
        //TODO: these game progression stuff are hardcoded for now, maybe forever
        private void OnEndCutScene(CutSceneEventData data)
        {
            _hasPlayed.Add(data.cutScene);
        }

        private void OnEndDialogue(DialogueEventData data)
        {
            _hasPlayed.Add(data.dialogue);

            if (ReferenceEquals(data.dialogue, _cutSceneAndDialogues.key1Dialogue))
            {
                DialogueMenu.Instance.DisplayPrompt("New Interaction", "You may now exit a painting by holding <color=#ff0000ff><size=27>Q</size></color>", _resources.exitPaintingTutorialImage, null, "OK");
            }
            else if (ReferenceEquals(data.dialogue, _cutSceneAndDialogues.key1Dialogue2))
            {
                TestRoomCutscene();
            }
            else if (ReferenceEquals(data.dialogue, _cutSceneAndDialogues.rotationUnlockDialogue))
            {
                DialogueMenu.Instance.DisplayPrompt("New Interaction", "You may now interact with the painting near its corners", _resources.paintingRotationTutorialImage, null, "OK");
            }

            //unlocks
            foreach(var unlockPair in _dialogueUnlocks)
            {
                if(ReferenceEquals(unlockPair.dialogue, data.dialogue))
                {
                    unlockPair.unlockVariable.val = true;
                    break;
                }
            }
        }

        private void OnEndPuzzle(PuzzleEventData data)
        {
            if(data.finished)
                DialogueMenu.Instance.DisplayDialogue(_cutSceneAndDialogues.puzzle2SuccessDialogue);
        }

        private void OnStartPuzzle(PuzzleEventData data)
        {
            if(curRoom.roomIndex == GameConst.k_startingRoomIndex && !_hasPlayed.Contains(_cutSceneAndDialogues.puzzle2StartDialogue))
                DialogueMenu.Instance.DisplayDialogue(_cutSceneAndDialogues.puzzle2StartDialogue);
        }
        #endregion

        #region DEBUG
        void TestRoomCutscene()
        {
            IEnumerator _innerRoutine()
            {
                while (curRoom.roomIndex != GameConst.k_startingRoomIndex)
                    yield return new WaitForEndOfFrame();

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
