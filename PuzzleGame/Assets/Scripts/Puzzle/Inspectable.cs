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
            
            //open screen space canvas (which are not managed by the UIManager for now)
            if(_screenInspectionCanvas)
            {
                _screenInspectionCanvas.gameObject.SetActive(true);
            }

            //open world space canvas
            GameContext.s_UIMgr.OpenMenu(_worldInspectionCanvas);

            //display first dialogue
            if (_firstEncounterDialogue && !_firstEncounterDialogue.hasPlayed)
            {
                DialogueMenu.Instance.DisplayDialogue(_firstEncounterDialogue);
            }

            Messenger.Broadcast(M_EventType.ON_CHANGE_PLAYER_CONTROL, new PlayerControlEventData(false));
        }


        /// <summary>
        /// this is called from the back button of screen space canvas
        /// </summary>
        public virtual void EndInspect()
        {
            spriteRenderer.enabled = true;

            //disable screen space
            if(_screenInspectionCanvas)
            {
                _screenInspectionCanvas.gameObject.SetActive(false);
            }

            //enable player ctrl
            Messenger.Broadcast(M_EventType.ON_CHANGE_PLAYER_CONTROL, new PlayerControlEventData(true));
        }

        protected virtual void Update()
        {

        }
    }
}