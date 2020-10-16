using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public abstract class GameMenu : MonoBehaviour
    {
        public virtual void OnEnterMenu()
        {

        }

        public virtual void OnLeaveMenu()
        {
        }

        public virtual void OnBackPressed()
        {
            if (GameContext.s_UIMgr != null)
            {
                GameContext.s_UIMgr.CloseCurrentMenu();
            }
        }
    }

    [DisallowMultipleComponent]
    public abstract class GameMenu<MenuType> : GameMenu where MenuType : GameMenu<MenuType>
    {
        private static MenuType _Instance;
        public static MenuType Instance { get { return _Instance; } }

        protected virtual void Awake()
        {
            if (_Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                _Instance = (MenuType)this;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_Instance == this)
            {
                _Instance = null;
            }
        }
    }
}
