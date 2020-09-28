using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.EventSystem;
using PuzzleGame.UI;

namespace PuzzleGame
{
    [CreateAssetMenu(menuName="PuzzleGame/InteractableAction")]
    public class InteractableAction : ScriptableObject
    {
        public void ConsumableInteraction(Interactable interactable, Player player)
        {
            GameContext.s_gameMgr.curRoom.RemoveItemAll(interactable.itemID);
            player.AddToInventory(interactable.itemID, 1);
        }

        public void EnterPainting(Interactable interactable, Player player)
        {
            GameContext.s_gameMgr.curRoom.GoToNext();
        }


        public void VerticalSliceEscape(Interactable interactable, Player player)
        {
            if(GameContext.s_gameMgr.curRoom.roomIndex != 0)
            {
                DialogueMenu.Instance.VerticalSliceWrongRoomPrompt();
            }
            else
            {
                if (player.inventory.Count == 0)
                {
                    DialogueMenu.Instance.VerticalSliceNoooKeyPrompt();
                }
                else
                {
                    DialogueMenu.Instance.VerticalSliceKeyPrompt();
                }
            }

            GameContext.s_UIMgr.OpenMenu(DialogueMenu.Instance);
        }
    }
}
