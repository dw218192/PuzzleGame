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
        public string title = "Message";
        [TextArea]
        public string prompt = "Message Content";
        public Sprite promptImage;
        public PromptOptionDesc[] options;
        public bool hasBackButton = true;
        public string backButtonName = "OK";

        //runtime
        public bool hasPlayed { get; set; }
        private void OnEnable()
        {
            hasPlayed = false;
        }
    }
}
