using UnityEngine;

public class ShockwaveBracelet : ItemEffect, IActiveItem
{
    public float Cooldown { get; set; } = 20f;
    public bool TimeBased { get; set; } = true;
    public readonly float displayValue;
    readonly int AOE_DAMAGES;

    public ShockwaveBracelet()
    {
        AOE_DAMAGES = (int)(Utilities.Hero.Stats.GetValueWithoutCoeff(Stat.ATK) * 3);
        displayValue = Cooldown;
    }

    public void Activate()
    {
        Hero hero = GameObject.FindWithTag("Player").GetComponent<Hero>();
        GameObject shockwaveCollider = GameObject.Instantiate(GameResources.Get<GameObject>("ShockwaveBraceletCollide"));
        GameObject shockwaveVFX = GameObject.Instantiate(GameResources.Get<GameObject>("VFX_ShockWaveTank"));
        shockwaveCollider.SetActive(false);

        shockwaveVFX.transform.position = hero.transform.position;
        shockwaveCollider.transform.position = hero.transform.position;
        //shockwaveCollider.SetActive(true);

        shockwaveVFX.GetComponent<VFXStopper>().PlayVFX();

        Collider[] colliders = shockwaveCollider.GetComponent<CapsuleCollider>().CapsuleOverlap();

        if (colliders.Length > 0)
        {
            foreach (var collider in colliders)
            {
                if (collider.gameObject.TryGetComponent<IDamageable>(out var entity) && collider.gameObject != hero.gameObject)
                {
                    hero.ApplyKnockback(entity, hero);
                    hero.Attack(entity, AOE_DAMAGES);
                }
            }
        }

        GameObject.Destroy(shockwaveCollider, 3f);
        GameObject.Destroy(shockwaveVFX, 3f);
    }

    public void OnRetrieved()
    {

    }

    public void OnRemove()
    {

    }
}
