using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    [CreateAssetMenu(menuName = "PuzzleGame/InventoryItem")]
    public class InventoryItemDef : ScriptableObject
    {
        public Sprite inventoryDisplaySprite;
        public string description;
    }
}
