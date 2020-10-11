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
        //interaction event
        [SerializeField] UnityEvent<Interactable, Player> _event;
        [SerializeField] EInteractType _type;

        //for arrow/outline effects
        [SerializeField] Color _outlineColor = Color.red;
        [SerializeField] bool _alwaysShowOutline = false;
        [SerializeField] ArrowDef _arrowDef = null;
        [SerializeField] bool _animateArrow = false;
        [SerializeField] Transform _arrowIconTransform;

        //for pick-ups only
        [SerializeField] EItemID _itemID = EItemID.INVALID;

        //inspect setting
        [SerializeField] BoolVariable _prerequite;

        public bool canInteract { get { return _prerequite == null || _prerequite.val; } }
        public EInteractType type { get { return _type; } }
        public Color outlineColor { get { return _outlineColor; } }
        public bool alwaysShowOutline { get { return _alwaysShowOutline; } }
        public EItemID itemID { get { return _itemID; } }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            /*
            if (_prerequite != null)
            {
                Debug.Log($"_prerequite.val={_prerequite.val}\n!_prerequite || _prerequite.val={!_prerequite || _prerequite.val}");
            }
            */
            if (spriteRenderer && spriteRenderer.sprite)
                SetOutline(false);
        }

        public override void RoomInit()
        {
            base.RoomInit();

            //if this interactable has an id (usually used for pick-ups)
            //id is the same regardless of the room level
            if (itemID != EItemID.INVALID)
                ItemDatabase.RegisterItem(itemID, new ItemDatabase.ItemData() { inventoryIcon = spriteRenderer.sprite });
        }

        public void OnEnterRange()
        {
            //if we're a pick-up item or outline is forced to show
            if (type == EInteractType.PICK_UP || alwaysShowOutline)
            {
                if (spriteRenderer && spriteRenderer.sprite)
                {
                    SetOutline(true);
                }
            }
            
            //if animated arrow effect is enabled on this item
            if (_arrowDef && _arrowIconTransform)
            {
                GameContext.s_effectMgr.ShowArrow(_arrowDef, _animateArrow, _arrowIconTransform.position,
                    Quaternion.LookRotation(_arrowIconTransform.forward, _arrowIconTransform.up));
            }
        }

        public void OnInteract(Player player)
        {
            _event.Invoke(this, player);
        }

        public void OnExitRange()
        {
            if (type == EInteractType.PICK_UP || alwaysShowOutline)
                SetOutline(false);

            if (_arrowDef && _arrowIconTransform)
                GameContext.s_effectMgr.HideArrow();
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