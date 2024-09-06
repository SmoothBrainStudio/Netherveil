using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    private void Update()
    {
        transform.forward = Vector3.right;
        transform.Rotate(90f, 0f, 90f);
    }
}
