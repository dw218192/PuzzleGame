using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using PuzzleGame.EventSystem;
using PuzzleGame.UI;
using System;

namespace PuzzleGame
{
    public class Inspectable : Interactable
    {
        [Header("Inspectable Config")]
        [SerializeField] protected DialogueDef _firstEncounterDialogue;
        //world space canvas for details on the object
        [SerializeField] protected InspectionCanvas _worldInspectionCanvas;
        //screen space canvas
        [SerializeField] protected Canvas _screenInspectionCanvas;
        protected bool _canInspect = true;

        protected override void Awake()
        {
            base.Awake();
            _interactionEvent.AddPersistentCall((Action)BeginInspect);
        }

        protected override void Start()
        {
            base.Start();

            if(_screenInspectionCanvas)
            {
                _screenInspectionCanvas.transform.SetParent(null);
                _screenInspectionCanvas.gameObject.SetActive(false);
            }
        }

        public virtual void BeginInspect()
        {
            if (!_canInspect)
                return;

            spriteRenderer.enabled = false;
            
            if(_screenInspectionCanvas)
            {
                _screenInspectionCanvas.gameObject.SetActive(true);
            }

            GameContext.s_UIMgr.OpenMenu(_worldInspectionCanvas);

            if (_firstEncounterDialogue && !_firstEncounterDialogue.hasPlayed)
            {
                DialogueMenu.Instance.DisplayDialogue(_firstEncounterDialogue);
            }
        }

        public virtual void EndInspect()
        {
            spriteRenderer.enabled = true;

            if(_screenInspectionCanvas)
            {
                _screenInspectionCanvas.gameObject.SetActive(false);
            }
        }

        protected virtual void Update()
        {

        }
    }
}