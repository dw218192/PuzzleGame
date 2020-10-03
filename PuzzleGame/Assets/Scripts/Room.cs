using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;
using UnityEngine.Assertions;

using PuzzleGame.EventSystem;

using Object = UnityEngine.Object;
namespace PuzzleGame
{
    public enum ERoomState
    {
        NORMAL,
        IN_PAINTING,
    }

    public class Room : MonoBehaviour
    {
        LinkedList<Actor> _actors = null;

        [SerializeField] GameObject _paintingMask = null;
        [SerializeField] Transform _paintingTransform = null;
        [SerializeField] Transform _paintingRotationAnchor = null;
        [SerializeField] Transform _roomMin, _roomMax; //min and max of room OBB
        SpriteMask _spriteMask = null;

        [SerializeField] Transform _contentRoot = null;
        [SerializeField] Tilemap _roomTile = null;

        //relative to the room, top-left (0,0)
        //unscaled painting size
        [SerializeField] Rect _paintingAreaLocal;
        //unscaled visible size
        [SerializeField] Rect _visibleAreaLocal;
        //room camera settings
        [SerializeField] int _cameraViewDistLocal;
        [SerializeField] Transform _viewCenterPos;
        [SerializeField] Transform _playerSpawnPos;

        ERoomState _state = ERoomState.NORMAL;

        //conversion methods
        /// <summary>
        /// given a point in this room, find out the world position of this point in parent room
        /// without needing to actually create the parent
        /// </summary>
        /// <param name="roomSpacePos"></param>
        /// <returns></returns>
        public Vector2 getCorrespondingParentRoomPoint(Vector2 roomSpacePos)
        {
            //inverse scale
            float scale = _visibleAreaLocal.width / _paintingAreaLocal.width;
            float parentScale = roomScale * scale;
            Vector2 parentContentRootPos = roomPointToWorldPoint(_visibleAreaLocal.position) - _paintingAreaLocal.position * parentScale;
            return parentContentRootPos + roomSpacePos * parentScale;
        }
        public Vector2 roomPointToWorldPoint(Vector2 roomSpacePos)
        {
            return _contentRoot.TransformPoint(roomSpacePos);
        }
        public Vector2 roomDirToWorldDir(Vector2 dir)
        {
            return _contentRoot.TransformVector(dir);
        }
        public float roomUnitToWorldUnit(float length)
        {
            return length * _contentRoot.lossyScale.x;
        }

        public float roomScale { get { return _contentRoot.lossyScale.x; } }
        public float cameraViewDist { get { return roomUnitToWorldUnit(_cameraViewDistLocal); } }
        public Vector2 playerSpawnPos { get { return _playerSpawnPos.position; } }
        public Vector2 viewCenterPos { get { return _viewCenterPos.position; } }
        Vector2 _roomRotCenterLocal;
        public Vector2 roomRotCenter { get { return roomPointToWorldPoint(_roomRotCenterLocal); } }

        //Note: the rect's size is signed
        public Rect paintingArea { get { return new Rect(roomPointToWorldPoint(_paintingAreaLocal.position), roomDirToWorldDir(_paintingAreaLocal.size)); } }
        public Rect visibleArea { get { return new Rect(roomPointToWorldPoint(_visibleAreaLocal.position), roomDirToWorldDir(_visibleAreaLocal.size)); } }
        public Bounds roomAABB
        {
            get
            {
                Vector2 diagonal1 = _roomMax.position - _roomMin.position;
                Vector2 topLeft = roomPointToWorldPoint(new Vector2(_roomMin.localPosition.x, _roomMax.localPosition.y));
                Vector2 bottomRight = roomPointToWorldPoint(new Vector2(_roomMax.localPosition.x, _roomMin.localPosition.y));
                Vector2 diagonal2 = bottomRight - topLeft;

                float height = Mathf.Abs(Vector2.Dot(Vector2.down, diagonal1));
                height = Mathf.Max(height, Mathf.Abs(Vector2.Dot(Vector2.down, diagonal2)));
                float width = Mathf.Abs(Vector2.Dot(Vector2.right, diagonal1));
                width = Mathf.Max(width, Mathf.Abs(Vector2.Dot(Vector2.right, diagonal2)));

                return new Bounds((Vector2)_roomMin.position + diagonal1 / 2, new Vector2(width, height));
            }
        }

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
            {
                if(actor.spriteRenderer)
                    actor.spriteRenderer.maskInteraction = interaction;
            }
        }
        
