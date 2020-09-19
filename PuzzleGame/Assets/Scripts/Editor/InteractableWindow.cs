using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PuzzleGame.Editor
{
    public class InteractableWindow : EditorWindow
    {
        EInteractType _type;
        string _name;
        Sprite _sprite;

        private void OnGUI()
        {
            if (!RoomDesignTool.editingStage)
            {
                EditorGUILayout.LabelField("open Assets/Prefabs/Room first");
                return;
            }

            EditorGUILayout.BeginVertical();
            {
                _name = EditorGUILayout.TextField("name", _name);
                _type = (EInteractType) EditorGUILayout.EnumPopup("interaction type", _type);
                _sprite = (Sprite) EditorGUILayout.ObjectField(_sprite, typeof(Sprite), allowSceneObjects:true);

                if(GUILayout.Button("create"))
                {
                    GameObject go = new GameObject(_name);
                    Interactable interactable = go.AddComponent<Interactable>();
                    interactable.type = _type;

                    SpriteRenderer rend = go.AddComponent<SpriteRenderer>();
                    rend.sprite = _sprite;
                    rend.sortingLayerName = GameConst.k_interactableSpriteLayer;

                    EditorSceneManager.MoveGameObjectToScene(go, RoomDesignTool.editingStage.scene);
                    go.transform.parent = RoomDesignTool.editingStage.prefabContentsRoot.transform;
                    go.transform.localPosition = Vector3.zero;

                    Selection.activeTransform = go.transform;
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}