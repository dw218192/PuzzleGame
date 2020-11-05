using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(Canvas))]
    public class ScreenInspectionCanvas : InspectionCanvas
    {
        public override void OnEnterMenu()
        {
            base.OnEnterMenu();
        }
        public override bool enableInspectCamRotation
        {
            get => base.enableInspectCamRotation;
            set
            {
                base.enableInspectCamRotation = value;

                if (value) //camera rotates with the inspectable, so canvas should not rotate (because from the camera's view, the inspectable is not rotated)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).localRotation = Quaternion.identity;
                    }
                }
                else //camera has no rotation, canvas should rotate (because it represents the inspectable)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).localRotation = Quaternion.Euler(0, 0, _inspectable.transform.rotation.eulerAngles.z);
                    }
                }
            }
        }
    }
}