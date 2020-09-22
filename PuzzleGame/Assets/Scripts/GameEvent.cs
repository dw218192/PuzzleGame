using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.EventSystem
{
    [Serializable]
    public class RoomEventData : MessengerEventData
    {
        public Room room;
    }

    public enum M_EventType
    {
        ON_ENTER_ROOM,
        ON_EXIT_ROOM
    }
}
