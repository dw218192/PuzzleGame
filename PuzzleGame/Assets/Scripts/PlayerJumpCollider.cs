using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    [RequireComponent(typeof(EdgeCollider2D))]
    public class PlayerJumpCollider : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.layer = GameConst.k_boundaryLayer;
            GetComponent<EdgeCollider2D>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            Player player = other.gameObject.GetComponent<Player>();
            if (player)
            {
                player.controller.StopFalling();
            }
        }
    }
}
