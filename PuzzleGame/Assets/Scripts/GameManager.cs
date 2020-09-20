using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    public class GameManager : MonoBehaviour
    {
        public Room roomPrefab = null;

        private void Awake()
        {
            if (GameContext.s_gameMgr != null)
                Destroy(this);
            else
                GameContext.s_gameMgr = this;
        }

        private void Start()
        {
            Room r = GameObject.Find("Room").GetComponent<Room>();
            r.SpawnNext();
            r.next.SpawnNext();
        }
    }
}
