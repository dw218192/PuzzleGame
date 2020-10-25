using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PuzzleGame.UI
{
    public class InventorySlot : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        const float s_itemDragScaleOffset = 7f;
        public static InventoryItem s_curDraggingItem { get; private set; }

        [SerializeField] Image _itemImage;
        [SerializeField] Text _quantityText;
        [SerializeField] Button _slotButton;

        //runtime data
        InventoryItem _item;

        //dragging
        RectTransform _itemImgRect;
        Vector2 _originalItemImgPosLocal;
        Vector3 _originalScale;

        private void Awake()
        {
            _slotButton.onClick.AddListener(OnClick);
        }
        private void Start()
        {
            _itemImgRect = _itemImage.GetComponent<RectTransform>();
            _originalItemImgPosLocal = _itemImage.transform.localPosition;
            _originalScale = _itemImage.transform.localScale;
        }
        private void OnClick()
        {
            if(_itemImage.enabled)
            {
                const string fmt = "0.#########################";
                string description = $"{_item.def.description}\n\n\n<size=17>item scale in current room = <color=red>{(_item.GetRoomRelativeScale()).ToString(fmt)}x </color></size>";
                DialogueMenu.Instance.DisplayPromptOneShot("Item Description", description, _itemImage.sprite, null, "Back");
            }
        }
        public void Set(InventoryItem item)
        {
            _item = item;
            _itemImage.enabled = _quantityText.enabled = true;

            _itemImage.sprite = _item.def.inventoryDisplaySprite;
            _quantityText.text = "x" + _item.quantity.ToString();
        }
        public void UnSet()
        {
            _itemImage.enabled = _quantityText.enabled = false;
        }

        private bool IsRectTransformInsideSreen(RectTransform rectTransform)
        {
            bool isInside = false;
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            int visibleCorners = 0;
            Rect rect = new Rect(0, 0, Screen.width, Screen.height);
            foreach (Vector3 corner in corners)
            {
                if (rect.Contains(corner))
                {
                    visibleCorners++;
                }
            }
            if (visibleCorners == 4)
            {
                isInside = true;
            }
            return isInside;
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!_itemImage.enabled)
                return;

            _itemImgRect.position = eventData.position;

            if (!IsRectTransformInsideSreen(_itemImgRect))
            {
                _itemImgRect.localPosition = _originalItemImgPosLocal;
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            _itemImgRect.localScale = _originalScale;
            _itemImgRect.localPosition = _originalItemImgPosLocal;
            s_curDraggingItem = null;
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!_itemImage.enabled)
                return;

            Debug.Assert(s_curDraggingItem == null);
            _itemImgRect.localScale = Vector3.one * _item.GetRoomRelativeScale() * s_itemDragScaleOffset;
            s_curDraggingItem = _item;
        }
    }
}
