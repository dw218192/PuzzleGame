using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;

namespace PuzzleGame
{
    public enum ERoomState
    {
        NORMAL,
        IN_PAINTING,
    }

    public class Room : MonoBehaviour
    {
        Actor[] _actors = null;
        [SerializeField] GameObject _paintingMask = null;
        SpriteMask _spriteMask = null;

        [SerializeField] Transform _contentRoot = null;
        [SerializeField] Tilemap _roomTile = null;

        //unscaled painting size
        [SerializeField] Rect _paintingArea;

        //relative to the room, top-left (0,0)
        //unscaled visible size
        [SerializeField] Rect _visibleArea;

        Vector2Int _roomSize = Vector2Int.zero;
        ERoomState _state = ERoomState.NORMAL;

        public Rect paintingArea { get { return _paintingArea; } }
        public Rect visibleArea { get { return _visibleArea; } }
        public Transform contentRoot { get { return _contentRoot; } }
        public Room next { get; private set; } = null;
        public Room prev { get; private set; } = null;
        public int roomIndex
        {
            get
            {
                return _roomIndex;
            }
            private set
            {
                _roomIndex = value;

                SortingGroup group = gameObject.GetComponent<SortingGroup>();
                if (!group)
                    group = gameObject.AddComponent<SortingGroup>();
                group.sortingOrder = value;
            }
        }
        int _roomIndex;

        public void SetState(ERoomState state)
        {
            if (state == _state)
                return;
        }

        public void SetToPainting(Transform parentContentTransform)
        {
            float scale = _paintingArea.width / _visibleArea.width;
            float parentLossyScale = parentContentTransform.lossyScale.x;
            _contentRoot.localScale = new Vector3(parentLossyScale * scale, parentLossyScale * scale, 1f);

            //scale and move the room to painting position
            //such that visible area fits the painting area
            Vector2 parentPos = parentContentTransform.localPosition;
            Vector2 offset = _paintingArea.position - _visibleArea.position * scale;
            offset *= parentLossyScale;

            _contentRoot.localPosition = parentPos + offset;

            //sprite mask size is the same as parent's painting size
            _paintingMask.transform.localScale = new Vector3(
                _paintingArea.width * parentLossyScale,
                _paintingArea.height * parentLossyScale, 1f);

            _paintingMask.transform.localPosition = parentPos + _paintingArea.position * parentLossyScale;
        }

        private void Awake()
        {
            _actors = GetComponentsInChildren<Actor>();

            foreach(var actor in _actors)
            {
                actor.room = this;
            }

            _roomSize = new Vector2Int(_roomTile.size.x, _roomTile.size.y);

            _paintingMask = Instantiate(_paintingMask);
            _spriteMask = _paintingMask.GetComponentInChildren<SpriteMask>();
            _spriteMask.isCustomRangeActive = false;
            _roomTile.GetComponent<TilemapRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        public void SpawnNext()
        {
            GameObject clone = Instantiate(GameContext.s_gameMgr.roomPrefab.gameObject);
            next = clone.GetComponent<Room>();
            next.SetToPainting(_contentRoot);
            next.roomIndex = roomIndex + 1;
            next.prev = this;
        }

        public void SpawnPrev()
        {
            throw new NotImplementedException();
        }
    }
}