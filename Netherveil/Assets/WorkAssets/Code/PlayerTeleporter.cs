using UnityEngine;

public class PlayerTeleporter : MonoBehaviour
{
    [SerializeField] private Transform toTeleport;
    private Transform player;

    private void Awake()
    {
        player = Utilities.Player.transform;
    }

    public void Teleport()
    {
        player.GetComponent<CharacterController>().enabled = false;

        player.transform.position = toTeleport.position;
        player.transform.rotation = toTeleport.rotation;

        player.GetComponent<CharacterController>().enabled = true;
    }
}
