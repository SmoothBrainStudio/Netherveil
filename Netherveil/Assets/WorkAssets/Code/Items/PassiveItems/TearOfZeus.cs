using UnityEngine;
using UnityEngine.VFX;

public class TearOfZeus : ItemEffect, IPassiveItem
{
    readonly int AOE_DAMAGES = 10;
    GameObject thunderstrikeCollider;
    GameObject thunderstrikeVFX;
    public void OnRetrieved()
    {
        thunderstrikeCollider = GameObject.Instantiate(GameResources.Get<GameObject>("ThunderstrikeCollide"));
        thunderstrikeVFX = GameObject.Instantiate(GameResources.Get<GameObject>("VFX_ThunderStrike"));
        thunderstrikeCollider.SetActive(false);

        Utilities.PlayerInput.OnEndDash += DropTear;
        Utilities.PlayerInput.OnEndDashAttack += DropTear;
    }

    public void OnRemove()
    {
        GameObject.Destroy(thunderstrikeCollider);
        GameObject.Destroy(thunderstrikeVFX);
        Utilities.PlayerInput.OnEndDash -= DropTear;
        Utilities.PlayerInput.OnEndDashAttack -= DropTear;
    }

    private void DropTear(Vector3 playerPos)
    {
        Hero hero = GameObject.FindWithTag("Player").GetComponent<Hero>();
        AudioManager.Instance.PlayThunders(playerPos);

        thunderstrikeVFX.transform.position = playerPos;
        thunderstrikeCollider.transform.position = playerPos;
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
    }
}
