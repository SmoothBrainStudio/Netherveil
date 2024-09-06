using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotationVFX : MonoBehaviour
{
    [SerializeField] float RotationSpeed;
    void Start()
    {
        
    }


    void Update()
    {
        gameObject.transform.Rotate(0f, RotationSpeed, 0F);
    }
}
