using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PuzzleGame
{
    public enum EInteractType
    {

    }

    public class Interactable : Actor
    {
        [SerializeField] EInteractType _type;
        public EInteractType type { get { return _type; } set { _type = value; } }

        protected override void Init()
        {
            
        }
    }
}