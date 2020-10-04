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

        [SerializeField] Transform _contentRoot = null;
        [SerializeField] Tilemap _roomTile = null;
        [SerializeField] Tilemap _paintingTile = null;

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

        public float paintingToVisibleAreaScale { get { return _paintingAreaLocal.width / _visibleAreaLocal.width; } }
        public float roomScale { get { return _contentRoot.lossyScale.x; } }
        public float cameraViewDist { get { return roomUnitToWorldUnit(_cameraViewDistLocal); } }
        public Vector2 playerSpawnPos { get { return _playerSpawnPos.position; } }
        public Vector2 viewCenterPos { get { return _viewCenterPos.position; } }

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
                SetSpriteSortingOrder(value);
                ConfigSpriteMask(value);
            }
        }
        int _roomIndex;

        public void SetState(ERoomState state)
        {
            if (state == _state)
                return;
        }

        private void ConfigSpriteMask(int roomIdx)
        {
            SpriteMask[] masks = _paintingMask.GetComponentsInChildren<SpriteMask>();
            int[] sortingLayers = new int[]
            {
                GameConst.k_DefaultSortingLayerId,
                GameConst.k_DecoSortingLayerId,
                GameConst.k_InteractableSortingLayerId
            };
            Assert.IsTrue(masks.Length == sortingLayers.Length);

            for(int i=0; i<sortingLayers.Length; i++)
            {
                SpriteMask mask = masks[i];
                mask.isCustomRangeActive = true;
                mask.frontSortingLayerID = sortingLayers[i];
                mask.backSortingLayerID = sortingLayers[i];
                mask.frontSortingOrder = roomIdx;
                mask.backSortingOrder = roomIdx - 1;
            }
        }
        
        private void SetSpriteSortingOrder(int roomIdx)
        {
            _roomTile.GetComponent<TilemapRenderer>().sortingOrder = roomIdx;
            _paintingTile.GetComponent<TilemapRenderer>().sortingOrder = roomIdx;

            foreach (var actor in _actors)
            {
                if (actor.spriteRenderer)
                    actor.spriteRenderer.sortingOrder = roomIdx;
            }
        }

        private void SetSpriteMaskInteraction(SpriteMaskInteraction interaction)
        {
            _roomTile.GetComponent<TilemapRenderer>().maskInteraction = interaction;
            _paintingTile.GetComponent<TilemapRenderer>().maskInteraction = interaction;

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
        public void BecomePainting(Room parent)
        {
            transform.parent = parent._paintingTransform;
            transform.localScale = new Vector3(paintingToVisibleAreaScale, paintingToVisibleAreaScale, 1f);
            transform.localPosition = Vector2.zero;
            
            SetSpriteMaskInteraction(SpriteMaskInteraction.VisibleInsideMask);
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
            _paintingMask.transform.parent = _contentRoot;

            SetSpriteMaskInteraction(SpriteMaskInteraction.None);

            Messenger.AddListener<RoomEventData>(M_EventType.ON_ENTER_ROOM, OnEnterRoom);
            Messenger.AddListener<RoomEventData>(M_EventType.ON_BEFORE_ENTER_ROOM, OnBeforeEnterRoom);

            //make the room pivot at the visible area pivot
            _contentRoot.localPosition = -_visibleAreaLocal.position;

            //make the painting mask same size as the visible area, and above the room in z axis
            //because our visible area is the same as the painting area of our parent
            _paintingMask.transform.localScale = new Vector3(Mathf.Abs(_visibleAreaLocal.width), Mathf.Abs(_visibleAreaLocal.height), 1);
            _paintingMask.transform.localPosition = _visibleAreaLocal.position;
            Vector3 pos = _paintingMask.transform.position;
            pos.z = -1;
            _paintingMask.transform.position = pos;
        }

        /// <summary>
        /// spawns a whole chain of rooms, returns the room with identity scale and sets it to be current room
        /// </summary>
        /// <param name="numLevels">number of levels</param>
        /// <returns>the root</returns>
        public static Room SpawnChain(int numLevels, int identityRoomLevel)
        {
            if (identityRoomLevel < 0 || identityRoomLevel >= numLevels)
                throw new ArgumentOutOfRangeException("identity room level must be between 0 and numLevels-1, inclusive");

            Room ret = null;

            Room cur = Instantiate(GameContext.s_gameMgr.roomPrefab, Vector3.zero, Quaternion.identity).GetComponent<Room>();
            cur.roomIndex = 0;
            float rootRoomScale = Mathf.Pow(1/cur.paintingToVisibleAreaScale, identityRoomLevel);
            cur.transform.localScale = new Vector3(rootRoomScale, rootRoomScale, 1);

            for(int level=0; level<numLevels; level++, cur = cur.next)
            {
                cur.SpawnNext();

                if (level == identityRoomLevel)
                {
                    ret = cur;
                }
            }
            ret.SetCurrent();

            return ret;
        }

        void SpawnNext()
        {
            if (next)
                throw new NotSupportedException("reconnection between rooms is not supported");

            GameObject clone = Instantiate(GameContext.s_gameMgr.roomPrefab.gameObject);
            next = clone.GetComponent<Room>();   
            next.BecomePainting(this);
            next.roomIndex = roomIndex + 1;
            next.prev = this;
        }

        public void RotateNext(float angle)
        {
            if (!next)
                return;

            //rotate painting frame
            //all children should be childed under the painting's transform
            _paintingTransform.RotateAround(_paintingRotationAnchor.position, Vector3.forward, angle);
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
            _paintingTile.GetComponent<TilemapRenderer>().enabled = enable;
            _roomTile.GetComponent<TilemapRenderer>().enabled = enable;
            foreach(var actor in _actors)
            {
                if(actor.spriteRenderer)
                {
                    actor.spriteRenderer.enabled = enable;
                }
            }
        }

        /// <summary>
        /// initializes a room as the current room
        /// note: for transition, use GoToNext or GoToPrev instead
        /// </summary>
        void SetCurrent()
        {
            Room ptr = prev;
            //hide parent rooms
            while(ptr)
            {
                ptr.SetRoomCollision(false);
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
            SetSpriteMaskInteraction(SpriteMaskInteraction.None);

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

            SetSpriteMaskInteraction(SpriteMaskInteraction.VisibleInsideMask);
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
                SetSpriteMaskInteraction(SpriteMaskInteraction.None);
                SetRoomCollision(true);
                SetActive(true);
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