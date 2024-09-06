using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Map
{
    [CustomEditor(typeof(Room))]
    public class RoomEditor : Editor
    {
        private Room roomScript;
        //private Transform roomPreset;
        //private int currentIndex = 0;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(4);

            roomScript = target as Room;

            if (GUILayout.Button("Bake rooms"))
            {
                BakeAllRooms();
            }
        }

        private void BakeAllRooms()
        {
            foreach (Transform t in roomScript.Presets.transform)
            {
                t.gameObject.SetActive(true);
                BakeRoom(roomScript, t);
                t.gameObject.SetActive(false);
            }
        }

        private void BakeRoom(Room roomScript, Transform transformRoom)
        {
            if (transformRoom.TryGetComponent(out NavMeshSurface navMesh))
            {
                navMesh.BuildNavMesh();

                if (navMesh.navMeshData != null)
                {
                    string prefabPath = GetPrefabPath();
                    string directory = Path.GetDirectoryName(prefabPath);
                    string filePath = Path.Combine(directory, $"{roomScript.gameObject.name}-{transformRoom.name}.asset");

                    AssetDatabase.CreateAsset(navMesh.navMeshData, filePath);
                    EditorUtility.SetDirty(navMesh);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    throw new System.Exception($"NavMesh data of the room \"{transformRoom.name}\" is null!");
                }

                MakeSceneDirty();
            }
        }

        private string GetPrefabPath()
        {
            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage)
                return stage.assetPath;
            else
                return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(roomScript.transform);
        }

        private void MakeSceneDirty()
        {
            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage)
                EditorSceneManager.MarkSceneDirty(stage.scene);
        }
    }
}
