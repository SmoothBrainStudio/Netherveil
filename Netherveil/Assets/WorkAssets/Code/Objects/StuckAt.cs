using UnityEngine;

public class StuckAt : MonoBehaviour
{
    private Vector3 startLocalPosition;
    private Vector3 startLocalEuleurAngles;
    private Vector3 startPosition;
    private Vector3 startEuleurAngles;

    [SerializeField] private bool localPosition = true;

    [SerializeField] private bool stuckPositionX = true;
    [SerializeField] private bool stuckPositionY = true;
    [SerializeField] private bool stuckPositionZ = true;

    [Space]

    [SerializeField] private bool localRotation = true;

    [SerializeField] private bool stuckRotationX = true;
    [SerializeField] private bool stuckRotationY = true;
    [SerializeField] private bool stuckRotationZ = true;

    private void Start()
    {
        startLocalPosition = transform.localPosition;
        startLocalEuleurAngles = transform.localEulerAngles;

        startPosition = transform.position;
        startEuleurAngles = transform.eulerAngles;
    }

    private void Update()
    {
        Vector3 position = localPosition ? transform.localPosition : transform.position;
        Vector3 rotation = localRotation ? transform.localEulerAngles : transform.eulerAngles;

        if (stuckPositionX)
            position.x = localPosition ? startLocalPosition.x : startPosition.x;
        if (stuckPositionY)
            position.y = localPosition ? startLocalPosition.y : startPosition.y;
        if (stuckPositionZ)
            position.z = localPosition ? startLocalPosition.z : startPosition.z;

        if (stuckRotationX)
            rotation.x = localRotation ? startLocalEuleurAngles.x : startLocalEuleurAngles.x;
        if (stuckRotationY)
            rotation.y = localRotation ? startLocalEuleurAngles.y : startLocalEuleurAngles.y;
        if (stuckRotationZ)
            rotation.z = localRotation ? startLocalEuleurAngles.z : startLocalEuleurAngles.z;

        if (localPosition)
        {
            transform.localPosition = position;
        }
        else
        {
            transform.position = position;
        }

        if (localRotation)
        {
            transform.localEulerAngles = rotation;
        }
        else
        {
            transform.eulerAngles = rotation;
        }
    }
}
