using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private void Start()
    {
        Utilities.Player.GetComponent<CharacterController>().enabled = false;
        Utilities.Player.transform.position = transform.position;
        Utilities.Player.GetComponent<CharacterController>().enabled = true;
        Camera.main.transform.position = transform.position;
    }
}