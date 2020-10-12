using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    public class Actor : MonoBehaviour, IAnimationClipSource
    {
        [SerializeField] AnimationClip[] _animClips;
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
        public Rigidbody2D actorRigidBody
        {
            get
            {
                if (!_rigidBody)
                    _rigidBody = GetComponent<Rigidbody2D>();
                return _rigidBody;
            }
        }
        public Collider2D actorCollider
        {
            get
            {
                if (!_collider)
                    _collider = GetComponent<Collider2D>();
                return _collider;
            }
        }
        private SpriteRenderer _spriteRenderer = null;
        private Collider2D _collider = null;
        private Rigidbody2D _rigidBody = null;

        protected virtual void Awake()
        {
            //init work before game systems are initialized
        }

        protected virtual void Start()
        {
            //put common init work here
        }

        void IAnimationClipSource.GetAnimationClips(List<AnimationClip> results)
        {
            if(_animClips != null)
                results.AddRange(_animClips);
        }
    }
}