using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public interface IGameMenu
    {
        void OnEnterMenu();
        void OnLeaveMenu();
        void OnBackPressed();
    }

    public abstract class GameMenu : MonoBehaviour, IGameMenu
    {
        protected virtual void Start()
        {
            gameObject.SetActive(false);
            GameContext.s_UIMgr.RegisterMenu(this);
        }

        public virtual void OnEnterMenu()
        {
            gameObject.SetActive(true);
        }

        public virtual void OnLeaveMenu()
        {
            gameObject.SetActive(false);
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
    public abstract class SingletonGameMenu<MenuType> : GameMenu where MenuType : SingletonGameMenu<MenuType>
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
