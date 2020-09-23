using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    [RequireComponent(typeof(PlayerController), typeof(Actor), typeof(BoxCollider))]
    public class Player : MonoBehaviour
    {
        public PlayerController controller { get; private set; }

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            gameObject.layer = GameConst.k_playerLayer;
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
