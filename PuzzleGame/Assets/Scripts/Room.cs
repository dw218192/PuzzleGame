using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PuzzleGame
{
    public class Room : MonoBehaviour
    {
        Actor[] _actors = null;
        [SerializeField] GameObject _painting = null;
        Vector2 _paintingSize;
        
        Room _next = null;
        Room _prev = null;

        private void Awake()
        {
            _actors = GetComponentsInChildren<Actor>();
            foreach(var actor in _actors)
            {
                actor.room = this;
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}