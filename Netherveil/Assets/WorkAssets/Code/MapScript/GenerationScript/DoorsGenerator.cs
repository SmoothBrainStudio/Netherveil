using System;
using System.Collections.Generic;
using UnityEngine;

namespace Map.Generation
{
    [Serializable]
    public struct Door
    {
        public Door(Transform transform)
        {
            forward = transform.forward;
            localPosition = transform.position;
            localRotation = transform.rotation.eulerAngles.y;
            parentSkeleton = transform.parent.parent.gameObject;
            room = transform.parent.parent.parent.gameObject.GetComponent<Room>();
        }

        public Vector3 forward;
        [SerializeField] private Vector3 localPosition;
        public float localRotation;
        public GameObject parentSkeleton;
        public Room room;

        public Vector3 Forward
        {
            get
            {
                return Quaternion.Euler(0, parentSkeleton.transform.eulerAngles.y, 0) * forward;
            }
        }

        public float Rotation
        {
            get
            {
                return (parentSkeleton.transform.eulerAngles.y + localRotation) % 360;
            }
        }

        public Vector3 Position
        {
            get
            {
                Vector3 pos = localPosition + parentSkeleton.transform.position;
                Vector3 dir = pos - parentSkeleton.transform.position;
                dir = Quaternion.Euler(0, parentSkeleton.transform.eulerAngles.y, 0) * dir;
                pos = dir + parentSkeleton.transform.position;

                return pos;
            }
        }
    }

    public class DoorsGenerator : MonoBehaviour
    {
        public List<Door> doors = new List<Door>();

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (var door in doors)
            {
                Gizmos.DrawSphere(door.Position, 0.25f);
            }
        }

        private void OnValidate()
        {
            for (int i = 0; i < doors.Count; i++)
            {
                Door door = doors[i];
                door.room = door.parentSkeleton.transform.parent.gameObject.GetComponent<Room>();
                doors[i] = door;
            }
        }

#if UNITY_EDITOR
        public void GeneratePrefab()
        {
            doors.Clear();

            foreach (Transform child in transform)
            {
                doors.Add(new Door(child));
            }

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
#endif

        public void RemoveDoor(Door door)
        {
            if (!doors.Remove(door))
            {
                Debug.LogWarning("Try to remove a door with the wrong struct", gameObject);
                return;
            }
        }

        public void EmptyDoors()
        {
            doors.Clear();
        }
    }
}