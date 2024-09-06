using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class SpearStrike : ItemEffect , IPassiveItem 
{
    readonly int AOE_DAMAGES = 10;

    public void OnRetrieved() 
    {
        Spear.OnPlacedInWorld += Thunderstrike;
    }

    public void OnRemove() 
    {
        Spear.OnPlacedInWorld -= Thunderstrike;
    } 
 
    private void Thunderstrike(Spear spear)
    {
        Hero hero = GameObject.FindWithTag("Player").GetComponent<Hero>();
        GameObject thunderstrikeCollider = GameObject.Instantiate(GameResources.Get<GameObject>("ThunderstrikeCollide"));
        GameObject thunderstrikeVFX = GameObject.Instantiate(GameResources.Get<GameObject>("VFX_ThunderStrike"));
        AudioManager.Instance.PlayThunders(spear.transform.position);
        thunderstrikeCollider.SetActive(false);

        thunderstrikeVFX.transform.position = new Vector3(spear.transform.position.x, hero.transform.position.y, spear.transform.position.z);
        thunderstrikeCollider.transform.position = new Vector3(spear.transform.position.x, hero.transform.position.y, spear.transform.position.z);
        //thunderstrikeCollider.SetActive(true);

        thunderstrikeVFX.GetComponent<VisualEffect>().Play();

        Collider[] colliders = thunderstrikeCollider.GetComponent<CapsuleCollider>().CapsuleOverlap();

        if (colliders.Length > 0)
        {
            foreach (var collider in colliders)
            {
                if (collider.gameObject.TryGetComponent<IDamageable>(out var entity) && collider.gameObject != hero.gameObject)
                {
                    hero.Attack(entity, AOE_DAMAGES);
                }
            }
        }

        GameObject.Destroy(thunderstrikeCollider, 1f);
        GameObject.Destroy(thunderstrikeVFX, 1f);
    }
} 
