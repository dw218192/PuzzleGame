using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PuzzleGame.Editor
{
    public class InteractableWindow : EditorWindow
    {
        public enum ECreateTarget
        {
            STATIC_OBJ,
            GENERIC_INTERACTABLE,
            PICK_UP
        }

        ECreateTarget _createTarget;

        string _name;
        Sprite _sprite;

        private void DrawInteractableGUI(ECreateTarget targetType)
        {
            _name = EditorGUILayout.TextField("name", _name);
            _sprite = (Sprite)EditorGUILayout.ObjectField(_sprite, typeof(Sprite), allowSceneObjects: true);

            if (GUILayout.Button("create"))
            {
                GameObject go = null;
                if (targetType == ECreateTarget.PICK_UP)
                    go = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Interaction/Interactable.prefab"));
                else if (targetType == ECreateTarget.GENERIC_INTERACTABLE)
                    go = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Interaction/StaticInteraction.prefab"));
                
                if(!go)
                {
                    EditorUtility.DisplayDialog("Error", $"unknown creation type {targetType} for interactables", "yes");
                    return;
                }

                go.name = _name;
                go.GetComponent<SpriteRenderer>().sprite = _sprite;

                EditorSceneManager.MoveGameObjectToScene(go, RoomDesignTool.editingRoom.gameObject.scene);
                go.transform.parent = RoomDesignTool.editingRoom.contentRoot;
                go.transform.localPosition = Vector3.zero;

                Selection.activeTransform = go.transform;
            }
        }

        private void DrawStaticObjGUI()
        {
            _name = EditorGUILayout.TextField("name", _name);
            _sprite = (Sprite)EditorGUILayout.ObjectField(_sprite, typeof(Sprite), allowSceneObjects: true);

            if (GUILayout.Button("create"))
            {
                GameObject go = new GameObject(_name);
                go.AddComponent<Actor>();
                go.isStatic = true;

                SpriteRenderer rend = go.AddComponent<SpriteRenderer>();
                rend.sprite = _sprite;
                rend.sortingLayerName = GameConst.k_interactableSpriteLayer;

                EditorSceneManager.MoveGameObjectToScene(go, RoomDesignTool.editingRoom.gameObject.scene);
                go.transform.parent = RoomDesignTool.editingRoom.contentRoot;
                go.transform.localPosition = Vector3.zero;

                go.AddComponent<BoxCollider2D>();
                Rigidbody2D rgBody = go.AddComponent<Rigidbody2D>();
                rgBody.bodyType = RigidbodyType2D.Static;

                Selection.activeTransform = go.transform;
            }
        }

        private void OnGUI()
        {
            if (!RoomDesignTool.editingRoom)
            {
                EditorGUILayout.LabelField("open Assets/Prefabs/Room first");
                return;
            }

            EditorGUILayout.BeginVertical();
            {
                _createTarget = (ECreateTarget) EditorGUILayout.EnumPopup("type", _createTarget);
                switch(_createTarget)
                {
                    case ECreateTarget.STATIC_OBJ:
                        DrawStaticObjGUI();
                        break;
                    case ECreateTarget.GENERIC_INTERACTABLE:
                    case ECreateTarget.PICK_UP:
                        DrawInteractableGUI(_createTarget);
                        break;
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}