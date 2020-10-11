using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

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
            {
                if(Mathf.Min(curRoom.roomSize.x, curRoom.roomSize.y) <= 10f)
                {
                    DialogueMenu.Instance.DisplayPrompt("I am too large to fit into this room", null, true);
                }
                else
                {
                    curRoom.GoToNext();
                }
            }
            else
            {
                Debug.LogError(curRoom ? "player is null" : "current room is null");
            }
        }

        public void RotatePaintingCW(Interactable interactable, Player player)
        {
            Room curRoom = GameContext.s_gameMgr.curRoom;

            if (curRoom)
            {
                curRoom.RotateNext(true);
            }
            else
            {
                Debug.LogError("current room is null");
            }
        }
        
        public void RotatePaintingCCW(Interactable interactable, Player player)
        {
            Room curRoom = GameContext.s_gameMgr.curRoom;

            if (curRoom)
            {
                curRoom.RotateNext(false);
            }
            else
            {
                Debug.LogError("current room is null");
            }
        }

        public void InvertPainting(Interactable interactable, Player player)
        {
            Room curRoom = GameContext.s_gameMgr.curRoom;

            if (curRoom)
            {
                curRoom.InvertNext();
            }
            else
            {
                Debug.LogError("current room is null");
            }
        }
    }
}
