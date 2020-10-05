using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.EventSystem;
using PuzzleGame.UI;

namespace PuzzleGame
{
    /// <summary>
    /// wrappers for events
    /// </summary>
    [CreateAssetMenu(menuName="PuzzleGame/InteractableAction")]
    public class InteractableAction : ScriptableObject
    {
        public void ConsumableInteraction(Interactable interactable, Player player)
        {
            Room curRoom = GameContext.s_gameMgr.curRoom;
            if (curRoom)
            {
                curRoom.RemoveItemAll(interactable.itemID);
                player.AddToInventory(interactable.itemID, 1);
            }
            else
            {
                Debug.LogError("current room is null");
            }
        }

        public void EnterPainting(Interactable interactable, Player player)
        {
            Room curRoom = GameContext.s_gameMgr.curRoom;

            if(curRoom)
                curRoom.GoToNext();
            else
                Debug.LogError("current room is null");
        }

        public void RotatePaintingCW(Interactable interactable, Player player)
        {
            Room curRoom = GameContext.s_gameMgr.curRoom;

            if (curRoom)
                curRoom.RotateNext(true);
            else
                Debug.LogError("current room is null");
        }
        
        public void RotatePaintingCCW(Interactable interactable, Player player)
        {
            Room curRoom = GameContext.s_gameMgr.curRoom;

            if (curRoom)
                curRoom.RotateNext(false);
            else
                Debug.LogError("current room is null");
        }

        public void InvertPainting(Interactable interactable, Player player)
        {
            Room curRoom = GameContext.s_gameMgr.curRoom;

            if (curRoom)
                curRoom.InvertNext();
            else
                Debug.LogError("current room is null");
        }
    }
}
