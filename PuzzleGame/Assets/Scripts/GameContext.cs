using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// this class is a common entry point to access singleton managers
    /// </summary>
    public static class GameContext
    {
        public static GameManager s_gameMgr;
    }

    /// <summary>
    /// put everything constant in the project here (e.g. layer name, string, etc.)
    /// </summary>
    public static class GameConst
    {
        public const string k_interactableSpriteLayer = "Interactable";
        public const int k_pixelPerWorldUnit = 16;
    }
}