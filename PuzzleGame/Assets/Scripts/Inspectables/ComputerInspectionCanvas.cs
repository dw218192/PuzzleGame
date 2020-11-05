using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(Canvas))]
    public class ComputerInspectionCanvas : ScreenInspectionCanvas
    {
        class WindowOrderComp : IComparer<ComputerWindow>
        {
            int IComparer<ComputerWindow>.Compare(ComputerWindow x, ComputerWindow y)
            {
                return x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex());
            }
        }

        [SerializeField] ComputerWindow[] _windows;
        LinkedList<ComputerWindow> _windowStack;

        protected override void Awake()
        {
            base.Awake();
            _windowStack = new LinkedList<ComputerWindow>();
        }

        public override void Init(Inspectable inspectable)
        {
            base.Init(inspectable);

            //sorted by ascending sibling index
            Array.Sort(_windows, new WindowOrderComp());

            foreach (var win in _windows)
            {
                if (!win)
                    continue;

                if (win.gameObject.activeSelf)
                    _windowStack.AddLast(win);
            }
        }

        public void CloseWindow()
        {
            if (_windowStack.Count > 0)
            {
                _windowStack.Last.Value.gameObject.SetActive(false);
                _windowStack.RemoveLast();
            }
        }

        public void OpenOrFocusWindow(ComputerWindow window)
        {
            bool isFocus = false;
            for (var node = _windowStack.First; node != null; node = node.Next)
            {
                if (ReferenceEquals(window, node.Value))
                {
                    _windowStack.Remove(node);
                    isFocus = true;
                    break;
                }
            }

            _windowStack.AddLast(window);
            window.transform.SetAsLastSibling();

            if (!isFocus)
            {
                window.gameObject.SetActive(true);
                GameActions.PlaySounds(window.windowOpenSound);
            }
        }
    }
}