        /// <summary>
        /// make the room contained in the parent's painting
        /// </summary>
        /// <param name="parent"></param>
        public void SetToPainting(Room parent)
        {
            float scale = _paintingAreaLocal.width / _visibleAreaLocal.width;
            float parentLossyScale = parent._contentRoot.lossyScale.x;
            _contentRoot.localScale = new Vector3(parentLossyScale * scale, parentLossyScale * scale, 1f);

            //scale and move the room to painting position
            //such that visible area fits the painting area
            _contentRoot.localPosition = parent.roomPointToWorldPoint(_paintingAreaLocal.position) - this.roomDirToWorldDir(_visibleAreaLocal.position);

            //enable sprite masking for this room
            SetSpriteMasking(SpriteMaskInteraction.VisibleInsideMask);

            //put a sprite mask at parent's painting position and scale it to the painting's size
            _paintingMask.SetActive(true);
            Vector2 paintingSize = new Vector2(Mathf.Abs(_paintingAreaLocal.width), Mathf.Abs(_paintingAreaLocal.height));

            Vector2 parentPaintingSize = paintingSize * parentLossyScale;
            _paintingMask.transform.localScale = new Vector3(parentPaintingSize.x, parentPaintingSize.y, 1f);
            _paintingMask.transform.localPosition = parent.paintingArea.position;
        }

        /// <summary>
        /// make the room contain the child in its painting
        /// </summary>
        /// <param name="child"></param>
        public void SetFromPainting(Room child)
        {
            float scale = _visibleAreaLocal.width / _paintingAreaLocal.width;
            float childLossyScale = child.roomScale;
            _contentRoot.localScale = new Vector3(childLossyScale * scale, childLossyScale * scale, 1f);

            //_contentRoot.localPosition = child.roomPointToWorldPoint(_visibleArea.position) - this.roomDirToWorldDir(_paintingArea.position);
            _contentRoot.localPosition = child.getCorrespondingParentRoomPoint(Vector2.zero);

            //enable sprite masking for child
            child.SetSpriteMasking(SpriteMaskInteraction.VisibleInsideMask);

            child._paintingMask.SetActive(true);
            float lossyScale = _contentRoot.lossyScale.x;
            Vector2 paintingSize = new Vector2(Mathf.Abs(_paintingAreaLocal.width), Mathf.Abs(_paintingAreaLocal.height));

            child._paintingMask.transform.localScale = new Vector3(
                paintingSize.x * lossyScale,
                paintingSize.y * lossyScale, 1f);
            child._paintingMask.transform.localPosition = paintingArea.position;
        }

