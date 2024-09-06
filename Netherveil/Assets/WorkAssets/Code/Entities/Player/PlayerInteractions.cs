using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    public List<IInterractable> InteractablesInRange { get ; private set; } = new List<IInterractable>();

    private void Update()
    {
        SelectClosestItem();
    }

    private void SelectClosestItem()
    {
        if (InteractablesInRange.Count == 0)
            return;

        Vector2 playerPos = transform.position.ToCameraOrientedVec2();
        InteractablesInRange = InteractablesInRange.OrderBy(interactable =>
        {
            Vector2 itemPos = (interactable as MonoBehaviour).transform.position.ToCameraOrientedVec2();
            return Vector2.Distance(playerPos, itemPos);
        }
        ).ToList();


        for (int i = 1; i< InteractablesInRange.Count; ++i)
        {
            InteractablesInRange[i].Deselect();
        }

        InteractablesInRange[0].Select();
    }
}
