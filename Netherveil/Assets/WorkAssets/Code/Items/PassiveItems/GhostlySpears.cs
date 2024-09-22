using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class GhostlySpears : ItemEffect , IPassiveItem 
{
    readonly List<GameObject> ghostSpears = new();
    readonly List<GameObject> spearThrowWrappers = new();
    readonly List<GameObject> spearVFXs = new();
    readonly float DEGREE_OFFSET = 15f;
    public void OnRetrieved() 
    {
        //used a wrapper instead of the object itself to make it rotate from player's position, not the middle of the collide
        GameObject spearThrowWrapper = Utilities.PlayerController.SpearThrowWrapper;
        GameObject spearVFX = Utilities.PlayerController.SpearLaunchVFX.gameObject;

        for(int i = 0; i< 2; ++i)
        {
            spearThrowWrappers.Add(GameObject.Instantiate(spearThrowWrapper, spearThrowWrapper.transform.position,
            spearThrowWrapper.transform.rotation, spearThrowWrapper.transform.parent));
            spearVFXs.Add(GameObject.Instantiate(spearVFX, spearVFX.transform.position,
            spearVFX.transform.rotation));
        }
        Spear.NbSpears += 2;

        Utilities.PlayerInput.OnThrowSpear += ThrowGhostlySpears;
        Utilities.PlayerInput.OnRetrieveSpear += RetrieveGhostlySpears;
        Spear.OnPlacedInHand += DestroyGhostlySpears;
    }

    public void OnRemove()
    {
        for (int i = 0; i < spearThrowWrappers.Count; ++i)
        {
            GameObject.Destroy(spearThrowWrappers[i]);
            GameObject.Destroy(spearVFXs[i]);
        }
        spearThrowWrappers.Clear();
        spearVFXs.Clear();

        Spear.NbSpears -= 2;

        Utilities.PlayerInput.OnThrowSpear -= ThrowGhostlySpears;
        Utilities.PlayerInput.OnRetrieveSpear -= RetrieveGhostlySpears;
        Spear.OnPlacedInHand -= DestroyGhostlySpears;
    } 

    private void ThrowGhostlySpears(Vector3 posToReach)
    {
        GameObject spear = Utilities.PlayerController.Spear.gameObject;
        PlayerController player = Utilities.PlayerController;

        for (int i = 0; i< spearThrowWrappers.Count; ++i)
        {
            GameObject ghostSpear = GameObject.Instantiate(spear, spear.transform.position, spear.transform.rotation, spear.transform.parent);
            ghostSpear.GetComponentInChildren<MeshRenderer>().material = GameResources.Get<Material>("PhantomSpearMat");
            ghostSpear.GetComponentInChildren<VisualEffect>().Play();
            ghostSpears.Add(ghostSpear);

            ghostSpear.GetComponent<Spear>().SpearThrowCollider = spearThrowWrappers[i].GetComponentInChildren<BoxCollider>(includeInactive: true);
            Vector3 newPosToReach = Quaternion.Euler(0f, i != 0 ? DEGREE_OFFSET : -DEGREE_OFFSET, 0f) * (posToReach - player.transform.position) + player.transform.position;
            spearThrowWrappers[i].transform.LookAt(newPosToReach);

            spearVFXs[i].transform.position = player.transform.position;
            spearVFXs[i].transform.LookAt(newPosToReach);
            spearVFXs[i].GetComponent<VisualEffect>().Play();

            ghostSpear.GetComponent<Spear>().Throw(newPosToReach);
        }
    }

    private void RetrieveGhostlySpears()
    {
        foreach(GameObject spear in ghostSpears)
        {
            spear.GetComponent<Spear>().Return();
        }
    }

    private void DestroyGhostlySpears()
    {
        foreach(GameObject spear in ghostSpears)
        {
            GameObject.Destroy(spear);
        }

        ghostSpears.Clear();
    }
} 
