using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

namespace PuzzleGame.Editor
{
    public static class RoomDesignTool
    {
        public static Room editingRoom { get; private set; } = null;

        static bool _showRoomLayout;

        [InitializeOnLoadMethod]
        static void Init()
        {
            SceneView.duringSceneGui += OnSceneView;
            PrefabStage.prefabStageOpened += OnPrefabStageOpen;
            PrefabStage.prefabStageClosing += OnPrefabStageClosing;
        }

        static void OnPrefabStageOpen(PrefabStage stage)
        {
            if(stage.assetPath == "Assets/Prefabs/Room.prefab")
            {
                editingRoom = stage.prefabContentsRoot.GetComponent<Room>();
            }
        }

        static void OnPrefabStageClosing(PrefabStage stage)
        {
            if (stage.assetPath == "Assets/Prefabs/Room.prefab")
            {
                editingRoom = null;
            }
        }

        static void OnSceneView(SceneView view)
        {
            if (!editingRoom)
                return;

            Handles.BeginGUI();
            {
                if(GUI.Button(new Rect(10, 10, 100, 32), new GUIContent("加东西")))
                {
                    EditorWindow.GetWindow<InteractableWindow>("add new interactable", true);
                }

                if(_showRoomLayout)
                {
                    if(GUI.Button(new Rect(10, 42, 100, 32), new GUIContent("关房间布局")))
                    {
                        _showRoomLayout = false;
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(10, 42, 100, 32), new GUIContent("开房间布局")))
                    {
                        _showRoomLayout = true;
                    }
                }
            }
            Handles.EndGUI();

            void DrawRect(in Rect rect, Color color, string label)
            {
                //Room uses a left-top coordinate system
                Vector3[] points = new Vector3[5]
                {
                    rect.position,
                    rect.position + Vector2.right * rect.width,
                    rect.position + new Vector2(rect.width, -rect.height),
                    rect.position + Vector2.down * rect.height,
                    rect.position
                };
                Handles.color = color;
                Handles.DrawAAPolyLine(3, points);

                GUIStyle style = new GUIStyle();
                style.normal.textColor = color;
                Handles.Label(rect.position + Vector2.right * 0.3f, label, style);
            }

            if(_showRoomLayout)
            {
                DrawRect(editingRoom.paintingArea, Color.red, "画");
                DrawRect(editingRoom.visibleArea, Color.green, "画里可见区域");
            }
        }
    }
}

