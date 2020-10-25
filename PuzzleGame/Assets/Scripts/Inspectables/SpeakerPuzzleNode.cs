using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame
{
    [RequireComponent(typeof(Collider2D), typeof(Image))]
    public class SpeakerPuzzleNode : MonoBehaviour
    {
        [SerializeField] Text _contentText;
        Collider2D _collider;
        Image _img;
        public char letter { get { return _contentText.text[0]; } }

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _img = GetComponent<Image>();
        }

        public void SetColor(Color color)
        {
            _img.color = color;
        }
    }
}
