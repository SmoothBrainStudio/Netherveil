using UnityEngine;

[ExecuteInEditMode]
public class Billboard : MonoBehaviour
{
    private void Update()
    {
        transform.LookAt(Camera.main.transform.position);
        transform.Rotate(0f, 180f, 0f);
    }
}
