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
        public bool canInspect
        {
            get => _canInspect;
            protected set
            {
                Actor[] actors = GameContext.s_gameMgr.GetAllActorsByID(actorId);
                foreach(var actor in actors)
                {
                    Debug.Assert(actor is Inspectable);
                    Inspectable ins = actor as Inspectable;
                    ins._canInspect = value;
                }
            }
        }


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
            if (!canInspect)
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
            Messenger.Broadcast(M_EventType.ON_INSPECTION_START, new InspectionEventData(this));
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
            Messenger.Broadcast(M_EventType.ON_INSPECTION_END, new InspectionEventData(this));
        }

        protected virtual void Update()
        {

        }
    }
}