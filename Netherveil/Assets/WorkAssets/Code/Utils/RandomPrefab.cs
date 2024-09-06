using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class RandomPrefab : MonoBehaviour
{
    [SerializeField] List<GameObject> prefabs;
    public void Create()
    {
        if (prefabs != null && prefabs.Any())
        {
            Instantiate(prefabs[Random.Range(0, prefabs.Count)], transform.position, transform.rotation);
        }

        DestroyImmediate(gameObject);
    }
}
