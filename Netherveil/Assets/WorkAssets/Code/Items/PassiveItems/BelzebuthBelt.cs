using UnityEngine;
using UnityEngine.VFX;

public class BelzebuthBelt : ItemEffect, IPassiveItem
{
    const float coefValue = 1.33f;


#pragma warning disable CS0414 // Supprimer le warning dans unity
#pragma warning disable IDE0052 // Supprimer les membres privés non lus
    //used to display in description, dont delete it
    readonly float displayValue;
#pragma warning restore IDE0052
#pragma warning restore CS0414

    public BelzebuthBelt()
    {
        displayValue = coefValue - 1f;
    }

    public void OnRetrieved()
    {
        PlayerController player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        player.GetComponent<Hero>().Stats.SetValue(Stat.ATK_RANGE, player.GetComponent<Hero>().Stats.GetValue(Stat.ATK_RANGE) * coefValue);
        //divide by 5 because the vfx is based on plane scale size, and -0.2 is to make the arrow perfectly at the spear end pos
        player.SpearLaunchVFX.SetFloat("VFX Length", player.GetComponent<Hero>().Stats.GetValue(Stat.ATK_RANGE) / 5f - 0.2f);

        for (int i = 0; i < player.SpearAttacks.Count; ++i)
        {
            foreach (Collider collider in player.SpearAttacks[i].data)
            {
                Vector3 scale = collider.gameObject.transform.localScale;
                scale.z *= coefValue;

                //bad test but right now it's okay
                if (player.SpearAttacksVFX[i].HasFloat("VFX Size"))
                {
                    scale.x *= coefValue;
                }
                collider.gameObject.transform.localScale = scale;
                collider.gameObject.transform.localPosition *= coefValue;
            }
        }

        foreach (VisualEffect vfx in player.SpearAttacksVFX)
        {
            if (vfx.HasFloat("Length"))
            {
                vfx.SetFloat("Length", vfx.GetFloat("Length") * coefValue);
            }
            else if (vfx.HasFloat("VFX Size"))
            {
                vfx.SetFloat("VFX Size", vfx.GetFloat("VFX Size") * coefValue);
            }
        }
    }

    public void OnRemove()
    {
        PlayerController player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        player.GetComponent<Hero>().Stats.SetValue(Stat.ATK_RANGE, player.GetComponent<Hero>().Stats.GetValue(Stat.ATK_RANGE) / coefValue);
        //divide by 5 because the vfx is based on plane scale size, and -0.2 is to make the arrow perfectly at the spear end pos
        player.SpearLaunchVFX.SetFloat("VFX Length", player.GetComponent<Hero>().Stats.GetValue(Stat.ATK_RANGE) / 5f - 0.2f);

        for (int i = 0; i < player.SpearAttacks.Count; ++i)
        {
            foreach (Collider collider in player.SpearAttacks[i].data)
            {
                Vector3 scale = collider.gameObject.transform.localScale;
                scale.z /= coefValue;
                if (player.SpearAttacksVFX[i].HasFloat("VFX Size"))
                {
                    scale.x /= coefValue;
                }
                collider.gameObject.transform.localScale = scale;
                collider.gameObject.transform.localPosition /= coefValue;
            }
        }

        foreach (VisualEffect vfx in player.SpearAttacksVFX)
        {
            if (vfx.HasFloat("Length"))
            {
                vfx.SetFloat("Length", vfx.GetFloat("Length") / coefValue);
            }
            else if (vfx.HasFloat("VFX Size"))
            {
                vfx.SetFloat("VFX Size", vfx.GetFloat("VFX Size") / coefValue);
            }
        }
    }

}