using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] bool saucisse  = false;
    void Start()
    {
        
    }

    void Update()
    {
        if (saucisse)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1.0f;
        }
    }
}
