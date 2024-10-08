using UnityEngine;

public class FixedMouseMouvementCamera : MonoBehaviour
{
    private Vector3 StartCameraRotation;
    [SerializeField] private Vector2 offsetMovement = new Vector2(10f, 10f);
    private bool isActive = true;

    private void Start()
    {
        StartCameraRotation = transform.rotation.eulerAngles;
        isActive = DeviceManager.Instance.IsPlayingKB(); // Set isActive based on input method
    }

    private void OnEnable()
    {
        DeviceManager.OnChangedToGamepad += () => isActive = false; // Disable camera movement on gamepad
        DeviceManager.OnChangedToKB += () => isActive = true; // Enable camera movement on keyboard
    }

    private void OnDisable()
    {
        DeviceManager.OnChangedToGamepad -= () => isActive = false; // Prevent memory leaks
        DeviceManager.OnChangedToKB -= () => isActive = true; // Prevent memory leaks
    }

    private void Update()
    {
        if (!isActive)
        {
            transform.eulerAngles = StartCameraRotation; // Reset to original rotation if not active
            return;
        }

        Vector3 mouseScreenPosition = Input.mousePosition;

        // Normalize mouse position to viewport coordinates [-1, 1]
        Vector2 mouseViewportPositionNormalize = (Camera.main.ScreenToViewportPoint(mouseScreenPosition) - Vector3.one / 2f) * 2f;

        // Clamp to ensure mouse position stays within [-1, 1]
        Vector2 mouseViewportPositionClamp = new Vector2(
            Mathf.Clamp(mouseViewportPositionNormalize.x, -1, 1),
            Mathf.Clamp(mouseViewportPositionNormalize.y, -1, 1)
        );

        // Calculate movement based on clamped position
        Vector2 mouseViewportPosition = mouseViewportPositionClamp * offsetMovement;

        // Calculate rotation adjustment from mouse movement
        Vector3 angleMovement = new Vector3(-mouseViewportPosition.y, mouseViewportPosition.x, 0f);

        // Update the camera rotation based on the initial rotation and the calculated angle
        transform.eulerAngles = StartCameraRotation + angleMovement;
    }
}
