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
        // Checks if the collider is a damageable entity that can trigger traps
        if (!isPressed && other.TryGetComponent(out IDamageable damageable) &&
            (damageable as MonoBehaviour).TryGetComponent(out Entity entity) && entity.canTriggerTraps)
        {
            isPressed = true;
            ActivateTraps(); // Activates traps when the pressure plate is pressed
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Resets the plate position when the damageable entity exits
        if (isPressed && other.TryGetComponent(out IDamageable damageable) &&
            (damageable as MonoBehaviour).TryGetComponent(out Entity entity) && entity.canTriggerTraps)
        {
            isPressed = false;
            plateToMove.transform.position = unpressedPos; // Move the plate back to its original position
        }
    }

    private void ActivateTraps()
    {
        Vector3 platePos = plateToMove.transform.position;
        unpressedPos = platePos; // Store the original position of the plate
        plateToMove.transform.position = new Vector3(platePos.x, platePos.y - .1f, platePos.z); // Move plate down
        vfx.Play(); // Play visual effects on activation
        activeSound.Play(transform.position); // Play sound effect at the trap's position

        // Activate each trap in the array
        foreach (var t in trapToActivate)
        {
            if (t.TryGetComponent(out IActivableTrap activableTrap))
            {
                activableTrap.Active(); // Call the Active method on the activable trap
            }
        }
    }
}
