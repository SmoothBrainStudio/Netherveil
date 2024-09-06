using UnityEngine;

public class MoveTowardUpdate : MonoBehaviour
{
    [SerializeField] private float speed;

    void Update()
    {
        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, gameObject.transform.position + transform.forward, Time.deltaTime * speed);
    }
}
