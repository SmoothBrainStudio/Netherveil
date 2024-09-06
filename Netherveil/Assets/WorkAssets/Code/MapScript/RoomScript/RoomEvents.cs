using Map.Generation;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;

namespace Map.Component
{
    public class RoomEvents : MonoBehaviour
    {
        private RoomData roomData;

        private Room room;
        private GameObject treasures;
        private NavMeshSurface navMeshSurface;

        static private bool hasLeaved = true;
        private bool allChestsOpenCalled = false;
        private bool allEnemiesDeadCalled = false;
        private bool enterRoomCalled = false;
        private bool exitRoomCalled = false;

        private Collider triggerCollide = null;
        private Vector3 enterPos = Vector3.zero;

        private void Awake()
        {
            foreach (Collider coll in GetComponents<Collider>())
            {
                if (coll.isTrigger)
                {
                    triggerCollide = coll;
                    break;
                }
            }

            // find room go's
            room = transform.parent.gameObject.GetComponent<Room>();
            room.Presets.GenerateRandomPreset(); // TODO : Generate preset else where
            treasures = room.Presets.transform.GetChild(0).Find("Treasures").gameObject;
            navMeshSurface = room.GetComponentInChildren<NavMeshSurface>(true);

            // create data of the map
            roomData = new RoomData(room);
        }

        private void Start()
        {
            //if (roomCleared)
            //{
            //    // je suis obligé de faire ça pour que la seed soit correcte
            //    // par rapport à l'ancienne sauvegarde si le joueur tue un boss
            //    if (roomData.Type == RoomType.Boss)
            //    {
            //        //Seed.Iterate(3);
            //    }
            //}

            if (roomData.Type == RoomType.Lobby)
            {
                EnterEvents();
            }

            // set bool to true to not call the events in the room if there is no enemy
            allEnemiesDeadCalled = (room.Enemies.transform.childCount == 0);
            // set bool to true to not call the events in the room if there is no chest
            allChestsOpenCalled = (treasures.GetComponentsInChildren<Item>().Count() == 0);
        }

        private void EnterEvents()
        {
            enterRoomCalled = true;
            RoomEvents.hasLeaved = false;
            MapUtilities.currentRoomData = roomData;
            MapUtilities.nbEnterRoomByType[MapUtilities.currentRoomData.Type] += 1;
            room.Enter();
        }

        private void FirstExitEvents()
        {
            exitRoomCalled = true;
            RoomEvents.hasLeaved = true;
            room.Exit();

            for (int i = 0; i < transform.parent.parent.childCount; i++)
            {
                if (transform.parent.parent.GetChild(i) == transform.parent)
                {
                    FindObjectOfType<MapGenerator>().roomClearIds.Add(i);
                    break;
                }
            }

            // global events
            MapUtilities.onFirstExit?.Invoke();

            SaveManager.Save();
        }

        private void LateUpdate()
        {
            if (!allEnemiesDeadCalled && room.Enemies.transform.childCount == 0)
            {
                allEnemiesDeadCalled = true;
                room.AllEnemiesDead();
            }

            if (!allChestsOpenCalled && treasures.GetComponentsInChildren<Item>().Count() == 0) // <- extremement lourd
            {
                allChestsOpenCalled = true;
                room.AllChestsOpen();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                enterPos = triggerCollide.ClosestPointOnBounds(other.bounds.center);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!enterRoomCalled && hasLeaved && other.gameObject.CompareTag("Player"))
            {
                Vector3 enterToPlayer = enterPos - other.bounds.center;
                if (enterToPlayer.magnitude >= 4f)
                {
                    EnterEvents();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (!exitRoomCalled && !hasLeaved)
                {
                    FirstExitEvents();
                }

                MapUtilities.onEarlyExitRoom?.Invoke();
                MapUtilities.onExitRoom?.Invoke();
            }
        }

        public void Clear()
        {
            enterRoomCalled = true;
            RoomEvents.hasLeaved = false;
            MapUtilities.currentRoomData = roomData;
            MapUtilities.nbEnterRoomByType[MapUtilities.currentRoomData.Type] += 1;
            room.Enter();

            exitRoomCalled = true;
            RoomEvents.hasLeaved = true;
            room.Exit();

            if (roomData.Type == RoomType.Boss)
            {
                Seed.Iterate(2);
            }
        }
    }
}