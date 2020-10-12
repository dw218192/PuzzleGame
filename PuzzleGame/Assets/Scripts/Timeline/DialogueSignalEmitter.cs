using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace PuzzleGame
{
    public class DialogueSignalEmitter : SignalEmitter
    {
        public string[] dialogues;
        public Constant dialogueId;
    }
}
