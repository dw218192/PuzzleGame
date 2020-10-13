using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PuzzleGame.EventSystem
{
    #region Custom Unity Events
    [Serializable]
    //dialogue sequence + dialogue identifier
    public class DialogueEvent : UnityEvent<DialogueDef> { }
    #endregion

    #region Messenger Events
    [Serializable]
    public class RoomEventData : MessengerEventData
    {
        private RoomEventData() { }
        public RoomEventData(Room room)
        {
            this.room = room;
        }
        public Room room;
    }

    [Serializable]
    public class InventoryChangeEventData : MessengerEventData
    {
        private InventoryChangeEventData() { }
        public InventoryChangeEventData(InventoryItemDef def, int slotIndex, int curItemQuantity)
        {
            this.itemDef = def;
            this.slotIndex = slotIndex;
            this.curItemQuantity = curItemQuantity;
        }

        public InventoryItemDef itemDef;
        public int slotIndex;
        public int curItemQuantity;
    }

    [Serializable]
    public class CutSceneEventData : MessengerEventData
    {
        private CutSceneEventData() { }
        public CutSceneEventData(TimelineAsset cutScene)
        {
            this.cutScene = cutScene;
        }
        public TimelineAsset cutScene;
    }

    [Serializable]
    public class DialogueEventData : MessengerEventData
    {
        private DialogueEventData() { }
        public DialogueEventData(DialogueDef dialogue)
        {
            this.dialogue = dialogue;
        }
        public DialogueDef dialogue;
    }

    [Serializable]
    public class PuzzleEventData : MessengerEventData
    {
        private PuzzleEventData() { }
        public PuzzleEventData(bool finished)
        {
            this.finished = finished;
        }
        public bool finished;
    }

    public enum M_EventType
    {
        ON_BEFORE_ENTER_ROOM, //ON_BEFORE_ENTER_ROOM will trigger ON_ENTER_ROOM
        ON_ENTER_ROOM,
        ON_EXIT_ROOM,
        ON_INVENTORY_CHANGE,
        ON_CUTSCENE_START,
        ON_CUTSCENE_END,
        ON_DIALOGUE_START,
        ON_DIALOGUE_END,
        ON_PUZZLE_START,
        ON_PUZZLE_END
    }
    #endregion
}