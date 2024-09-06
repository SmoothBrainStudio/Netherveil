using Map;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tool
{
    public class RoomGeneratorWindow : EditorWindow
    {
        RoomType roomType = RoomType.Normal;
        string roomName = "";
        GameObject houdiniRoom;
        GameObject bakedRoom;

        [UnityEditor.MenuItem("Tools/Room/Create")]
        public static void CreateRoom()
        {
            EditorWindow.GetWindow(typeof(RoomGeneratorWindow));
        }

        Editor gameObjectEditor;
        private void OnGUI()
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.BeginChangeCheck();
            houdiniRoom = EditorGUILayout.ObjectField("Houdini Room", houdiniRoom, typeof(GameObject), true) as GameObject;
            if (EditorGUI.EndChangeCheck())
            {
                DestroyImmediate(gameObjectEditor);
                if (houdiniRoom != null)
                {
                    if (gameObjectEditor == null)
                        gameObjectEditor = Editor.CreateEditor(houdiniRoom);
                }
            }

            bakedRoom = EditorGUILayout.ObjectField("Baked Room", bakedRoom, typeof(GameObject), true) as GameObject;
            roomName = EditorGUILayout.TextField("Prefab Name", roomName);
            roomType = (RoomType)EditorGUILayout.EnumPopup("Type of room", roomType);

            if (GUILayout.Button("Generate Room"))
            {
                GenerateRoomPrefab();
            }

            if (gameObjectEditor != null)
            {
                gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetAspectRect(2), GUIStyle.none);
            }

            EditorGUILayout.EndVertical();
        }

        private GameObject CreateRoomGameObject()
        {
            Object source = Resources.Load("RoomPrefab");
            GameObject roomGO = (GameObject)PrefabUtility.InstantiatePrefab(source);
            roomGO.name = roomName;
            roomGO.GetComponent<Room>().type = roomType;

            return roomGO;
        }

        private void SaveRoomGameObject(GameObject roomGO)
        {
            string typeRoomPath = "/SampleSceneAssets/Levels/Prefabs/Map/Room/" + roomType.ToString();
            string roomFolderPath = typeRoomPath + "/" + roomName;
            string roomPrefabPath = roomFolderPath + "/" + roomName + ".prefab";

            string meshPath = "Assets/SampleSceneAssets/Art/Meshs/Room/" + roomType.ToString() + "/" + roomName + "Mesh.asset";

            if (!Directory.Exists(UnityEngine.Application.dataPath + roomFolderPath))
            {
                AssetDatabase.CreateFolder("Assets" + typeRoomPath, roomName);
            }
                
            // save mesh
            AssetDatabase.CreateAsset(roomGO.GetComponent<Room>().Skeleton.GetComponent<MeshFilter>().sharedMesh, meshPath);
            AssetDatabase.SaveAssets();
            // save prefab
            PrefabUtility.SaveAsPrefabAsset(roomGO, UnityEngine.Application.dataPath + roomPrefabPath);
        }

        private void GenerateRoomPrefab()
        {
            GameObject roomGO = CreateRoomGameObject();
            Room room = roomGO.GetComponent<Room>();

            Mesh sharedMesh = Instantiate(bakedRoom.GetComponentInChildren<MeshFilter>(true).sharedMesh);
            room.Skeleton.GetComponent<MeshFilter>().sharedMesh = sharedMesh;
            room.Skeleton.GetComponent<MeshCollider>().sharedMesh = sharedMesh;
            room.Skeleton.GetComponent<BoxCollider>().center = sharedMesh.bounds.center;
            room.Skeleton.GetComponent<BoxCollider>().size = sharedMesh.bounds.size;

            room.Skeleton.GetComponent<MeshRenderer>().sharedMaterials = bakedRoom.GetComponentInChildren<MeshRenderer>(true).sharedMaterials;

            foreach (Transform child in houdiniRoom.transform.Find("HDA_Data").Find("Instances_1"))
            {
                if (child.name.Contains("Door_Instance"))
                {
                    Instantiate(child.gameObject, room.DoorsGenerator.transform);
                }
                else
                {
                    Instantiate(child.gameObject, room.StaticProps.transform);
                }
            }
            room.DoorsGenerator.GeneratePrefab();

            SaveRoomGameObject(roomGO);
            
            // destroy garbage in scene
            DestroyImmediate(roomGO);
        }
    }
}