        private void Awake()
        {
            _actors = new LinkedList<Actor>(GetComponentsInChildren<Actor>());

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

            Messenger.AddListener<RoomEventData>(M_EventType.ON_ENTER_ROOM, OnEnterRoom);
            Messenger.AddListener<RoomEventData>(M_EventType.ON_BEFORE_ENTER_ROOM, OnBeforeEnterRoom);

            float invPaintingScale = _visibleAreaLocal.width / _paintingAreaLocal.width;
            _roomRotCenterLocal = invPaintingScale * (_paintingRotationAnchor.localPosition - _paintingTransform.localPosition); //relative to visible area
            _roomRotCenterLocal += _visibleAreaLocal.position; //relative to content root
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

        public void RotateNext(float angle)
        {
            if (!next)
                return;

            //rotate painting frame
            _paintingTransform.RotateAround(_paintingRotationAnchor.position, Vector3.forward, angle);

            //rotate child room
            Room cur = next;
            while(cur)
            {
                Room curPrev = cur.prev;
                cur.RotateSelf(angle);

                Vector3 offset = curPrev.roomPointToWorldPoint(curPrev._paintingAreaLocal.position) - cur.roomPointToWorldPoint(cur._visibleAreaLocal.position);
                cur.MoveSelf(offset);

                cur = cur.next;
            }
        }

        private void MoveSelf(Vector2 offset)
        {
            _contentRoot.Translate(offset, Space.World);
            _paintingMask.transform.Translate(offset, Space.World);
        }

        private void RotateSelf(float angle)
        {
            if (!prev)
                throw new NotSupportedException("cannot rotate a room without parent");
            //rotate room content
            _contentRoot.RotateAround(roomRotCenter, Vector3.forward, angle);
            //rotate sprite mask
            _paintingMask.transform.RotateAround(roomRotCenter, Vector3.forward, angle);
        }

        private void SetRoomCollision(bool enable)
        {
            foreach (var actor in _actors)
            {
                if(actor.actorCollider)
                {
                    actor.actorCollider.enabled = enable;
                }

                if (actor.actorRigidBody)
                {
                    if (enable)
                        actor.actorRigidBody.WakeUp();
                    else
                        actor.actorRigidBody.Sleep();
                }
            }
        }

        private void SetActive(bool enable)
        {
            _contentRoot.gameObject.SetActive(enable);
            _paintingMask.SetActive(enable);
        }

        /// <summary>
        /// initializes a room as the "root" room
        /// note: for transition, use GoToNext or GoToPrev instead
        /// </summary>
        public void SetCurrent()
        {
            Room ptr = prev;
            //hide parent rooms
            while(ptr)
            {
                ptr.SetActive(false);
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

            //reinit item ids
            foreach(var actor in _actors)
            {
                actor.RoomInit();
            }

            //callbacks
            Messenger.Broadcast(M_EventType.ON_BEFORE_ENTER_ROOM, new RoomEventData(this));
        }

        public void GoToNext()
        {
            if (!next)
                return;

            // we don't hide the current room here, i.e. this.SetActive(false) 
            // after we entered the next room
            // this is done in OnEnterRoom event handler

            Messenger.Broadcast(M_EventType.ON_BEFORE_ENTER_ROOM, new RoomEventData(next));
        }

        public void GoToPrev()
        {
            if (!prev)
                return;

            SetSpriteMasking(SpriteMaskInteraction.VisibleInsideMask);
            prev.SetActive(true);

            Messenger.Broadcast(M_EventType.ON_BEFORE_ENTER_ROOM, new RoomEventData(prev));
        }

        public void RemoveItemThisRoomOnly(EItemID itemID)
        {
            Assert.IsTrue(itemID != EItemID.INVALID, "Invalid Item ID");

            Interactable interactable = null;

            for(var it = _actors.First; it != null; it = it.Next)
            {
                if(it.Value is Interactable)
                {
                    Interactable inter = (Interactable)it.Value;
                    if(inter.itemID == itemID)
                    {
                        interactable = inter;
                        _actors.Remove(it);
                        break;
                    }
                }
            }

            Assert.IsTrue(interactable, "Invalid Item ID");

            Destroy(interactable.gameObject);
        }

        public void RemoveItemDownwards(EItemID itemID)
        {
            RemoveItemThisRoomOnly(itemID);
            if(next)
            {
                next.RemoveItemDownwards(itemID);
            }
        }

        public void RemoveItemAll(EItemID itemID)
        {
            RemoveItemDownwards(itemID);

            Room cur = prev;
            while(cur)
            {
                cur.RemoveItemThisRoomOnly(itemID);
                cur = cur.prev;
            }
        }

        private void OnDrawGizmos()
        {
            if(GameContext.s_gameMgr && Object.ReferenceEquals(this, GameContext.s_gameMgr.curRoom))
            {
                Gizmos.DrawSphere(roomRotCenter, 0.4f);
            }
        }

        #region GAME EVENTS
        private void OnBeforeEnterRoom(RoomEventData data)
        {

        }

        private void OnEnterRoom(RoomEventData data)
        {
            //is this the parent of the room that the player entered?
            if (Object.ReferenceEquals(data.room, next))
            {
                SetRoomCollision(false);
                SetActive(false);
            }
            //is this the room that the player entered?
            else if(Object.ReferenceEquals(data.room, this))
            {
                //disable masking, enable collision
                SetSpriteMasking(SpriteMaskInteraction.None);
                SetRoomCollision(true);
            }
            //is this the child of the room that the player entered?
            else if(Object.ReferenceEquals(data.room, prev))
            {
                SetRoomCollision(false);
            }
        }
        #endregion
    }
}