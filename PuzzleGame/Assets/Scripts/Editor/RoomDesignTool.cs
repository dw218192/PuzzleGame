using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

namespace PuzzleGame.Editor
{
    public static class RoomDesignTool
    {
        public static PrefabStage editingStage { get; private set; } = null;

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
                editingStage = stage;
            }
        }

        static void OnPrefabStageClosing(PrefabStage stage)
        {
            if (stage.assetPath == "Assets/Prefabs/Room.prefab")
            {
                editingStage = null;
            }
        }

        static void OnSceneView(SceneView view)
        {
            if (!editingStage)
                return;

            Handles.BeginGUI();
            {
                if(GUI.Button(new Rect(10, 10, 100, 32), new GUIContent("加东西")))
                {
                    EditorWindow.GetWindow<InteractableWindow>();
                }
            }
            Handles.EndGUI();
        }
    }
}

