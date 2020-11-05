using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(Canvas))]
    public abstract class InspectionCanvas : MonoBehaviour, IGameMenu
    {
        /// <summary>
        /// UI elements that do not rotate with the canvas
        /// </summary>
        [SerializeField] bool _enableInspectCamRot = false;
        public virtual bool enableInspectCamRotation 
        { 
            get 
            { 
                return _enableInspectCamRot; 
            }
            set
            {
                if (value)
                {
                    _inspectCamera.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    _inspectCamera.transform.rotation = Quaternion.identity;
                }
                _enableInspectCamRot = value;
            }
        }

        protected Camera _inspectCamera;
        protected Inspectable _inspectable;

        public virtual void Init(Inspectable inspectable)
        {
            _inspectable = inspectable;

            _inspectCamera = inspectable.inspectionCamera;
            _inspectCamera.gameObject.SetActive(false);

            _inspectCamera.orthographic = true;
            _inspectCamera.cullingMask = ~(1 << GameConst.k_playerLayer);

            _inspectCamera.orthographicSize *= inspectable.room.roomScale;

            gameObject.SetActive(false);
            GameContext.s_UIMgr.RegisterMenu(this);
        }

        protected virtual void Awake()
        {

        }

        protected virtual void Start()
        {

        }

        public virtual void OnBackPressed()
        {
            if (GameContext.s_UIMgr != null && ReferenceEquals(GameContext.s_UIMgr.GetActiveMenu(), this))
            {
                GameContext.s_UIMgr.CloseCurrentMenu();
            }
        }

        public virtual void OnEnterMenu()
        {
            enableInspectCamRotation = _enableInspectCamRot;

            _inspectCamera.gameObject.SetActive(true);
            gameObject.SetActive(true);
        }

        public virtual void OnLeaveMenu()
        {
            _inspectCamera.gameObject.SetActive(false);
            gameObject.SetActive(false);
            _inspectable.EndInspect();
        }
    }
}