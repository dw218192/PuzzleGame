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
        [SerializeField] protected Camera _inspectionCamera;
        public Camera inspectionCamera { get => _inspectionCamera; }
        
        [SerializeField] protected DialogueDef _firstEncounterDialogue;
        //canvas for details on the object
        [SerializeField] protected InspectionCanvas _inspectionCanvas;
        //screen space canvas
        [SerializeField] protected Canvas _screenCanvas;
        [SerializeField] protected Button _screenBackButton;

        protected bool _canInspect = true;
        public virtual bool canInspect
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
        public override bool canInteract
        {
            get => _inspectionCanvas && _canInspect && base.canInteract;
        }

        protected override void Awake()
        {
            base.Awake();

            _interactionEvent.AddPersistentCall((Action)BeginInspect);
            _screenBackButton.onClick.AddListener(_inspectionCanvas.OnBackPressed);
        }

        protected override void Start()
        {
            base.Start();

            if(_screenCanvas)
            {
                _screenCanvas.transform.SetParent(null);
                _screenCanvas.gameObject.SetActive(false);
            }

            if(_inspectionCanvas)
            {
                _inspectionCanvas.Init(this);
            }
        }

        public virtual void BeginInspect()
        {
            Debug.Assert(_canInspect);

            spriteRenderer.enabled = false;
            
            //open screen space canvas (which are not managed by the UIManager for now)
            if(_screenCanvas)
            {
                _screenCanvas.gameObject.SetActive(true);
            }

            //open world space canvas
            GameContext.s_UIMgr.OpenMenu(_inspectionCanvas);

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
            if(_screenCanvas)
            {
                _screenCanvas.gameObject.SetActive(false);
            }

            //enable player ctrl
            Messenger.Broadcast(M_EventType.ON_CHANGE_PLAYER_CONTROL, new PlayerControlEventData(true));
            Messenger.Broadcast(M_EventType.ON_INSPECTION_END, new InspectionEventData(this));
        }

        protected virtual void Update()
        {

        }

        private void OnDrawGizmos()
        {
            
        }
    }
}