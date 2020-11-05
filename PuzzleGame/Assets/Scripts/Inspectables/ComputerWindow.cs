using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using PuzzleGame.UI;

namespace PuzzleGame
{
    [RequireComponent(typeof(DraggableWindow))]
    public class ComputerWindow : MonoBehaviour
    {
        [SerializeField] Button _closeButton;
        [SerializeField] Button _okButton;
        [SerializeField] [TextArea] string _info;

        [SerializeField] AudioClip _windowOpenSound;
        public AudioClip windowOpenSound { get => _windowOpenSound; }
    }
}
