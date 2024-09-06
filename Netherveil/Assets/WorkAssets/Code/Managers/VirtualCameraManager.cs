using Cinemachine;
using UnityEngine;

public class VirtualCameraManager : MonoBehaviour
{
    private CinemachineVirtualCamera[] virtualCameras;

    private void Start()
    {
        virtualCameras = FindObjectsOfType<CinemachineVirtualCamera>();
    }

    public void SetCamera(CinemachineVirtualCamera cam)
    {
        foreach (var camera in virtualCameras)
        {
            camera.Priority = 0;
        }
        cam.Priority = 100;
    }
}
