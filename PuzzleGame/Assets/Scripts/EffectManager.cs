using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// entry point for all visual effect sprites
    /// for now all effect sprites are "singletons"
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        [SerializeField] GameObject _arrowGameObj;
        [SerializeField] ProgressBar _progressBar;

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
            _progressBar = Instantiate(_progressBar.gameObject).GetComponent<ProgressBar>();

            _arrowGameObj.SetActive(false);
            _progressBar.gameObject.SetActive(false);
        }

        public void HideAnimtedArrow()
        {
            _arrowGameObj.SetActive(false);
        }

        public void HideProgressBar()
        {
            _progressBar.transform.parent = null;
            _progressBar.gameObject.SetActive(false);
        }

        public void ShowAnimatedArrow(Vector2 position, Vector2 direction)
        {
            _arrowGameObj.SetActive(true);
            _arrowGameObj.transform.position = position;
            _arrowGameObj.transform.up = direction;
        }

        public void ShowProgressBar(Vector2 pos, Quaternion rotation, Transform parent)
        {
            _progressBar.gameObject.SetActive(true);
            _progressBar.transform.position = pos;
            _progressBar.transform.rotation = rotation;
            _progressBar.transform.parent = parent;
        }

        public void SetProgress(float progress)
        {
            _progressBar.SetProgress(progress);
        }
    }
}
