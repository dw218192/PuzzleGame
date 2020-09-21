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

        //room camera settings
        [SerializeField] int _cameraViewDist;
        [SerializeField] Vector2 _viewCenterPos;
        [SerializeField] Vector2 _playerSpawnPos;

        Vector2Int _roomSize = Vector2Int.zero;
        ERoomState _state = ERoomState.NORMAL;

        public int cameraViewDist { get { return _cameraViewDist; } }
        public Vector2 playerSpawnPos { get { return _playerSpawnPos; } }
        public Vector2 viewCenterPos { get { return _viewCenterPos; } }
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
            set
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

        public void SetToPainting(Room parent)
        {
            float scale = _paintingArea.width / _visibleArea.width;
            float parentLossyScale = parent._contentRoot.lossyScale.x;
            _contentRoot.localScale = new Vector3(parentLossyScale * scale, parentLossyScale * scale, 1f);

            //scale and move the room to painting position
            //such that visible area fits the painting area
            Vector2 parentPos = parent._contentRoot.localPosition;
            Vector2 offset = _paintingArea.position - _visibleArea.position * scale;
            offset *= parentLossyScale;

            _contentRoot.localPosition = parentPos + offset;

            //enable sprite masking for myself
            _roomTile.GetComponent<TilemapRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

            //sprite mask size is the same as parent's painting size
            _paintingMask.SetActive(true);
            _paintingMask.transform.localScale = new Vector3(
                _paintingArea.width * parentLossyScale,
                _paintingArea.height * parentLossyScale, 1f);

            _paintingMask.transform.localPosition = parentPos + _paintingArea.position * parentLossyScale;
        }

        public void SetFromPainting(Room child)
        {
            float scale = _visibleArea.width / _paintingArea.width;
            float childLossyScale = child._contentRoot.lossyScale.x;
            _contentRoot.localScale = new Vector3(childLossyScale * scale, childLossyScale * scale, 1f);

            float lossyScale = _contentRoot.lossyScale.x;
            Vector2 childPos = child._contentRoot.localPosition;
            childPos += child.visibleArea.position * childLossyScale;
            _contentRoot.localPosition = childPos - _paintingArea.position * lossyScale;

            //enable sprite masking for child
            child._roomTile.GetComponent<TilemapRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

            //sprite mask size is the same as parent's painting size
            child._paintingMask.SetActive(true);
            child._paintingMask.transform.localScale = new Vector3(
                _paintingArea.width * lossyScale,
                _paintingArea.height * lossyScale, 1f);

            Vector2 parentPos = _contentRoot.localPosition;
            child._paintingMask.transform.localPosition = parentPos + _paintingArea.position * lossyScale;
        }

        private void Awake()
        {
            _actors = GetComponentsInChildren<Actor>();

            foreach(var actor in _actors)
            {
                actor.room = this;
            }

            _roomSize = new Vector2Int(_roomTile.size.x, _roomTile.size.y);

            //Note: painting mask is for the display of current room in the previous room
            //so it's only activated when we spawn a child or a parent
            _paintingMask = Instantiate(_paintingMask, transform);
            _spriteMask = _paintingMask.GetComponentInChildren<SpriteMask>();
            _spriteMask.isCustomRangeActive = false;
            _paintingMask.SetActive(false);

            _roomTile.GetComponent<TilemapRenderer>().maskInteraction = SpriteMaskInteraction.None;
        }

        public void SpawnNext()
        {
            if (next)
                throw new NotSupportedException("reconnection between rooms is not supported");

            GameObject clone = Instantiate(GameContext.s_gameMgr.roomPrefab.gameObject);
            next = clone.GetComponent<Room>();
            next.SetToPainting(this);
            next.roomIndex = roomIndex + 1;
            next.prev = this;
        }

        public void SpawnPrev()
        {
            if (prev)
                throw new NotSupportedException("reconnection between rooms is not supported");

            GameObject clone = Instantiate(GameContext.s_gameMgr.roomPrefab.gameObject);
            prev = clone.GetComponent<Room>();
            prev.SetFromPainting(this);
            prev.roomIndex = roomIndex - 1;
            prev.next = this;
        }
    }
}