using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.EventSystem
{
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
        public InventoryChangeEventData(EItemID itemID, int slotIndex, int curItemQuantity)
        {
            this.itemID = itemID;
            this.slotIndex = slotIndex;
            this.curItemQuantity = curItemQuantity;
        }

        public EItemID itemID;
        public int slotIndex;
        public int curItemQuantity;
    }

    public enum M_EventType
    {
        ON_BEFORE_ENTER_ROOM, //ON_BEFORE_ENTER_ROOM will trigger ON_ENTER_ROOM
        ON_ENTER_ROOM,
        ON_EXIT_ROOM,
        ON_INVENTORY_CHANGE
    }
}
