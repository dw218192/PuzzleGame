using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame
{
    [RequireComponent(typeof(Collider2D), typeof(Image))]
    public class SpeakerPuzzleNode : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _contentText;
        Image _img;
        public char letter { get { return _contentText.text[0]; } }

        private void Awake()
        {
            _img = GetComponent<Image>();
        }

        public void SetColor(Color color)
        {
            _img.color = color;
        }
    }
}
