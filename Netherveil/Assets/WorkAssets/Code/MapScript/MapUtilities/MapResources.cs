using Map.Generation;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class MapResources : MonoBehaviour
    {
        static bool load = false;
        static private readonly Dictionary<RoomType, List<Room>> roomPrefabsByType = new Dictionary<RoomType, List<Room>>()
        {
            { RoomType.Lobby, new List<Room>() },
            { RoomType.Tutorial, new List<Room>() },
            { RoomType.Normal, new List<Room>() },
            { RoomType.Treasure, new List<Room>() },
            { RoomType.Challenge, new List <Room>() },
            { RoomType.Merchant, new List <Room>() },
            { RoomType.Secret, new List <Room>() },
            { RoomType.MiniBoss, new List <Room>() },
            { RoomType.Boss, new List <Room>() },
        };

        [SerializeField] public List<Room> roomsToLoad;

        [SerializeField] private List<GameObject> obstructionDoors;
        [SerializeField] private List<GameObject> stairsPrefabs;
        [SerializeField] private GameObject gatePrefab;

        static public List<GameObject> ObstructionDoors;
        static public List<GameObject> StairsPrefabs;
        static public GameObject GatePrefab;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadGameResources()
        {
            GameObject.Instantiate(Resources.Load<GameObject>(nameof(MapResources)));
        }

        private void Awake()
        {
            if (!load)
            {
                ObstructionDoors = obstructionDoors;
                StairsPrefabs = stairsPrefabs;
                GatePrefab = gatePrefab;

                foreach (Room roomPrefab in roomsToLoad)
                {
                    roomPrefabsByType[roomPrefab.type].Add(roomPrefab);
                }

                load = true;
            }

            Destroy(gameObject);
        }

        public static Room RandRoomPrefab(RoomType type)
        {
            List<Room> roomPrefabs = roomPrefabsByType[type];

            if (roomPrefabs.Count == 0)
            {
                Debug.LogWarning("No room of type " + type + " in MapResources");
                return null;
            }

            return roomPrefabs[Seed.Range(0, roomPrefabs.Count)];
        }

        public static List<Room> RoomPrefabs(RoomType listType)
        {
            return roomPrefabsByType[listType];
        }
    }
}