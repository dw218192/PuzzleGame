using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class InspectionCanvas : MonoBehaviour, IGameMenu
    {
        /// <summary>
        /// UI elements that do not rotate with the canvas
        /// </summary>
        [SerializeField] bool _enableInspectCamRot = false;
        [SerializeField] Camera _inspectCamera;
        [SerializeField] Inspectable _inspectable;

        public bool enableInspectCamRotation 
        { 
            get => _enableInspectCamRot; 
            set 
            { 
                if(value != _enableInspectCamRot)
                {
                    if(value)
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
        }

        protected virtual void Start()
        {
            _inspectCamera.gameObject.SetActive(false);
            gameObject.SetActive(false);

            _inspectCamera.orthographicSize *= _inspectable.room.roomScale;

            GameContext.s_UIMgr.RegisterMenu(this);
        }

        public void OnBackPressed()
        {
            if (GameContext.s_UIMgr != null && ReferenceEquals(GameContext.s_UIMgr.GetActiveMenu(), this))
            {
                GameContext.s_UIMgr.CloseCurrentMenu();
            }
        }

        public void OnEnterMenu()
        {
            _inspectCamera.gameObject.SetActive(true);
            gameObject.SetActive(true);

            if(!_enableInspectCamRot)
            {
                _inspectCamera.transform.rotation = Quaternion.identity;
            }
            else
            {
                _inspectCamera.transform.localRotation = Quaternion.identity;
            }
        }

        public void OnLeaveMenu()
        {
            _inspectCamera.gameObject.SetActive(false);
            gameObject.SetActive(false);
            _inspectable.EndInspect();
        }
    }
}
