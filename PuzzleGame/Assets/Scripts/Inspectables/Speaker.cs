using System;
using System.Text;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using PuzzleGame.UI;
using UltEvents;

namespace PuzzleGame
{
    public class Speaker : Inspectable
    {
        class ColliderComparer : IComparer<Collider2D>
        {
            int IComparer<Collider2D>.Compare(Collider2D x, Collider2D y)
            {
                return y.transform.localPosition.y.CompareTo(x.transform.localPosition.y);
            }
        }

        /*
        class State
        {
            private State() { }

            public State(Speaker speaker)
            {
                initNodePositions = new Vector2[speaker._nodes.Length];
                nodePositions = new Vector2[speaker._nodes.Length];
                bladeRotations = new float[speaker._blades.Length];
                bladeActiveStates = new bool[speaker._blades.Length];

                for (int i = 0; i < speaker._nodes.Length; i++)
                {
                    initNodePositions[i] = speaker._nodes[i].transform.localPosition;
                    nodePositions[i] = speaker._nodes[i].transform.localPosition;
                }
                for (int i = 0; i < speaker._blades.Length; i++)
                {
                    bladeRotations[i] = 0;
                    bladeActiveStates[i] = false;
                }
            }

            public void Register(Speaker speaker)
            {
                speakers.Add(speaker);
            }

            public void SyncState(Speaker speaker)
            {
                for(int i=0; i<speaker._nodes.Length; i++)
                {
                    nodePositions[i] = speaker._nodes[i].transform.localPosition;
                }

                for(int i=0; i<speaker._blades.Length; i++)
                {
                    bladeRotations[i] = speaker._blades[i].zRotation;
                    bladeActiveStates[i] = speaker._blades[i].isActive;
                }

                foreach(var s in speakers)
                {
                    if(!ReferenceEquals(s, speaker))
                    {
                        s.UpdateState();
                    }
                }
            }

            public Vector2[] initNodePositions;
            public Vector2[] nodePositions;
            public float[] bladeRotations;
            public bool[] bladeActiveStates;
            public List<Speaker> speakers = new List<Speaker>();
        }
        static State s_State = null;
        */

        static Speaker[] s_allSpeakers = null;
        static Vector2[] s_initNodePositions = null;

        [Header("Speaker Puzzle Config")]
        [SerializeField] UltEvent _successEvent;
        [SerializeField] float _successSeqStepTime;
        
        [Header("Inspection Canvas Config")]
        [SerializeField] SpeakerPuzzleNode[] _nodes;
        [SerializeField] SpeakerBlade[] _blades;
        [SerializeField] EdgeCollider2D _goalCheckCollider;

        [Header("Screen Canvas Config")]
        [SerializeField] Button _resetButton;

        Collider2D[] _middleNodes;
        ContactFilter2D _filter;
       
        protected override void Awake()
        {
            base.Awake();

            _middleNodes = new Collider2D[10];

            _filter = new ContactFilter2D();
            _filter.useTriggers = true;
            _filter.layerMask = 1 << GameConst.k_UILayer;

            _resetButton.onClick.AddListener(ResetPuzzle);
        }

        protected override void Start()
        {
            base.Start();

            if (s_allSpeakers == null)
            {
                var actors = GameContext.s_gameMgr.GetAllActorsByID(actorId);
                List<Speaker> filtered = new List<Speaker>();
                foreach(var actor in actors)
                {
                    if (actor)
                        filtered.Add((Speaker)actor);
                }
                s_allSpeakers = filtered.ToArray();

                s_initNodePositions = new Vector2[_nodes.Length];

                for (int i = 0; i < _nodes.Length; i++)
                {
                    s_initNodePositions[i] = _nodes[i].transform.localPosition;
                }
            }
        }

        public void SyncState()
        {
            foreach(var speaker in s_allSpeakers)
            {
                if(speaker && !ReferenceEquals(speaker, this))
                {
                    for (int i = 0; i < _nodes.Length; i++)
                    {
                        speaker._nodes[i].transform.localPosition = _nodes[i].transform.localPosition;
                    }

                    for (int i = 0; i < _blades.Length; i++)
                    {
                        speaker._blades[i].zRotation = _blades[i].zRotation;
                        speaker._blades[i].SetBladeActive(_blades[i].isActive);
                    }
                }
            }
        }

        /*
        public void UpdateState()
        {
            for (int i = 0; i < _nodes.Length; i++)
            {
                _nodes[i].transform.localPosition = s_State.nodePositions[i];
            }

            for (int i = 0; i < _blades.Length; i++)
            {
                _blades[i].zRotation = s_State.bladeRotations[i];
                _blades[i].SetBladeActive(s_State.bladeActiveStates[i]);
            }
        }
        */

        public void ResetPuzzle()
        {
            ReturnAllBlades();

            for (int i = 0; i < _nodes.Length; i++)
            {
                _nodes[i].transform.localPosition = s_initNodePositions[i];
            }

            for (int i = 0; i < _blades.Length; i++)
            {
                _blades[i].zRotation = 0;
            }

            SyncState();
        }

        public void ReturnAllBlades()
        {
            for (int i = 0; i < _blades.Length; i++)
            {
                if(_blades[i].isActive)
                {
                    _blades[i].placePoint.GiveItem();
                }

                _blades[i].SetBladeActive(false);
            }
        }

        public void CheckGoalState()
        {
            int overlapCount = _goalCheckCollider.OverlapCollider(_filter, _middleNodes);
            Array.Sort(_middleNodes, 0, overlapCount, new ColliderComparer());

            StringBuilder builder = new StringBuilder();
            List<SpeakerPuzzleNode> nodes = new List<SpeakerPuzzleNode>();
            for (int i=0; i<overlapCount; i++)
            {
                var node = _middleNodes[i].GetComponent<SpeakerPuzzleNode>();
                if (node)
                {
                    builder.Append(node.letter);
                    nodes.Add(node);
                }
            }

            string str = builder.ToString().ToLower();
            if (str.Equals("pizza"))
            {
                StartCoroutine(_successRoutine(nodes, _successSeqStepTime));
            }
        }

        IEnumerator _successRoutine(List<SpeakerPuzzleNode> middleNodes, float stepTime)
        {
            ReturnAllBlades();
            //no need to sync state here as the puzzle becomes uninteractable

            Color color = Color.green;
            foreach(var node in middleNodes)
            {
                node.SetColor(color);
                yield return new WaitForSeconds(stepTime);
            }

            foreach (var node in middleNodes)
            {
                node.SetColor(color);
            }

            _successEvent?.Invoke();
            canInspect = false;
        }
    }
}