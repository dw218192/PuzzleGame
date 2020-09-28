using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

namespace PuzzleGame
{
    public enum EInteractType
    {
        MOVABLE,
        GENERIC_EVENT,
        PICK_UP
    }

    public enum EItemID
    {
        INVALID,
        KEY_1,
        KEY_2,
        KEY_3,
        KEY_4,
        KEY_5
    }

    public static class ItemDatabase
    {
        public struct ItemData
        {
            public Sprite inventoryIcon;
            public string description;
        }

        static Dictionary<EItemID, ItemData> _itemDataMap = new Dictionary<EItemID, ItemData>();

        public static void RegisterItem(EItemID id, ItemData data)
        {
            Assert.IsTrue(!_itemDataMap.ContainsKey(id));
            _itemDataMap.Add(id, data);
        }
        public static ItemData GetItemData(EItemID id)
        {
            Assert.IsTrue(_itemDataMap.ContainsKey(id));
            return _itemDataMap[id];
        }
    }

    public class Interactable : Actor
    {
        [SerializeField] UnityEvent<Interactable, Player> _event;
        [SerializeField] EInteractType _type;
        [SerializeField] Color _outlineColor = Color.red;

        [SerializeField] bool _alwaysShowOutline = false;
        [SerializeField] bool _showArrow = false;
        [SerializeField] Transform _arrowIconTransform;
        [SerializeField] EItemID _itemID = EItemID.INVALID;

        public EInteractType type { get { return _type; } }
        public Color outlineColor { get { return _outlineColor; } }
        public bool alwaysShowOutline { get { return _alwaysShowOutline; } }
        public EItemID itemID { get { return _itemID; } }

        protected override void Start()
        {
            base.Start();
            SetOutline(false);
        }

        public override void RoomInit()
        {
            base.RoomInit();

            if (itemID != EItemID.INVALID)
                ItemDatabase.RegisterItem(itemID, new ItemDatabase.ItemData() { inventoryIcon = spriteRenderer.sprite });
        }

        public void OnEnterRange()
        {
            if (type == EInteractType.PICK_UP || alwaysShowOutline)
                SetOutline(true);
            if (_showArrow && _arrowIconTransform)
                GameContext.s_effectMgr.ShowAnimatedArrow(_arrowIconTransform.position, _arrowIconTransform.up);
        }

        public void OnInteract(Player player)
        {
            _event.Invoke(this, player);
        }

        public void OnExitRange()
        {
            if (type == EInteractType.PICK_UP || alwaysShowOutline)
                SetOutline(false);
            if (_showArrow && _arrowIconTransform)
                GameContext.s_effectMgr.HideAnimtedArrow();
        }

        public void SetOutline(bool enable)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat("_Outline", enable ? 1f : 0f);
            if(enable)
                mpb.SetColor("_OutlineColor", outlineColor);
            spriteRenderer.SetPropertyBlock(mpb);
        }
    }
}