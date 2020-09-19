using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    public class Room : MonoBehaviour
    {
        Interactable[] _interactables = null;

        private void Awake()
        {
            _interactables = GetComponentsInChildren<Interactable>();
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