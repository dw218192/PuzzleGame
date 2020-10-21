using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class OutlineButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        Outline _outline;
        private void Awake()
        {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _outline.enabled = true;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _outline.enabled = false;
        }
    }
}
