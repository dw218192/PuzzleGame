using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.EventSystem;

namespace PuzzleGame
{
    [CreateAssetMenu(menuName="PuzzleGame/InteractableAction")]
    public class InteractableAction : ScriptableObject
    {
        public void ConsumableInteraction(Interactable interactable, Player player)
        {
            Destroy(interactable.gameObject);
        }

        public void EnterPainting(Interactable interactable, Player player)
        {
            GameContext.s_gameMgr.curRoom.GoToNext();
        }
    }
}
