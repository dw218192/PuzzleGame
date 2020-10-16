using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField] Image _itemImage;
        [SerializeField] Text _quantityText;
        [SerializeField] Button _slotButton;

        string _itemDesc;
        
        private void Awake()
        {
            _slotButton.onClick.AddListener(OnClick);
        }
        private void OnClick()
        {
            if(_itemImage.enabled)
                DialogueMenu.Instance.DisplayPromptOneShot("Item Description", _itemDesc, _itemImage.sprite, null, "Back");
        }
        public void Set(InventoryItemDef def, int quantity)
        {
            _itemImage.enabled = _quantityText.enabled = true;
            _itemDesc = def.description;
            _itemImage.sprite = def.inventoryDisplaySprite;

            _quantityText.text = "x" + quantity.ToString();
        }
        public void UnSet()
        {
            _itemImage.enabled = _quantityText.enabled = false;
        }
    }
}
