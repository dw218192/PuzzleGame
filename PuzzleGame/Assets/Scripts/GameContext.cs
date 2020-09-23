using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// this class is a common entry point to access global variables
    /// </summary>
    public static class GameContext
    {
        public static GameManager s_gameMgr;
        public static Player s_player;
    }

    /// <summary>
    /// put everything constant in the project here (e.g. layer name, string, etc.)
    /// </summary>
    public static class GameConst
    {
        public const string k_interactableSpriteLayer = "Interactable";
        public const int k_pixelPerWorldUnit = 16;

        //layers are set in project and used for collision rules
        //rule: boundary collides with everything
        //      tile only collides with props
        //      player collides with props
        //      playerJumping only collides with boundary
        public const int k_playerLayer = 8;
        public const int k_playerJumpingLayer = 9;
        public const int k_propLayer = 10;
        public const int k_tileLayer = 11;
        public const int k_boundaryLayer = 12;
    }
}