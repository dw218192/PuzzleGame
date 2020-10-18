using PuzzleGame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace PuzzleGame
{
    /// <summary>
    /// contains wrapper functions around game classes to provide game logic building blocks
    /// </summary>
    public static class GameActions
    {
        public static void EnterPainting(Condition condition)
        {
            if (condition && !condition.Evaluate())
            {
                return;
            }
            
            Room curRoom = GameContext.s_gameMgr.curRoom;

            if (curRoom.roomIndex == GameConst.k_maxRoomIndex - 1)
            {
                DialogueMenu.Instance.DisplayPromptOneShot("Message", "I am too large to fit into this room", null, null, "Ok then");
            }
            else
            {
                curRoom.GoToNext();
            }
        }

        public static void RotatePainting(bool clockwise)
        {
            Room curRoom = GameContext.s_gameMgr.curRoom;
            curRoom.RotateNext(clockwise);
        }

        public static void AddToInventory(InventoryItemDef inventoryItem, int quantity)
        {
            Debug.Assert(GameContext.s_player);
            GameContext.s_player.AddToInventory(inventoryItem, quantity);
        }
        public static void ConsumeItem(InventoryItemDef item, int quantity)
        {
            GameContext.s_player.RemoveFromInventory(item, quantity);
        }
        public static void PickupItem(InventoryItemDef item, int quantity)
        {
            Room curRoom = GameContext.s_gameMgr.curRoom;
            curRoom.RemoveItemAll(item);
            AddToInventory(item, quantity);
        }
        public static void DisplayDialogue(DialogueDef dialogue)
        {
            Debug.Assert(DialogueMenu.Instance);
            DialogueMenu.Instance.DisplayDialogue(dialogue);
        }
        public static void DisplayDialogue(BoolVariable condition, DialogueDef dialogue)
        {
            DisplayDialogue(condition.val, dialogue);
        }
        public static void DisplayDialogue(Condition condition, DialogueDef dialogue)
        {
            DisplayDialogue(condition == null || condition.Evaluate(), dialogue);
        }
        public static void DisplayDialogue(bool condition, DialogueDef dialogue)
        {
            if (condition)
            {
                DisplayDialogue(dialogue);
            }
        }
        public static void DisplayPrompt(PromptDef prompt)
        {
            Debug.Assert(DialogueMenu.Instance);
            DialogueMenu.Instance.DisplayPrompt(prompt);
        }
        public static void DisplayPrompt(BoolVariable condition, PromptDef prompt)
        {
            DisplayPrompt(condition.val, prompt);
        }
        public static void DisplayPrompt(Condition condition, PromptDef prompt)
        {
            DisplayPrompt(condition == null || condition.Evaluate(), prompt);
        }
        public static void DisplayPrompt(bool condition, PromptDef prompt)
        {
            if (condition)
            {
                DisplayPrompt(prompt);
            }
        }
        public static void ClosePrompt(BoolVariable condition)
        {
            ClosePrompt(condition.val);
        }
        public static void ClosePrompt(bool condition)
        {
            if(condition && ReferenceEquals(GameContext.s_UIMgr.GetActiveMenu(), DialogueMenu.Instance))
            {
                DialogueMenu.Instance.ClosePrompt();
            }
        }
        public static void PlayCutscene(BoolVariable condition, TimelineAsset timeline)
        {
            PlayCutscene(condition.val, timeline);
        }
        public static void PlayCutscene(Condition condition, TimelineAsset timeline)
        {
            PlayCutscene(condition == null || condition.Evaluate(), timeline);
        }
        public static void PlayCutscene(bool condition, TimelineAsset timeline)
        {
            Room curRoom = GameContext.s_gameMgr.curRoom;

            if (condition)
            {
                curRoom.PlayCutScene(timeline);
            }
        }
        public static void SetBoolean(BoolVariable variable, bool value)
        {
            variable.val = value;
        }
        public static bool IsInRoom(int index)
        {
            return GameContext.s_gameMgr.curRoom.roomIndex == index;
        }
        public static bool HasPlayed(DialogueDef dialogue)
        {
            return dialogue.hasPlayed;
        }
        public static bool HasPlayed(PromptDef prompt)
        {
            return prompt.hasPlayed;
        }
        public static bool PlayerHasItem(InventoryItemDef requiredItem, int minQuantity)
        {
            return GameContext.s_player.HasItem(requiredItem, minQuantity);
        }
        public static void UnlockWithRequirement(BoolVariable unlockVariable, InventoryItemDef requiredItem, int minQuantity, PromptDef failPrompt)
        {
            if (unlockVariable.val)
            {
                return;
            }

            if (PlayerHasItem(requiredItem, minQuantity))
            {
                unlockVariable.val = true;
                ConsumeItem(requiredItem, minQuantity);
            }
            else
            {
                DialogueMenu.Instance.DisplayPrompt(failPrompt);
            }
        }

        #region DEMO
        public static void DoorInteraction(InventoryItemDef requiredItem, int quantity, PromptDef failPrompt)
        {
            if (PlayerHasItem(requiredItem, quantity))
            {
                GameContext.s_gameMgr.QuitGame();
            }
            else
            {
                DialogueMenu.Instance.DisplayPrompt(failPrompt);
            }
        }
        #endregion
    }
}
