using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using PuzzleGame.EventSystem;

namespace PuzzleGame.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] GameObject[] _itemSlots;
        
        struct SlotDisplayData
        {
            private Image _displayImg;
            private Text _quantityText;

            public SlotDisplayData(Image displayImg, Text quantityText)
            {
                _displayImg = displayImg;
                _quantityText = quantityText;

                _displayImg.enabled = _quantityText.enabled = false;
            }
            public void Set(Sprite sprite, int quantity)
            {
                _displayImg.enabled = _quantityText.enabled = true;

                _displayImg.sprite = sprite;
                _quantityText.text = quantity.ToString();
            }
            public void UnSet()
            {
                _displayImg.enabled = _quantityText.enabled = false;
            }
        }

        SlotDisplayData[] _slotDisplayData;
        int _numSlots;

        private void Awake()
        {
            Messenger.AddListener<InventoryChangeEventData>(M_EventType.ON_INVENTORY_CHANGE, OnInventoryChange);

            _numSlots = _itemSlots.Length;
            _slotDisplayData = new SlotDisplayData[_numSlots];

            for (int i=0; i< _numSlots; i++)
            {
                Image itemImg = _itemSlots[i].GetComponentInChildren<Image>();
                Text quantityText = _itemSlots[i].GetComponentInChildren<Text>();

                _slotDisplayData[i] = new SlotDisplayData(itemImg, quantityText);
            }
        }

        void OnInventoryChange(InventoryChangeEventData data)
        {
            Assert.IsTrue( data.slotIndex >= 0 && data.slotIndex < _numSlots && data.curItemQuantity > 0);

            //item not found
            if (data.curItemQuantity == 0)
            {
                _slotDisplayData[data.slotIndex].UnSet();
            }
            else
            {
                ItemDatabase.ItemData itemData = ItemDatabase.GetItemData(data.itemID);
                _slotDisplayData[data.slotIndex].Set(
                    itemData.inventoryIcon,
                    data.curItemQuantity);
            }
        }
    }
}
