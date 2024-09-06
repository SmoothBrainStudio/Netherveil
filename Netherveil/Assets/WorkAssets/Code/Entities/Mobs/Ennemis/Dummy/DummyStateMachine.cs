using System.Collections;
using UnityEngine;

public class DummyStateMachine : Mobs, IDummy
{
    [System.Serializable]
    private class DummySounds
    {
        public Sound hitSound;
    }

    enum Weakness
    {
        COMBO_FINISH,
        CHARGED_ATTACK,
        DISTANCE_ATTACK,
        DASH_ATTACK,
        TRAPS
    }

    private int hitHash;

    [SerializeField] private DummySounds dummySounds;

    [SerializeField] private Weakness weakness;
    [SerializeField] private FadeOutText textToFade;
    [SerializeField] private GameObject objectToDestroy;

    private bool triggerAttack = false;

    protected override void Start()
    {
        base.Start();
        hitHash = Animator.StringToHash("Hit");

        lifeBar.gameObject.SetActive(true);

        triggerAttack = false;

        Subscribe();
    }

    protected override void Update()
    {
        if (animator.speed == 0 || IsSpawning)
            return;

        base.Update();
    }

    private void TriggerAttackBool(IDamageable _damageable, IAttacker _attacker)
    {
        if (!gameObject.activeSelf)
            return;
        triggerAttack = true;
        StartCoroutine(DesactiveTheTrigger(.1f));
    }

    IEnumerator DesactiveTheTrigger(float _time)
    {
        yield return new WaitForSeconds(_time);
        triggerAttack = false;
    }

    public void ApplyDamage(int _value, IAttacker attacker, bool hasAnimation = true)
    {
        if (stats.GetValue(Stat.HP) <= 0 || IsInvincibleCount > 0)
        {
            triggerAttack = false;
            return;
        }

        if (!triggerAttack)
            _value = 0;

        if (hasAnimation)
        {
            FloatingTextGenerator.CreateDamageText(_value, transform.position);
            StartCoroutine(HitRoutine());
        }

        animator.ResetTrigger(hitHash);
        animator.SetTrigger(hitHash);

        dummySounds.hitSound.Play(transform.position, true);

        if (triggerAttack)
        {
            Stats.DecreaseValue(Stat.HP, _value, true);
        }
        lifeBar.ValueChanged(stats.GetValue(Stat.HP));

        triggerAttack = false;
        if (stats.GetValue(Stat.HP) <= 0)
        {
            Death();
        }
    }

    private void Subscribe()
    {
        switch (weakness)
        {
            case Weakness.COMBO_FINISH:
                Utilities.Hero.OnFinisherAttack += TriggerAttackBool;
                break;
            case Weakness.CHARGED_ATTACK:
                Utilities.Hero.OnChargedAttack += TriggerAttackBool;
                break;
            case Weakness.DISTANCE_ATTACK:
                Utilities.Hero.OnSpearAttack += TriggerAttackBool;
                break;
            case Weakness.DASH_ATTACK:
                Utilities.Hero.OnDashAttack += TriggerAttackBool;
                break;
            case Weakness.TRAPS:
                Projectile.OnProjectileHit += TriggerAttackBool;
                break;
        }
    }

    private void Unsubscribe()
    {
        switch (weakness)
        {
            case Weakness.COMBO_FINISH:
                Utilities.Hero.OnFinisherAttack -= TriggerAttackBool;
                break;
            case Weakness.CHARGED_ATTACK:
                Utilities.Hero.OnChargedAttack -= TriggerAttackBool;
                break;
            case Weakness.DISTANCE_ATTACK:
                Utilities.Hero.OnSpearAttack -= TriggerAttackBool;
                break;
            case Weakness.DASH_ATTACK:
                Utilities.Hero.OnDashAttack -= TriggerAttackBool;
                break;
            case Weakness.TRAPS:
                Projectile.OnProjectileHit -= TriggerAttackBool;
                break;
        }
    }

    public void Death()
    {
        AudioManager.Instance.PlaySound(AudioManager.Instance.DeathVFXSFX, transform.position);
        GameObject.Destroy(GameObject.Instantiate(GameResources.Get<GameObject>("VFX_Death"), transform.position, Quaternion.identity), 30f);
        animator.speed = 1;
        textToFade.fadeOut = true;
        Unsubscribe();
        Destroy(objectToDestroy);
        StopAllCoroutines();
    }
}
