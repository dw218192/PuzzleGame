using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

using PuzzleGame.EventSystem;
using PuzzleGame.UI;

namespace PuzzleGame
{
    public class CutSceneManager : MonoBehaviour
    {
        [SerializeField] PlayableDirector[] _directors;
        [SerializeField] Camera _cutSceneCam;

        Dictionary<PlayableDirector, int> _director2Id = new Dictionary<PlayableDirector, int>();

        private void Awake()
        {
            for (int i=0; i<_directors.Length; i++)
            {
                PlayableDirector director = _directors[i];
                director.extrapolationMode = DirectorWrapMode.None;
                director.playOnAwake = false;
                director.stopped += Director_stopped;
                _director2Id[director] = i;
            }
            _cutSceneCam.gameObject.SetActive(false);
        }

        private void Director_stopped(PlayableDirector obj)
        {
            Messenger.Broadcast(M_EventType.ON_CUTSCENE_END, new CutSceneEventData(_director2Id[obj]));
        }

        public void Play(int cutSceneId)
        {
            Debug.Assert(cutSceneId >= 0 && cutSceneId < _directors.Length);
            _directors[cutSceneId].Play();

            Messenger.Broadcast(M_EventType.ON_CUTSCENE_START, new CutSceneEventData(cutSceneId));
        }

        public void DisplayDialogue(string[] dialogue)
        {
            DialogueMenu.Instance.DisplayDialogue(dialogue);
        }
    }
}
