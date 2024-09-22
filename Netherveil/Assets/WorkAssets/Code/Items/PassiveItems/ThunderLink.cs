using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;
//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class ThunderLink : ItemEffect, IPassiveItem
{
    readonly List<BoxCollider> thunderLinkColliders = new();
    readonly List<VisualEffect> thunderLinkVFXs = new();
    readonly List<LineRenderer> thunderLinkLineRenderers = new();
    readonly List<Spear> spears = new();
    Coroutine thunderlinkRoutine = null;
    Coroutine moveRoutine = null;
    readonly float THUNDERLINK_WAIT_TIME = 0.15f;
    readonly float duration = 3f;
    readonly float chance = 0.2f;
    bool allSpearsSet = false;
    public int displayDamages;

    public ThunderLink()
    {
        displayDamages = (int)(2f * Utilities.Hero.Stats.GetCoeff(Stat.ATK));
    }

    public void OnRetrieved()
    {
        Spear.OnPlacedInWorld += CreateEletricLinks;
        Spear.OnLatePlacedInWorld += WaitAllSpearsSpawned;
        Utilities.PlayerInput.OnRetrieveSpear += DeleteEletricLinks;
    }

    public void OnRemove()
    {
        Spear.OnPlacedInWorld -= CreateEletricLinks;
        Utilities.PlayerInput.OnRetrieveSpear -= DeleteEletricLinks;
    }

    private void DeleteEletricLinks()
    {
        if(thunderlinkRoutine != null)
            CoroutineManager.Instance.StopCoroutine(thunderlinkRoutine);
        if(moveRoutine != null)
            CoroutineManager.Instance.StopCoroutine(moveRoutine);
        thunderlinkRoutine = null;
        moveRoutine = null;


        for (int i = 0; i < thunderLinkVFXs.Count; i++)
        {
            GameObject.Destroy(thunderLinkVFXs[i].gameObject);
            GameObject.Destroy(thunderLinkLineRenderers[i].gameObject);
        }

        spears.Clear();
        thunderLinkColliders.Clear();
        thunderLinkVFXs.Clear();
        thunderLinkLineRenderers.Clear();
    }

    private void CreateEletricLinks(Spear spear)
    {
        thunderLinkColliders.Add(spear.SpearThrowCollider);

        VisualEffect vfx = GameObject.Instantiate(GameResources.Get<GameObject>("VFX_ThunderLink").GetComponent<VisualEffect>(), GameObject.FindWithTag("Player").transform.position, Quaternion.identity);
        vfx.transform.position = GameObject.FindWithTag("Player").transform.position + Vector3.up;
        vfx.SetVector3("Attract Target", spear.transform.position + Vector3.up);
        vfx.GetComponent<VFXPropertyBinder>().GetPropertyBinders<VFXPositionBinderCustom>().ToArray()[0].Target = spear.transform;
        vfx.Play();
        thunderLinkVFXs.Add(vfx);



        LineRenderer lineRenderer = GameObject.Instantiate(GameResources.Get<GameObject>("VFX_ThunderLinkLine").GetComponent<LineRenderer>());
        //lineRenderer.widthMultiplier = 0f;
        thunderLinkLineRenderers.Add(lineRenderer);
        spear.SetThunderLinkVFX(vfx, lineRenderer);
        spears.Add(spear);
    }

    private void WaitAllSpearsSpawned(Spear spear)
    {
        allSpearsSet = true;
        //??= means "if ... == null, assigns this"
        thunderlinkRoutine ??= CoroutineManager.Instance.StartCoroutine(TriggerElectricLinks());
        moveRoutine ??= CoroutineManager.Instance.StartCoroutine(MoveThunderLink());
    }

    private IEnumerator TriggerElectricLinks()
    {
        Hero player = Utilities.Hero;
        yield return new WaitUntil(() => allSpearsSet == true);

        while (true)
        {
            ApplyDamages(player);
            yield return new WaitForSeconds(THUNDERLINK_WAIT_TIME);
        }
    }

    private void ApplyDamages(Hero player)
    {
        displayDamages = (int)(2f * Utilities.Hero.Stats.GetCoeff(Stat.ATK));

        List<Collider> alreadyAttacked = new List<Collider>();

        foreach (Spear spear in spears)
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.ThunderlinkSFX, spear.transform.position);

            Collider[] colliders = spear.SpearThrowCollider.BoxOverlap();

            if (colliders.Length > 0)
            {
                foreach (var collider in colliders)
                {
                    if (collider.gameObject.TryGetComponent<Entity>(out var entity) && entity is IDamageable && collider.gameObject != player.gameObject
                        && !alreadyAttacked.Contains(collider) && entity.IsInvincibleCount == 0)
                    {
                        FloatingTextGenerator.CreateDamageText(displayDamages, entity.transform.position);
                        (entity as IDamageable).ApplyDamage(displayDamages, Utilities.Hero, false);
                        entity.AddStatus(new Electricity(duration, chance), player);
                        alreadyAttacked.Add(collider);
                    }
                }
            }
        }
    }

    private IEnumerator MoveThunderLink()
    {
        while (true)
        {
            foreach (Spear spear in spears)
            {
                spear.ThunderLinkVFX.transform.position = Utilities.Player.transform.position + Vector3.up;
                spear.ThunderLinkLineRenderer.SetPosition(0, Utilities.Player.transform.position + Vector3.up);
                spear.ThunderLinkLineRenderer.SetPosition(1, new Vector3(spear.transform.position.x, Utilities.Player.transform.position.y + 1, spear.transform.position.z));
                spear.ScaleColliderToVector((spear.transform.position - Utilities.Player.transform.position));
                spear.SpearThrowCollider.transform.parent.LookAt(spear.transform.position);
            }
            yield return null;
        }
    }

}
