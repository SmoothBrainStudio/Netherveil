using UnityEngine;

public class MouseOnScreenBorder : MonoBehaviour
{
    private Vector3 targetPosition;
    private Vector3 currentVelocity = Vector3.zero;
    private float smoothTime = 0.2f;
    private Transform playerTransform;
    private PlayerInput playerInput;
    private UnityEngine.InputSystem.PlayerInput playerInputComponent;
    private Hero player;

    void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>();
        playerInput = playerTransform.gameObject.GetComponent<PlayerInput>();
        playerInputComponent = playerTransform.gameObject.GetComponent<UnityEngine.InputSystem.PlayerInput>();
    }

    void FixedUpdate()
    {
        // Calls the appropriate method based on input device (keyboard or joystick)
        if (DeviceManager.Instance.IsPlayingKB())
        {
            CollidMouseScreen();
        }
        else
        {
            CollideJoystickScreen();
        }

        ChangeOffsetPos();
    }

    private void CollidMouseScreen()
    {
        Vector2 mousepos = Input.mousePosition;
        Vector3 offsetX = Vector3.zero;
        Vector3 offsetY = Vector3.zero;
        float offsetDistBorder = 10f; // Distance from screen border to trigger offset
        float offsetDistCam = 2f; // Offset distance for the camera

        // Determine offsets based on mouse position relative to the screen edges
        if (mousepos.x > Screen.width - offsetDistBorder)
        {
            offsetX = Camera.main.transform.right * offsetDistCam;
        }
        else if (mousepos.x < offsetDistBorder)
        {
            offsetX = -Camera.main.transform.right * offsetDistCam;
        }

        if (mousepos.y > Screen.height - offsetDistBorder)
        {
            offsetY = Camera.main.transform.up * offsetDistCam;
        }
        else if (mousepos.y < offsetDistBorder)
        {
            offsetY = -Camera.main.transform.up * offsetDistCam;
        }

        // Set the target position based on calculated offsets
        targetPosition = playerTransform.position + offsetX + offsetY;
    }

    private void CollideJoystickScreen()
    {
        Vector2 joyStickInput = playerInputComponent.actions.FindActionMap("Gamepad", throwIfNotFound: true)["CamLookAway"].ReadValue<Vector2>();
        Vector3 offsetX = Vector3.zero;
        Vector3 offsetY = Vector3.zero;
        float offsetDistCam = 2f; // Offset distance for the camera

        // Determine offsets based on joystick input
        if (joyStickInput.x > 0.5f)
        {
            offsetX = Camera.main.transform.right * offsetDistCam;
        }
        else if (joyStickInput.x < -0.5f)
        {
            offsetX = -Camera.main.transform.right * offsetDistCam;
        }

        if (joyStickInput.y > 0.5f)
        {
            offsetY = Camera.main.transform.up * offsetDistCam;
        }
        else if (joyStickInput.y < -0.5f)
        {
            offsetY = -Camera.main.transform.up * offsetDistCam;
        }

        // Set the target position based on calculated offsets
        targetPosition = playerTransform.position + offsetX + offsetY;
    }

    private void ChangeOffsetPos()
    {
        // Smoothly move towards the target position unless certain conditions are met
        if (targetPosition != playerTransform.position && playerInput.Direction == Vector2.zero && player.State != (int)Entity.EntityState.DEAD)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        }
        else
        {
            // If conditions aren't met, return to player position with reduced smoothness
            transform.position = Vector3.SmoothDamp(transform.position, playerTransform.position, ref currentVelocity, smoothTime / 3);
        }
    }
}
