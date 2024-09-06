using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventSystem))]
public class SelectionForcer : MonoBehaviour
{
    private GameObject previousSelect;

    private void Update()
    {
        var currentSelect = EventSystem.current.currentSelectedGameObject;
        if (currentSelect != null)
        {
            previousSelect = currentSelect;
        }
        
        if (currentSelect == null)
        {
            EventSystem.current.SetSelectedGameObject(previousSelect);
        }
    }
}
