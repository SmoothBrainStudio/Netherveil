using PostProcessingEffects;
using System;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingLowHP : MonoBehaviour
{
    private bool activePostProcessing;
    [SerializeField] private PostProcessingHitEffect hitEffect;

    [SerializeField] private Volume volume;
    public Coroutine routine = null;
    public bool effectIsPlaying = false;

    public Volume Volume => volume;

    void Start()
    {
        activePostProcessing = false;
    }

    private void OnEnable()
    {
        Hero hero = Utilities.Hero;
        if (hero != null)
        {
            hero.OnTakeDamage += Active;
            hero.OnHeal += Desactive;
            hero.OnDeath += DesactiveAtDeath;
            hero.Stats.onStatChange += DesactiveAtDeath;
        }
        else
        {
            Debug.LogWarning("PostProcessingLowHP can't subscribe to hero event : hero is null");
        }
    }

    private void OnDisable()
    {
        Hero hero = Utilities.Hero;
        if (hero != null)
        {
            hero.OnTakeDamage -= Active;
            hero.OnHeal -= Desactive;
            hero.OnDeath -= DesactiveAtDeath;
        }
    }

    void Update()
    {
        if (activePostProcessing)
        {
            if (!effectIsPlaying)
            {
                routine = StartCoroutine(hitEffect.PlayRoutine(this));
            }
        }
    }

    private void Active(int _damage, IAttacker _attackAutor)
    {
        if (Utilities.Hero.Stats.GetValue(Stat.HP) <= (Utilities.Hero.Stats.GetMaxValue(Stat.HP) / 4f))
        {
            activePostProcessing = true;
        }
    }

    private void Desactive(int _healingAmount)
    {
        if (Utilities.Hero.Stats.GetValue(Stat.HP) > (Utilities.Hero.Stats.GetMaxValue(Stat.HP) / 4f))
        {
            activePostProcessing = false;
        }
    }

    private void DesactiveAtDeath(Vector3 _)
    {
       activePostProcessing = false;
    }

    private void DesactiveAtDeath(Stat stat)
    {
        if (stat != Stat.HP)
        {
            return;
        }

        if (Utilities.Hero.Stats.GetValue(Stat.HP) > (Utilities.Hero.Stats.GetMaxValue(Stat.HP) / 4f))
        {
            activePostProcessing = false;
        }
    }
}
