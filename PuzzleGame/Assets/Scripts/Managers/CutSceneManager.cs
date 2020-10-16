using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

using PuzzleGame.EventSystem;
using PuzzleGame.UI;
using System.Linq;

namespace PuzzleGame
{
    public class CutSceneManager : MonoBehaviour
    {
        [SerializeField] PlayableDirector[] _directors;
        [SerializeField] Camera _cutSceneCam;

        private void Awake()
        {
            foreach (var director in _directors)
            {
                director.extrapolationMode = DirectorWrapMode.None;
                director.playOnAwake = false;
                director.stopped += Director_stopped;

                Debug.Assert(director.playableAsset is TimelineAsset);
            }
            _cutSceneCam.gameObject.SetActive(false);
        }

        private void Director_stopped(PlayableDirector director)
        {
            Messenger.Broadcast(M_EventType.ON_CUTSCENE_END, new CutSceneEventData(director.playableAsset as TimelineAsset));
            GameContext.s_gameMgr.OnEndCutScene(director.playableAsset as TimelineAsset);
        }

        public void Play(TimelineAsset timeline)
        {
            PlayableDirector targetDirector = null;
            foreach (var director in _directors)
            {
                if(ReferenceEquals(director.playableAsset, timeline))
                {
                    targetDirector = director;
                    break;
                }
            }

            Debug.Assert(targetDirector);
            targetDirector.Play();
            
            Messenger.Broadcast(M_EventType.ON_CUTSCENE_START, new CutSceneEventData(timeline));
        }

        public void DisplayDialogue(DialogueDef dialogueDef)
        {
            DialogueMenu.Instance.DisplayDialogue(dialogueDef);
        }
    }
}
