using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    [CreateAssetMenu(menuName = "PuzzleGame/Dialogue")]
    public class DialogueDef : ScriptableObject
    {
        public string[] dialogues;
        //runtime
        public bool hasPlayed { get; set; }

        private void OnEnable()
        {
            hasPlayed = false;
        }
    }
}
