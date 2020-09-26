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
    }

    public class Interactable : Actor
    {
        [SerializeField] UnityEvent<Interactable, Player> _event;
        [SerializeField] EInteractType _type;
        public EInteractType type { get { return _type; } set { _type = value; } }

        protected override void Init()
        {
            
        }

        public void Interact(Player player)
        {
            _event.Invoke(this, player);
        }
    }
}