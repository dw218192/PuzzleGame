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

        ERoomState _state = ERoomState.NORMAL;

        //conversion methods
        private Vector2 roomPointToWorldPoint(Vector2 roomSpacePos)
        {
            float scale = _contentRoot.lossyScale.x;
            return new Vector2(_contentRoot.localPosition.x + roomSpacePos.x * scale, _contentRoot.localPosition.y - roomSpacePos.y * scale);
        }
        public Vector2 roomDirToWorldDir(Vector2 dir)
        {
            float scale = _contentRoot.lossyScale.x;
            return new Vector2(dir.x * scale, -dir.y * scale);
        }
        public float roomUnitToWorldUnit(float length)
        {
            return length * _contentRoot.lossyScale.x;
        }

        public float cameraViewDist { get { return roomUnitToWorldUnit(_cameraViewDist); } }
        public Vector2 playerSpawnPos { get { return roomPointToWorldPoint(_playerSpawnPos); } }
        public Vector2 viewCenterPos { get { return roomPointToWorldPoint(_viewCenterPos); } }
        public Rect paintingArea { get { return new Rect(roomPointToWorldPoint(_paintingArea.position), roomDirToWorldDir(_paintingArea.size)); } }
        public Rect visibleArea { get { return new Rect(roomPointToWorldPoint(_visibleArea.position), roomDirToWorldDir(_visibleArea.size)); } }
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
            _contentRoot.localPosition = parent.roomPointToWorldPoint(_paintingArea.position) - this.roomDirToWorldDir(_visibleArea.position);

            //enable sprite masking for this room
            _roomTile.GetComponent<TilemapRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            foreach (var actor in _actors)
                actor.spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

            //put a sprite mask at parent's painting position and scale it to the painting's size
            _paintingMask.SetActive(true);
            Vector2 parentPaintingSize = _paintingArea.size * parentLossyScale;
            _paintingMask.transform.localScale = new Vector3(parentPaintingSize.x, parentPaintingSize.y, 1f);
            _paintingMask.transform.localPosition = parent.paintingArea.position;
        }

        public void SetFromPainting(Room child)
        {
            float scale = _visibleArea.width / _paintingArea.width;
            float childLossyScale = child._contentRoot.lossyScale.x;
            _contentRoot.localScale = new Vector3(childLossyScale * scale, childLossyScale * scale, 1f);

            _contentRoot.localPosition = child.roomPointToWorldPoint(_visibleArea.position) - this.roomDirToWorldDir(_paintingArea.position);

            //enable sprite masking for child
            child._roomTile.GetComponent<TilemapRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            foreach (var actor in child._actors)
                actor.spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

            child._paintingMask.SetActive(true);
            float lossyScale = _contentRoot.lossyScale.x;
            child._paintingMask.transform.localScale = new Vector3(
                _paintingArea.width * lossyScale,
                _paintingArea.height * lossyScale, 1f);
            child._paintingMask.transform.localPosition = paintingArea.position;
        }

        private void Awake()
        {
            _actors = GetComponentsInChildren<Actor>();

            foreach(var actor in _actors)
            {
                actor.room = this;
                actor.spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
            }


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

        public void RotateNext()
        {

        }

        private void OnDrawGizmos()
        {
            //Gizmos.DrawSphere(paintingArea.position + 0.5f * paintingArea.size, 0.5f);
        }
    }
}