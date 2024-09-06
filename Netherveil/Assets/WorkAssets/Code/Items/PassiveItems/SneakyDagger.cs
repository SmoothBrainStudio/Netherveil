using UnityEngine;

public class SneakyDagger : ItemEffect , IPassiveItem 
{
    readonly int attackValue = 15;
    public void OnRetrieved() 
    {
        Utilities.Hero.OnBeforeApplyDamages += ExtraSneakyDamages;
    }

    public void OnRemove() 
    {
        Utilities.Hero.OnBeforeApplyDamages -= ExtraSneakyDamages;
    }

    private void ExtraSneakyDamages(ref int damages, IDamageable target)
    {
        Transform player = GameObject.FindWithTag("Player").transform;
        Vector3 enemyToPlayerVec = (player.position - (target as MonoBehaviour).transform.position).normalized;

        //if the player is in the back of the enemy, and is in an angle behind of 2 * (180 - (180 * 0.85)), it inflicts more damages
        //*2 because it is mirrored based on opposite of forward
        if (Vector3.Dot(enemyToPlayerVec, (target as MonoBehaviour).transform.forward) < -0.85f)
        {
            damages += attackValue;
        }
    }
} 
