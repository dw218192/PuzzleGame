using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
        [SerializeField] UnityEvent<Interactable, Player> _event;
        [SerializeField] EInteractType _type;
        [SerializeField] Color _outlineColor = Color.red;

        [SerializeField] bool _alwaysShowOutline = false;
        [SerializeField] bool _showArrow = false;
        [SerializeField] Transform _arrowIconTransform;

        public EInteractType type { get { return _type; } }
        public Color outlineColor { get { return _outlineColor; } }
        public bool alwaysShowOutline { get { return _alwaysShowOutline; } }

        protected override void Init()
        {
            SetOutline(false);
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