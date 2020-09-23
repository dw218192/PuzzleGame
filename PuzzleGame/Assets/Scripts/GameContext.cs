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
        public static Vector2 s_right = Vector2.right, s_up = Vector2.up;
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
        //      ground collides with props but does not collide with player or jumping player
        //      wall   collides with props and player but does not collide with jumping player
        //      props  collide with player and jumping player

        public const int k_playerLayer = 8;
        public const int k_playerJumpingLayer = 9;
        public const int k_propLayer = 10;
        public const int k_wallLayer = 11;
        public const int k_groundLayer = 12; //not used for now
        public const int k_boundaryLayer = 13;
    }
}