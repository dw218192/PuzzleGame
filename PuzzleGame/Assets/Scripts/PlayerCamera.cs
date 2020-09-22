using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.EventSystem;

namespace PuzzleGame
{
    public class PlayerCamera : MonoBehaviour
    {
        private void Start()
        {
            Messenger.AddListener<RoomEventData>(M_EventType.ON_ENTER_ROOM, OnEnterRoom);
            Messenger.AddListener<RoomEventData>(M_EventType.ON_EXIT_ROOM, OnExitRoom);
        }

        public void GoToRoom(Room room)
        {

        }

        //event system handlers
        void OnEnterRoom(RoomEventData data)
        {

        }

        void OnExitRoom(RoomEventData data)
        {

        }
    }
}
