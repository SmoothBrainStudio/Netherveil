using UnityEngine;

public class PressurePlateTrap : MonoBehaviour
{
    [SerializeField] ParticleSystem vfx;
    [SerializeField] private GameObject[] trapToActivate;
    [SerializeField] private Sound activeSound;
    [SerializeField] private GameObject plateToMove;

    private bool isPressed = false;
    private Vector3 unpressedPos = Vector3.zero;

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed && other.TryGetComponent(out IDamageable damageable) && (damageable as MonoBehaviour).TryGetComponent(out Entity entity) && entity.canTriggerTraps)
        {
            isPressed = true;
            ActivateTraps();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isPressed && other.TryGetComponent(out IDamageable damageable) && (damageable as MonoBehaviour).TryGetComponent(out Entity entity) && entity.canTriggerTraps)
        {
            isPressed = false;
            plateToMove.transform.position = unpressedPos;
        }
    }

    private void ActivateTraps()
    {
        Vector3 platePos = plateToMove.transform.position;
        unpressedPos = platePos;
        plateToMove.transform.position = new Vector3(platePos.x, platePos.y - .1f, platePos.z);
        vfx.Play();
        activeSound.Play(transform.position);

        foreach (var t in trapToActivate)
        {
            if (t.TryGetComponent(out IActivableTrap activableTrap))
            {
                activableTrap.Active();
            }
        }
    }
}