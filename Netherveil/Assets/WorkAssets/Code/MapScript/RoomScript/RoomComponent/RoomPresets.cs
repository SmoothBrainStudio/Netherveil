using Map.Generation;
using UnityEngine;

namespace Map.Component
{
    public class RoomPresets : MonoBehaviour
    {
        public void GenerateRandomPreset()
        {
            if (transform.childCount == 0)
            {
                Debug.LogError("RoomPresets doesn't have any preset children, try adding one", gameObject);
                return;
            }

            // Find a random preset and activate it
            int indexRandPreset = Seed.Range(0, transform.childCount);
            transform.GetChild(indexRandPreset).gameObject.SetActive(true);

            // Destroy all the other preset
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (i != indexRandPreset)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
        }
    }
}
