using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using PuzzleGame.EventSystem;
using PuzzleGame.UI;
using UnityEngine.Events;

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

            int keysLeft = 2;
            if(player.inventory.Count > 0)
            {
                keysLeft = 2 - player.inventory[0].quantity;
            }

            if (curRoom.roomIndex == GameConst.k_startingRoomIndex)
            {
                if(keysLeft > 0)
                {
                    DialogueMenu.Instance.DisplayPrompt("Message", $"You need <color=red>{keysLeft}</color> more key(s) to open this door", null, null, "Alright");
                }
                else
                {
                    Button.ButtonClickedEvent option1 = new Button.ButtonClickedEvent();
                    option1.AddListener(GameContext.s_gameMgr.QuitGame);

                    (string, Button.ButtonClickedEvent)[] options =
                    {
                        ("Yes", option1),
                    };
                    DialogueMenu.Instance.DisplayPrompt("Message", "insert all keys?", null, options, "No");
                }
            }
            else
            {
                DialogueMenu.Instance.DisplayPrompt("Message", "I'm too large to escape from this door", null, null, "Alright");
            }
        }
        #endregion
    }
}
