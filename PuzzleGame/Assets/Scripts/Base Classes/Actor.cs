﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    public class Actor : MonoBehaviour
    {
        public Room room { get; set; } = null;
        public SpriteRenderer spriteRenderer 
        {
            get
            {
                if (!_spriteRenderer)
                    _spriteRenderer = GetComponent<SpriteRenderer>();
                return _spriteRenderer;
            }
        }
        private SpriteRenderer _spriteRenderer = null;

        private void Awake()
        {
            //init work before game systems are initialized
            gameObject.layer = GameConst.k_propLayer;
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