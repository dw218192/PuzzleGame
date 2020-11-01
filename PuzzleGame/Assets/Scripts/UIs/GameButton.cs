using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UltEvents;

namespace PuzzleGame.UI
{
    public class GameButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] AudioClip _clickSound;
        [SerializeField] UltEvent _additionalOnClickEvents;

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            GameActions.PlaySounds(_clickSound);
            _additionalOnClickEvents?.Invoke();
        }
    }
}
