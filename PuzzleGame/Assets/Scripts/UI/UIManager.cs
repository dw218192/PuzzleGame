using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PuzzleGame.UI
{
    public class UIManager : MonoBehaviour
    {
        Stack<GameMenu> _MenuStack = new Stack<GameMenu>();
        List<GameMenu> _MenuInstances = new List<GameMenu>();

        [SerializeField] Canvas _canvas;
        [SerializeField] DialogueMenu _dialogueMenu;
        [SerializeField] MainMenu _mainMenu;

        private void Awake()
        {
            if (GameContext.s_UIMgr != null)
                Destroy(this);
            else
                GameContext.s_UIMgr = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            InitializeMenus();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void InitializeMenus()
        {
            BindingFlags myFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            FieldInfo[] fields = this.GetType().GetFields(myFlags);

            foreach (FieldInfo field in fields)
            {
                GameMenu prefab = field.GetValue(this) as GameMenu;

                if (prefab != null)
                {
                    GameMenu menuInstance = Instantiate(prefab, _canvas.transform);

                    _MenuInstances.Add(menuInstance);
                }
            }

            foreach (GameMenu menuInstance in _MenuInstances)
            {
                menuInstance.gameObject.SetActive(false);
/*
                if (menuInstance != MainMenu.Instance)
                {
                    menuInstance.gameObject.SetActive(false);
                }
                else
                {
                    OpenMenu(menuInstance);
                }
*/
            }
        }

        public void OpenMenu(GameMenu menuInstance)
        {
            if (menuInstance == null)
            {
                Debug.LogWarning("MenuManager.OpenMenu()- menu instance is null");
                return;
            }

            if (_MenuStack.Count > 0)
            {
                foreach (GameMenu menu in _MenuStack)
                {
                    menu.gameObject.SetActive(false);
                }
            }

            menuInstance.gameObject.SetActive(true);
            menuInstance.OnEnterMenu();
            _MenuStack.Push(menuInstance);
        }
        public void CloseCurrentMenu()
        {
            if (_MenuStack.Count == 0)
            {
                Debug.LogWarning("MenuManager.CloseCurrentMenu()- there is no menu in the stack");
                return;
            }

            GameMenu topMenu = _MenuStack.Pop();
            topMenu.OnLeaveMenu();
            topMenu.gameObject.SetActive(false);

            if (_MenuStack.Count > 0)
            {
                GameMenu nextMenu = _MenuStack.Peek();
                nextMenu.gameObject.SetActive(true);
            }
        }
        public GameMenu GetActiveMenu()
        {
            if (_MenuStack.Count > 0 && _MenuStack.Peek().gameObject.activeSelf)
            {
                return _MenuStack.Peek();
            }
            else
            {
                return null;
            }
        }
    }
}