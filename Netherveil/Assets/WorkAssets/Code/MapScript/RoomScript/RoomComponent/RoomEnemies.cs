using UnityEngine;

namespace Map.Component
{
    public class RoomEnemies : MonoBehaviour
    {
        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }
}