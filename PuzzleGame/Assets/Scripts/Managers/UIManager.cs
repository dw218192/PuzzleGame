using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using PuzzleGame.EventSystem;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UIManager : MonoBehaviour
    {
        //a linkedlist that behaves like a stack
        LinkedList<GameMenu> _MenuStack = new LinkedList<GameMenu>();
        List<GameMenu> _MenuInstances = new List<GameMenu>();

        Canvas _canvas;
        [SerializeField] GameObject _cutSceneFrame;
        [SerializeField] DialogueMenu _dialogueMenu;
        [SerializeField] MainMenu _mainMenu;

        private void Awake()
        {
            if (GameContext.s_UIMgr != null)
                Destroy(this);
            else
                GameContext.s_UIMgr = this;

            _canvas = gameObject.GetComponent<Canvas>();

            Messenger.AddListener(M_EventType.ON_CUTSCENE_START, (CutSceneEventData data) => { StartCutScene(); });
            Messenger.AddListener(M_EventType.ON_CUTSCENE_END, (CutSceneEventData data) => { EndCutScene(); });
            EndCutScene();
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
                if (menuInstance != MainMenu.Instance)
                {
                    menuInstance.gameObject.SetActive(false);
                }
                else
                {
                    OpenMenu(menuInstance);
                }
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
                for(var curNode = _MenuStack.First; curNode != null; curNode = curNode.Next)
                {
                    if(ReferenceEquals(curNode.Value, menuInstance))
                    {
                        //already opened
                        _MenuStack.Remove(curNode);
                    }
                    else
                    {
                        curNode.Value.gameObject.SetActive(false);
                    }
                }
            }

            menuInstance.gameObject.SetActive(true);
            menuInstance.OnEnterMenu();
            _MenuStack.AddFirst(menuInstance);
        }
        public void CloseCurrentMenu()
        {
            if (_MenuStack.Count == 0)
            {
                Debug.LogWarning("MenuManager.CloseCurrentMenu()- there is no menu in the stack");
                return;
            }

            GameMenu topMenu = _MenuStack.First.Value;
            _MenuStack.RemoveFirst();
            topMenu.OnLeaveMenu();
            topMenu.gameObject.SetActive(false);

            if (_MenuStack.Count > 0)
            {
                GameMenu nextMenu = _MenuStack.First.Value;
                nextMenu.gameObject.SetActive(true);
            }
        }
        public GameMenu GetActiveMenu()
        {
            if (_MenuStack.Count > 0 && _MenuStack.First.Value.gameObject.activeSelf)
            {
                return _MenuStack.First.Value;
            }
            else
            {
                return null;
            }
        }

        public void StartCutScene()
        {
            _cutSceneFrame.SetActive(true);
        }

        public void EndCutScene()
        {
            _cutSceneFrame.SetActive(false);
        }
    }
}