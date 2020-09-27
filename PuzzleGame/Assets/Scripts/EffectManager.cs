using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    public class EffectManager : MonoBehaviour
    {
        [SerializeField] GameObject _arrowGameObj;

        private void Awake()
        {
            if (GameContext.s_effectMgr != null)
                Destroy(this);
            else
                GameContext.s_effectMgr = this;
        }

        private void Start()
        {
            _arrowGameObj = Instantiate(_arrowGameObj);
            _arrowGameObj.SetActive(false);
        }

        public void HideAnimtedArrow()
        {
            _arrowGameObj.SetActive(false);
        }

        public void ShowAnimatedArrow(Vector2 position, Vector2 direction)
        {
            _arrowGameObj.SetActive(true);
            _arrowGameObj.transform.position = position;
            _arrowGameObj.transform.up = direction;
        }
    }
}
