using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    [RequireComponent(typeof(Collider2D))]
    public class GenericTrigger : MonoBehaviour
    {
        public delegate void TriggerHandler(Collider2D collider);
        private TriggerHandler _onTriggerEnter;
        private TriggerHandler _onTriggerStay;
        private TriggerHandler _onTriggerExit;

        private void Set(TriggerHandler func, ref TriggerHandler mem)
        {
            if(mem != null)
                throw new InvalidOperationException("Only one eventhandler is supported");
            mem = func;
        }
        private void UnSet(TriggerHandler func, ref TriggerHandler mem)
        {
            // you might want to check if the delegate matches the current.
            if (func == null || func == mem)
                mem = null;
            else
                throw new InvalidOperationException("Unable to unregister, wrong eventhandler");
        }

        public event TriggerHandler onTriggerEnter
        {
            add => Set(value, ref _onTriggerEnter);
            remove => UnSet(value, ref _onTriggerEnter);
        }
        public event TriggerHandler onTriggerStay
        {
            add => Set(value, ref _onTriggerStay);
            remove => UnSet(value, ref _onTriggerStay);
        }
        public event TriggerHandler onTriggerExit
        {
            add => Set(value, ref _onTriggerExit);
            remove => UnSet(value, ref _onTriggerExit);
        }

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            _onTriggerEnter?.Invoke(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            _onTriggerStay?.Invoke(collision);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            _onTriggerExit?.Invoke(collision);
        }
    }
}
