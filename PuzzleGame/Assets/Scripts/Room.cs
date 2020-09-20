using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace PuzzleGame
{
    public enum ERoomState
    {
        NORMAL,
        IN_PAINTING,
    }

    public class Room : MonoBehaviour
    {
        static Mesh s_paintingQuad;

        Actor[] _actors = null;
        [SerializeField] Transform _painting = null;
        [SerializeField] Tilemap _roomTile = null;

        //unscaled painting size
        [SerializeField] Rect _paintingArea;

        //relative to the room, top-left (0,0)
        //unscaled visible size
        [SerializeField] Rect _visibleArea;

        Vector2Int _roomSize = Vector2Int.zero;
        ERoomState _state = ERoomState.NORMAL;

        Camera _roomCam = null;
        RenderTexture _paintingRenderTex = null;

        public Room next { get; private set; } = null;
        public Room prev { get; private set; } = null;
        public int roomIndex = 0;

        public void SetState(ERoomState state)
        {
            if (state == _state)
                return;
        }

        private void Awake()
        {
            if(!s_paintingQuad)
            {
                s_paintingQuad = new Mesh();
                s_paintingQuad.vertices = new Vector3[4]
                {
                    new Vector3(0,0,0),
                    new Vector3(_paintingArea.width,0, 0),
                    new Vector3(0, -_paintingArea.height, 0),
                    new Vector3(_paintingArea.width, -_paintingArea.height,0)
                };
                s_paintingQuad.triangles = new int[6]
                {
                    0,1,2,
                    2,1,3
                };
                s_paintingQuad.uv = new Vector2[4]
                {
                    new Vector2(0,1),
                    new Vector2(1,1),
                    new Vector2(0,0),
                    new Vector2(1,0)
                };
            }

            _roomCam = GetComponentInChildren<Camera>();
            _roomCam.transform.localPosition = new Vector3(
                (_visibleArea.x + _visibleArea.width / 2),
                -(_visibleArea.y + _visibleArea.height / 2),
                -1f);

            _actors = GetComponentsInChildren<Actor>();

            foreach(var actor in _actors)
            {
                actor.room = this;
            }

            _roomSize = new Vector2Int(_roomTile.size.x, _roomTile.size.y);

            //if we're the root, init
            if(!prev && !next)
            {
                Init(1f);
            }

            //create the painting
            GameObject paintingHolder = new GameObject("paintingHolder");
            paintingHolder.transform.parent = _painting;
            paintingHolder.transform.localPosition = Vector3.zero;
            MeshFilter filter = paintingHolder.AddComponent<MeshFilter>();
            MeshRenderer rend = paintingHolder.AddComponent<MeshRenderer>();
            filter.sharedMesh = s_paintingQuad;
            rend.material = new Material(Shader.Find("Unlit/Texture"));
            rend.material.mainTexture = _paintingRenderTex;
        }

        public void SpawnNext()
        {
            GameObject clone = Instantiate(GameContext.s_gameMgr.roomPrefab.gameObject);
            clone.transform.parent = _painting;

            float scale = _paintingArea.width / _visibleArea.width;
            clone.transform.localScale = new Vector3(scale, scale, 1f);
            clone.transform.localPosition = new Vector3(-_visibleArea.x * scale, -_visibleArea.y * scale, 0f);

            next = clone.GetComponent<Room>();
            next.Init(clone.transform.lossyScale.x);

            next._roomCam.targetTexture = this._paintingRenderTex;

            clone.transform.localPosition = Vector3.one * 2000;
        }

        public void SpawnPrev()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// a room's scale must be uniform
        /// </summary>
        /// <param name="lossyScale">global scale</param>
        void Init(float lossyScale)
        {
            _roomCam.orthographicSize = _visibleArea.height * lossyScale / 2f;
            _roomCam.aspect = _visibleArea.width / _visibleArea.height;

            //set render texture for the painting
            Vector2 scaledPaintingSize = new Vector2(_paintingArea.width * lossyScale, _paintingArea.height * lossyScale);
            _paintingRenderTex = new RenderTexture(
                Mathf.RoundToInt(scaledPaintingSize.x) * GameConst.k_pixelPerWorldUnit, 
                Mathf.RoundToInt(scaledPaintingSize.y) * GameConst.k_pixelPerWorldUnit, 0);
            _paintingRenderTex.Create();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}