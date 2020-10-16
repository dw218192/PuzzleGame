using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UltEvents;
using System;

namespace PuzzleGame
{
    public class Inspectable : Interactable
    {
        [Header("Inspectable Config")]
        [SerializeField] protected Camera _inspectCamera;
        [SerializeField] protected Canvas _canvas;
        [SerializeField] protected Button _backButton;

        protected bool _isSuccessful = false;

        protected override void Awake()
        {
            base.Awake();

            _backButton.onClick.AddListener(() =>
            {
                EndInspect();
            });
            _interactionEvent.AddPersistentCall((Action)BeginInspect);

            //start without inspection active
            _inspectCamera.gameObject.SetActive(false);
            _canvas.gameObject.SetActive(false);
        }

        protected override void Start()
        {
            base.Start();
            _inspectCamera.orthographicSize *= room.roomScale;
        }

        public virtual void BeginInspect()
        {
            _inspectCamera.gameObject.SetActive(true);
            _canvas.gameObject.SetActive(true);
        }

        public virtual void EndInspect()
        {
            _inspectCamera.gameObject.SetActive(false);
            _canvas.gameObject.SetActive(false);
        }
    }
}