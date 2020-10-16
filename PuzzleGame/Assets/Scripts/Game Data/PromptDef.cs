using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UltEvents;

namespace PuzzleGame
{
    [Serializable]
    public class PromptOptionDesc
    {
        public string optionName;
        public UltEvent optionEvents;
    }

    [CreateAssetMenu(menuName = "PuzzleGame/Prompt")]
    public class PromptDef : ScriptableObject
    {
        public string title;
        public string prompt;
        public Sprite promptImage;
        public PromptOptionDesc[] options;
        public string backButtonName;

        //runtime
        public bool hasPlayed { get; set; }
        private void OnEnable()
        {
            hasPlayed = false;
        }
    }
}
