using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(Canvas))]
    public class WorldInspectionCanvas : InspectionCanvas
    {
        public override void Init(Inspectable inspectable)
        {
            base.Init(inspectable);

            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = inspectable.inspectionCamera;
        }
    }
}
