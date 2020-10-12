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
                curRoom.RemoveItemAll(interactable.itemDef);
                player.AddToInventory(interactable.itemDef, 1);
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
                if(curRoom.roomIndex == GameConst.k_maxRoomIndex - 1)
                {
                    DialogueMenu.Instance.DisplayPrompt("Message", "I am too large to fit into this room", null, null, "Ok then");
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

        #region PLAYTEST BUILD
        public void DoorInteraction(Interactable interactable, Player player)
        {
            Room curRoom = GameContext.s_gameMgr.curRoom;

            if (curRoom.roomIndex == GameConst.k_startingRoomIndex)
            {
                DialogueMenu.Instance.DisplayPrompt("Message", "You need 2 keys to open this door", null, null, "Alright");
            }
            else
            {
                DialogueMenu.Instance.DisplayPrompt("Message", "I'm too large to escape from this door", null, null, "Alright");
            }
        }
        #endregion
    }
}
