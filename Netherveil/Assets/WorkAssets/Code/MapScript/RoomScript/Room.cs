using Map.Component;
using Map.Generation;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace Map
{
    public class Room : MonoBehaviour
    {
        [SerializeField] public RoomType type = RoomType.Normal;

        [field: SerializeField, HideInInspector] public Skeleton Skeleton { get; private set; } = null;
        [field: SerializeField, HideInInspector] public DoorsGenerator DoorsGenerator { get; private set; } = null;
        [field: SerializeField, HideInInspector] public StaticProps StaticProps { get; private set; } = null;
        [field: SerializeField, HideInInspector] public Lights Lights { get; private set; } = null;
        [field: SerializeField, HideInInspector] public RoomUI UI { get; private set; } = null;
        [field: SerializeField, HideInInspector] public RoomEvents Events { get; private set; } = null;
        [field: SerializeField, HideInInspector] public RoomPresets Presets { get; private set; } = null;

        private RoomEnemies roomEnemies = null; // can't be initialized in editor
        public RoomEnemies Enemies
        {
            get
            {
                if (roomEnemies == null)
                {
                    roomEnemies = Presets.GetComponentInChildren<RoomEnemies>(true);
                }

                return roomEnemies;
            }
        }

        public NavMeshSurface NavMesh
        {
            get
            {
                return GetComponentInChildren<NavMeshSurface>(true);
            }
        }

        readonly public List<Room> neighbors = new List<Room>();

        private void OnValidate()
        {
            Skeleton = transform.GetComponentInChildren<Skeleton>(true);
            DoorsGenerator = transform.GetComponentInChildren<DoorsGenerator>(true);
            StaticProps = transform.GetComponentInChildren<StaticProps>(true);
            Lights = transform.GetComponentInChildren<Lights>(true);
            UI = transform.GetComponentInChildren<RoomUI>(true);
            Presets = transform.GetComponentInChildren<RoomPresets>(true);
            Events = transform.GetComponentInChildren<RoomEvents>(true);
        }

        public void Enter()
        {
            // set all elements to the map layer now that we can see them
            foreach (var c in GetComponentsInChildren<MapLayer>(true))
            {
                c.Set();
            }

            // set all neighbor elements has undiscovered
            foreach (Room neighbor in neighbors)
            {
                foreach (var c in neighbor.GetComponentsInChildren<MapLayer>(true))
                {
                    c.MarkUndiscovered();
                }

                // activate ui
                if (neighbor.UI)
                {
                    neighbor.UI.gameObject.SetActive(true);
                }
            }

            // activate ui
            if (UI)
            {
                UI.gameObject.SetActive(true);
            }

            NavMesh.enabled = true;
            Enemies.gameObject.SetActive(true);

            // Call Events
            MapUtilities.onEarlyFirstEnter?.Invoke();
            MapUtilities.onFirstEnter?.Invoke();
        }

        public void Exit()
        {
            NavMesh.enabled = false;
            Enemies.gameObject.SetActive(false);
        }

        public void AllEnemiesDead()
        {
            MapUtilities.onEarlyAllEnemiesDead?.Invoke();
            MapUtilities.onAllEnemiesDead?.Invoke();
        }

        public void AllChestsOpen()
        {
            MapUtilities.onEarlyAllChestOpen?.Invoke();
            MapUtilities.onAllChestOpen?.Invoke();
        }

        public void Unclear()
        {
            Enemies.gameObject.SetActive(false);
            foreach (var c in GetComponentsInChildren<MapLayer>(true))
            {
                c.Unset();
            }

            var roomUI = GetComponentInChildren<RoomUI>(true);
            if (roomUI)
            {
                roomUI.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Make the room marked has "cleared" in game
        /// </summary>
        public void ClearPreset()
        {
            Enemies.Clear();
            Events.Clear();
        }
    }
}