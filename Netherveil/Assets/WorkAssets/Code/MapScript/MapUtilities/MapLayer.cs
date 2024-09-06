using UnityEngine;

public class MapLayer : MonoBehaviour
{
    public void Set()
    {
        gameObject.layer = LayerMask.NameToLayer("Map");
    }

    public void MarkUndiscovered()
    {
        if (gameObject.layer != LayerMask.NameToLayer("Map"))
        {
            gameObject.layer = LayerMask.NameToLayer("UndiscoveredRoom");
        }
    }

    public void Unset()
    {
        if (gameObject.layer == LayerMask.NameToLayer("UndiscoveredRoom"))
        {
            return;
        }
            
        gameObject.layer = LayerMask.NameToLayer("Default");
    }
}