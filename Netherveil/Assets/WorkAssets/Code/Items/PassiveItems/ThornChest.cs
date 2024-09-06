using UnityEngine;

public class ThornChest : ItemEffect, IPassiveItem
{
    readonly float thornPourcentage = 0.15f;

    public void OnRetrieved()
    {
        Utilities.Hero.OnTakeDamage += ThornDamages;
    }

    public void OnRemove()
    {
        Utilities.Hero.OnTakeDamage -= ThornDamages;
    }

    private void ThornDamages(int damageValue, IAttacker attacker)
    {
        //check if attacker is different from null to prevent crash on trap damages
        if (attacker != null && attacker is IDamageable)
        {
            GameObject vfx = GameObject.Instantiate(GameResources.Get<GameObject>("VFX_Thorn"));
            vfx.transform.position = (attacker as MonoBehaviour).transform.position + new Vector3(0f, (attacker as MonoBehaviour).GetComponent<CapsuleCollider>().height, 0f);
            vfx.GetComponent<VFXStopper>().Duration = 0.3f;
            vfx.GetComponent<VFXStopper>().PlayVFX();
            (attacker as IDamageable).ApplyDamage((int)(damageValue * thornPourcentage), attacker);
            AudioManager.Instance.PlaySound(AudioManager.Instance.ThornChestSFX, Utilities.Player.transform.position);
        }
    }
}
