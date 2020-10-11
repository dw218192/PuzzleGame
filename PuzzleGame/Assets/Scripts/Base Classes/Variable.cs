using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    public abstract class Variable<T> : ScriptableObject
    {
        public T defaultValue;
        private T _val;
        /// Current value of the variable
        public T val
        {
            get
            {
                return _val;
            }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_val, value))
                {
                    _val = value;
                    valueChanged?.Invoke(value);
                }
            }
        }

        public event Action<T> valueChanged;
        public void Set(Variable<T> value)
        {
            val = value.val;
        }

        private void OnEnable()
        {
            val = defaultValue;
        }

        public static implicit operator T(Variable<T> variable)
        {
            if (variable == null)
            {
                return default(T);
            }
            return variable.val;
        }
    }
}