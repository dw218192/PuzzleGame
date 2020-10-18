using UnityEngine;
using UltEvents;

namespace PuzzleGame
{
    public enum EInteractType
    {
        HOLD,
        PRESS
    }

    public class Interactable : Actor
    {
        /// <summary>
        /// Interaction Events with static parameters (pre-configured during edit-time)
        /// </summary>
        [SerializeField] protected UltEvent _interactionEvent;
        /// <summary>
        /// Prerequites for interaction
        /// </summary>
        [SerializeField] Condition _prerequisite;

        //for arrow/outline effects
        [SerializeField] Color _outlineColor = Color.red;
        [SerializeField] bool _showOutline = false;
        [SerializeField] ArrowDef _arrowDef = null;
        [SerializeField] bool _animateArrow = false;
        [SerializeField] Transform _arrowIconTransform;

        //for pick-ups only
        [SerializeField] InventoryItemDef _itemDef = null;

        public bool canInteract { 
            get 
            {
                return _prerequisite == null || _prerequisite.Evaluate(); 
            }
        }

        public Color outlineColor { get { return _outlineColor; } }
        public InventoryItemDef itemDef { get { return _itemDef; } }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            if (spriteRenderer && spriteRenderer.sprite)
                SetOutline(false);
        }

        public void OnEnterRange()
        {
            //if we're a pick-up item or outline is forced to show
            if (_showOutline)
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
                    Quaternion.LookRotation(_arrowIconTransform.forward, _arrowIconTransform.up), _arrowDef.flipX, _arrowDef.flipY);
            }
        }

        public void OnInteract()
        {
            _interactionEvent?.Invoke();
        }

        public void OnExitRange()
        {
            if (_showOutline)
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