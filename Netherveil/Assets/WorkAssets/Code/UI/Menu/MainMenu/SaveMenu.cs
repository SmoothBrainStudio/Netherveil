using MeshUI;
using UnityEngine;
using UnityEngine.EventSystems;

public class SaveMenu : MonoBehaviour
{
    [SerializeField] private MeshButton[] meshButtons;
    private new Collider collider;

    private void Start()
    {
        collider = GetComponent<Collider>();
    }
    public void EnableMenu()
    {
        collider.enabled = false;
        foreach (MeshButton button in meshButtons)
        {
            button.enabled = true;
        }
    }

    public void DisableMenu()
    {
        collider.enabled = true;
        foreach (MeshButton button in meshButtons)
        {
            button.enabled = false;
        }
    }
}
