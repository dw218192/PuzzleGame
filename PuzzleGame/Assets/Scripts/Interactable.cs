using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using PuzzleGame.UI;

namespace PuzzleGame
{
    public enum EInteractType
    {
        MOVABLE,
        GENERIC_EVENT,
        PICK_UP
    }

    public class Interactable : Actor
    {
        //interaction event
        [SerializeField] UnityEvent<Interactable, Player> _event;
        [SerializeField] DialogueDef _interactionDialogue;
        [SerializeField] EInteractType _type;

        //for arrow/outline effects
        [SerializeField] Color _outlineColor = Color.red;
        [SerializeField] bool _alwaysShowOutline = false;
        [SerializeField] ArrowDef _arrowDef = null;
        [SerializeField] bool _animateArrow = false;
        [SerializeField] Transform _arrowIconTransform;

        //for pick-ups only
        [SerializeField] InventoryItemDef _itemDef = null;

        //inspect setting
        [SerializeField] BoolVariable _prerequite;

        public bool canInteract { get { return _prerequite == null || _prerequite.val; } }
        public EInteractType type { get { return _type; } }
        public Color outlineColor { get { return _outlineColor; } }
        public bool alwaysShowOutline { get { return _alwaysShowOutline; } }
        public InventoryItemDef itemDef { get { return _itemDef; } }

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

            if (_interactionDialogue)
                DialogueMenu.Instance.DisplayDialogue(_interactionDialogue);
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