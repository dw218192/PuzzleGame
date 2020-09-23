using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;

using PuzzleGame.EventSystem;

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
        Collider2D[] _colliders = null;
        Rigidbody2D[] _rigidBodies = null;

        [SerializeField] GameObject _paintingMask = null;
        SpriteMask _spriteMask = null;

        [SerializeField] Transform _contentRoot = null;
        [SerializeField] Tilemap _roomTile = null;

        //relative to the room, top-left (0,0)
        //unscaled painting size
        [SerializeField] Rect _paintingArea;

        //unscaled visible size
        [SerializeField] Rect _visibleArea;
        Vector2 _roomSize;
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
        public Rect roomArea { get { return new Rect(_contentRoot.position, roomDirToWorldDir(_roomSize)); } }
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

        private void SetSpriteMasking(SpriteMaskInteraction interaction)
        {
            _roomTile.GetComponent<TilemapRenderer>().maskInteraction = interaction;
            foreach (var actor in _actors)
                actor.spriteRenderer.maskInteraction = interaction;
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
            SetSpriteMasking(SpriteMaskInteraction.VisibleInsideMask);

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
            SetSpriteMasking(SpriteMaskInteraction.VisibleInsideMask);

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
            _colliders = GetComponentsInChildren<Collider2D>();
            _rigidBodies = GetComponentsInChildren<Rigidbody2D>();

            foreach (var actor in _actors)
            {
                actor.room = this;
            }

            //Note: painting mask is for the display of current room in the previous room
            //so it's only activated when we spawn a child or a parent
            _paintingMask = Instantiate(_paintingMask, transform);
            _spriteMask = _paintingMask.GetComponentInChildren<SpriteMask>();
            _spriteMask.isCustomRangeActive = false;
            _paintingMask.SetActive(false);

            SetSpriteMasking(SpriteMaskInteraction.None);
            _roomSize = new Vector2(_roomTile.size.x, _roomTile.size.y - 1);
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

        private void SetRoomCollision(bool enable)
        {
            foreach (var collider in _colliders)
                collider.enabled = enable;

            foreach (var rigidbody in _rigidBodies)
            {
                if (enable)
                    rigidbody.WakeUp();
                else
                    rigidbody.Sleep();
            }
        }

        private void Hide()
        {
            _contentRoot.gameObject.SetActive(false);
            _paintingMask.SetActive(false);
        }

        /// <summary>
        /// initializes a room as the current room
        /// note: for transition, use GoToNext or GoToPrev instead
        /// </summary>
        public void SetCurrent()
        {
            Room ptr = prev;
            //hide parent rooms
            while(ptr)
            {
                ptr.Hide();
                ptr = ptr.prev;
            }
            //disable children room collisions
            ptr = next;
            while(ptr)
            {
                ptr.SetRoomCollision(false);
                ptr = ptr.next;
            }

            //disable masking of the current room
            SetSpriteMasking(SpriteMaskInteraction.None);

            //callbacks
            Messenger.Broadcast(M_EventType.ON_BEFORE_ENTER_ROOM, new RoomEventData(this));
        }

        public void GoToNext()
        {
            if (!next)
                return;

            //disable masking
            next.SetSpriteMasking(SpriteMaskInteraction.None);
            next.SetRoomCollision(true);

            Hide();

            Messenger.Broadcast(M_EventType.ON_BEFORE_ENTER_ROOM, new RoomEventData(next));
        }

        public void GoToPrev()
        {
            throw new NotImplementedException();
        }

        private void OnDrawGizmos()
        {
            //Gizmos.DrawSphere(paintingArea.position + 0.5f * paintingArea.size, 0.5f);
        }
    }
}