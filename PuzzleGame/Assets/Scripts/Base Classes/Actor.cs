using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    public class Actor : MonoBehaviour
    {
        public Room room { get; set; } = null;
        public SpriteRenderer spriteRenderer { get; private set; } = null;

        private void Awake()
        {
            //init work before game systems are initialized
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            //put common init work here

            //class dependent init work
            Init();
        }

        protected virtual void Init()
        {

        }
    }
}