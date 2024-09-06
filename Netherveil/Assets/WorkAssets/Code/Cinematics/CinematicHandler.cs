using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class CinematicHandler : MonoBehaviour
{
    [SerializeField] private bool skipable = false;
    [SerializeField] private bool invisibleMouse = false;
    private PlayableDirector director;

    [Header("Events")]
    [SerializeField] private UnityEvent onSkip;

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
    }

    private void OnEnable()
    {
        if (invisibleMouse)
        {
            DeviceManager.ForceMouseInvisible = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void OnDisable()
    {
        if (invisibleMouse)
        {
            DeviceManager.ForceMouseInvisible = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void Update()
    {
        bool gamepadButtonPressed = !DeviceManager.Instance.IsPlayingKB() && Gamepad.current.allControls.Any(x => x is ButtonControl && x.IsPressed() && !x.synthetic);
        bool isMouseClick = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);

        if (skipable && (Input.anyKeyDown && !isMouseClick) || gamepadButtonPressed)
            Skip();
    }

    private void Skip()
    {
        director.time = director.playableAsset.duration;
        director.Evaluate();
        director.Stop();

        onSkip?.Invoke();
    }
}
