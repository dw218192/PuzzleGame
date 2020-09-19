using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    public static class ScreenSize
    {
        public static float ScreenToWorldHeight
        {
            get
            {
                Vector2 topRightCorner = new Vector2(1, 1);
                Vector2 edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);
                var height = edgeVector.y * 2;
                return height;
            }
        }
        public static float ScreenToWorldWidth
        {
            get
            {
                Vector2 topRightCorner = new Vector2(1, 1);
                Vector2 edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);
                var width = edgeVector.x * 2;
                return width;
            }
        }
    }
}
