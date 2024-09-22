using Fountain;
using PostProcessingEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

//Copyright 2024 Property of Olivier Maurin.All rights reserved.


public class Hero : Entity, IDamageable, IAttacker, IBlastable, ISavable
{
    public enum PlayerState : int
    {
        DASH = EntityState.NB,
        KNOCKBACK,
        UPGRADING_STATS,
        MOTIONLESS
    }

    public delegate void OnBeforeApplyDamagesDelegate(ref int damages, IDamageable target);

    [SerializeField] List<NestedList<GameObject>> CorruptionArmorsToActivatePerStep;
    [SerializeField] List<NestedList<GameObject>> BenedictionArmorsToActivatePerStep;
    [SerializeField] List<NestedList<GameObject>> NormalArmorsToActivatePerStep;

    Animator animator;
    PlayerInput playerInput;
    PlayerController playerController;
    public Inventory Inventory { get; private set; } = new Inventory();
    public List<Status> StatusToApply => statusToApply;

    //alignment variables
    public static Color corruptionColor = new(0.62f, 0.34f, 0.76f, 1.0f);
    public static Color corruptionColor2 = new(0.4f, 0.08f, 0.53f);
    public static Color benedictionColor = Color.yellow;
    public static Color benedictionColor2 = new(0.89f, 0.75f, 0.14f);

    public int CurrentAlignmentStep { get => (int)(Stats.GetValue(Stat.CORRUPTION) / STEP_VALUE); }
    public const int STEP_VALUE = 25;
    public const int BENEDICTION_MAX = -4;
    public const int CORRUPTION_MAX = 4;
    public const int MAX_INDEX_ALIGNMENT_TAB = 3;
    bool canLaunchUpgrade = false;

    const float BENEDICTION_HP_STEP = 25f;
    const float BENEDICTION_HEAL_COEF_STEP = 1f;
    const float CORRUPTION_ATK_STEP = 2f;
    const float CORRUPTION_HP_STEP = 25f;
    const float CORRUPTION_LIFESTEAL_STEP = 0.03f;
    const float CORRUPTION_TAKE_DAMAGE_COEF_STEP = 0.25f;

    bool damnationVeilVideoShown = false;
    bool divineShieldVideoShown = false;
    bool damoclesSwordVideoShown = false;
    bool ezrealAttackVideoShown = false;

    //player events
    private event Action<IDamageable> onKill;
    private event IAttacker.AttackDelegate onAttack;
    private event IAttacker.HitDelegate onAttackHit;
    public event Action<int, IAttacker> OnTakeDamage;
    public event Action<IDamageable, IAttacker> OnBasicAttack;
    public event Action<IDamageable, IAttacker> OnDashAttack;
    public event Action<IDamageable, IAttacker> OnSpearAttack;
    public event Action<IDamageable, IAttacker> OnChargedAttack;
    public event Action<IDamageable, IAttacker> OnFinisherAttack;
    public event Action OnQuestObtained;
    public event Action OnQuestFinished;
    public event Action<ISpecialAbility> OnBenedictionMaxUpgrade;
    public event Action<ISpecialAbility> OnCorruptionMaxUpgrade;
    public event Action OnBenedictionMaxDrawback;
    public event Action OnCorruptionMaxDrawback;
    public event Action<int> OnHeal;
    public event OnBeforeApplyDamagesDelegate OnBeforeApplyDamages;
    public IAttacker.AttackDelegate OnAttack { get => onAttack; set => onAttack = value; }
    public IAttacker.HitDelegate OnAttackHit { get => onAttackHit; set => onAttackHit = value; }
    public Action<IDamageable> OnKill { get => onKill; set => onKill = value; }

    //quest variables
    Quest currentQuest = null;
    public Quest CurrentQuest
    {
        get => currentQuest;
        set
        {
            currentQuest = value;

            if (value == null)
            {
                if (HudHandler.current.QuestHUD.QuestEnable)
                {
                    HudHandler.current.QuestHUD.Toggle();
                }
                OnQuestFinished?.Invoke();
            }
            else
            {
                if (!HudHandler.current.QuestHUD.QuestEnable)
                {
                    HudHandler.current.QuestHUD.Toggle();
                }
                value.AcceptQuest();
                value.LateAcceptQuest();
                OnQuestObtained?.Invoke();
            }
        }
    }
    public bool DoneQuestQTThiStage { get; set; } = false;
    public bool DoneQuestQTApprenticeThisStage { get; set; } = false;

    //save & load variables
    const string saveFileName = "Hero";
    bool isLoading = false;

    //miscellaneous variables
    public bool ClearedTuto { get; set; } = false;
    public int LastDamagesSuffered { get; private set; } = 0;
    public bool CanHealFromConsumables { get; set; } = true;

    const float MAX_LIFESTEAL_HP_PERCENTAGE = 0.75f;
    float takeDamageCoeff = 1f;

    protected override void Start()
    {
        base.Start();
        animator = GetComponentInChildren<Animator>();
        playerInput = GetComponent<PlayerInput>();
        playerController = GetComponent<PlayerController>();
        GetComponent<Knockback>().onObstacleCollide += ApplyDamage;

        OnBasicAttack += ApplyDamoclesSwordEffect;
        OnKill += ApplyLifeSteal;
        FountainInteraction.onAddBenedictionCorruption += ChangeStatsBasedOnAlignment;
        Quest.OnQuestFinished += ChangeStatsBasedOnAlignment;
        Item.OnLateRetrieved += ChangeStatsBasedOnAlignment;
        stats.onStatChange += CheckIfLaunchUpgrade;
        //OnDeath += Inventory.RemoveAllItems;

        SaveManager.onSave += Save;
        LoadSave();
    }

    private void OnDestroy()
    {
        FountainInteraction.onAddBenedictionCorruption -= ChangeStatsBasedOnAlignment;
        Quest.OnQuestFinished -= ChangeStatsBasedOnAlignment;
        Item.OnLateRetrieved -= ChangeStatsBasedOnAlignment;
        stats.onStatChange -= CheckIfLaunchUpgrade;

        SaveManager.onSave -= Save;

        //Inventory.RemoveAllItems(Vector3.zero);
    }

    public void ApplyDamage(int _value, IAttacker attacker, bool notEffectDamages = true)
    {
        if (IsInvincibleCount > 0)
        {
            FloatingTextGenerator.CreateEffectDamageText(0, transform.position, Color.red);
            return;
        }

        if ((-_value) < 0 && stats.GetValue(Stat.HP) > 0) //just to be sure it really inflicts damages
        {
            if (notEffectDamages)
            {
                //only multiplied for damages inflicted outside of status effects
                _value = (int)(_value * takeDamageCoeff);

                DeviceManager.Instance.ForceStopVibrations();
                playerController.ResetValues();
                animator.ResetTrigger(playerController.ChargedAttackReleaseHash);
                animator.SetBool(playerController.ChargedAttackCastingHash, false);
                animator.ResetTrigger(playerController.BasicAttackHash);
                State = (int)Entity.EntityState.MOVE;

                AudioManager.Instance.PlaySound(playerController.HitSFX);
                FloatingTextGenerator.CreateEffectDamageText(_value, transform.position, Color.red);
                PostProcessingEffectManager.current.Play(Effect.Hit, false);
                playerController.HitVFX.Play();
            }

            LastDamagesSuffered = _value;
            Stats.DecreaseValue(Stat.HP, _value, false);

            OnTakeDamage?.Invoke(_value, attacker);
        }

        if (stats.GetValue(Stat.HP) <= 0 && State != (int)EntityState.DEAD)
        {
            Death();
            AudioManager.Instance.PlaySound(playerController.DeadSFX);
        }
    }
    public void Death()
    {
        animator.speed = 1;
        OnDeath?.Invoke(this.transform.position);
        GetComponent<Knockback>().StopAllCoroutines();
        Destroy(GetComponent<CharacterController>());
        playerController.OverridePlayerRotation(210f, true);
        animator.applyRootMotion = true;
        State = (int)EntityState.DEAD;
        animator.SetBool(playerController.IsKnockbackHash, false);
        animator.SetBool(playerController.IsDeadHash, true);
    }

    #region ATTACK
    public void Attack(IDamageable damageable, int additionalDamages = 0)
    {
        int damages = (int)stats.GetValueWithoutCoeff(Stat.ATK);

        if (playerInput.LaunchedChargedAttack)
        {
            ChargedAttackDamages(damageable, ref damages);
        }
        else if (playerController.ComboCount == playerController.MAX_COMBO_COUNT - 1)
        {
            FinisherDamages(damageable, ref damages);
        }
        else if (playerController.Spear.IsThrowing || playerController.Spear.IsThrown)
        {
            damages += playerController.SPEAR_DAMAGES;
            OnSpearAttack?.Invoke(damageable, this);
        }
        else if (playerInput.LaunchedDashAttack)
        {
            OnDashAttack?.Invoke(damageable, this);
        }
        else
        {
            damages += playerController.BASIC_ATTACK_DAMAGES;
            OnBasicAttack?.Invoke(damageable, this);
        }

        damages += additionalDamages;

        OnBeforeApplyDamages?.Invoke(ref damages, damageable);
        damages = (int)(damages * stats.GetCoeff(Stat.ATK));
        damageable.ApplyDamage(damages, this);

        OnAttackHit?.Invoke(damageable, this);
    }

    private void FinisherDamages(IDamageable damageable, ref int damages)
    {
        damages += playerController.FINISHER_DAMAGES;
        DeviceManager.Instance.ApplyVibrations(0.1f, 0f, 0.1f);
        ApplyKnockback(damageable, this);
        OnFinisherAttack?.Invoke(damageable, this);
    }

    private void ChargedAttackDamages(IDamageable damageable, ref int damages)
    {
        damages += (int)(playerController.CHARGED_ATTACK_DAMAGES * playerInput.ChargedAttackCoef);
        ApplyKnockback(damageable, this, stats.GetValue(Stat.KNOCKBACK_DISTANCE) * playerController.CHARGED_ATTACK_KNOCKBACK_COEFF * playerInput.ChargedAttackCoef,
            stats.GetValue(Stat.KNOCKBACK_COEFF) * playerController.CHARGED_ATTACK_KNOCKBACK_COEFF * playerInput.ChargedAttackCoef);
        OnChargedAttack?.Invoke(damageable, this);
    }


    /// <summary>
    /// applies the damocles sword effect when player is equal to or above 50% corruption
    /// </summary>
    /// <param name="damageable"></param>
    /// <param name="attacker"></param>
    private void ApplyDamoclesSwordEffect(IDamageable damageable, IAttacker attacker)
    {
        Mobs mob = damageable as Mobs;
        if (CurrentAlignmentStep >= 2 && mob != null)
        {
            mob.AddStatus(new DamoclesSword(3f, 0.3f), attacker);
        }
    }
    #endregion

    #region HEAL_MECHANICS
    private void ApplyLifeSteal(IDamageable damageable)
    {
        int lifeIncreasedValue = (int)(Stats.GetValue(Stat.LIFE_STEAL) * (damageable as Mobs).Stats.GetMaxValue(Stat.HP) * MAX_LIFESTEAL_HP_PERCENTAGE);
        lifeIncreasedValue = (int)(lifeIncreasedValue * Stats.GetValue(Stat.HEAL_COEFF));
        if (lifeIncreasedValue > 0 && (damageable as Mobs) != null && !(damageable as Mobs).IsSpawning)
        {
            HealPlayer(lifeIncreasedValue);
        }
    }

    public void HealConsumable(float healValue)
    {
        int realHealValue = (int)(healValue * Stats.GetValue(Stat.HEAL_COEFF));

        if (!CanHealFromConsumables)
        {
            realHealValue = 0;
        }

        HealPlayer(realHealValue);
    }

    public void HealPlayer(int realHealValue)
    {
        if (realHealValue > 0)
        {
            AudioManager.Instance.PlaySound(playerController.HealSFX, transform.position);
            Stats.IncreaseValue(Stat.HP, realHealValue, true);
        }

        FloatingTextGenerator.CreateHealText(realHealValue, transform.position);
        OnHeal?.Invoke(realHealValue);
    }
    #endregion

    #region ALIGNMENT_MANAGEMENT

    private void CheckIfLaunchUpgrade(Stat stat)
    {
        if (stat != Stat.CORRUPTION)
            return;

        canLaunchUpgrade = true;
    }

    public void ChangeStatsBasedOnAlignment()
    {
        int curStep = (int)(Stats.GetValue(Stat.CORRUPTION) / STEP_VALUE);
        int lastStep = (int)(Stats.GetLastValue(Stat.CORRUPTION) / STEP_VALUE);
        if (curStep == lastStep || !canLaunchUpgrade)
            return;

        if (!isLoading)
        {
            TriggerAnimAndVFX(curStep, lastStep);
        }
        ManageDrawbacks(lastStep);

        ReactivateDefaultArmor();

        if (curStep < 0)
        {
            ManageBenedictionUpgrade(curStep);
        }
        else if (curStep > 0)
        {
            ManageCorruptionUpgrade(curStep);
        }
    }

    private void TriggerAnimAndVFX(int curStep, int lastStep)
    {
        playerInput.DisableGameplayInputs();
        State = (int)Hero.PlayerState.UPGRADING_STATS;
        canLaunchUpgrade = false;

        bool corruptionUpgradeOnly = curStep > 0 && lastStep >= 0 && lastStep < curStep;
        bool benedictionUpgradeOnly = curStep < 0 && lastStep <= 0 && lastStep > curStep;

        bool hasbenedictionDrawbackNegativeToPositive = curStep >= 0 && lastStep < 0;
        bool hasbenedictionDrawbackNegativeOnly = curStep < 0 && lastStep <= 0 && lastStep < curStep;

        bool hascorruptionDrawbackPositiveToNegative = curStep <= 0 && lastStep > 0;
        bool hascorruptionDrawbackPositiveOnly = curStep > 0 && lastStep >= 0 && lastStep > curStep;

        if (corruptionUpgradeOnly)
        {
            ResetAligmentDependentAnimTriggers();
            playerController.corruptionUpgradeVFX.GetComponent<VFXStopper>().PlayVFX();
            AudioManager.Instance.PlaySound(playerController.CorruptionUpgradeSFX, transform.position);
            animator.SetTrigger(playerController.CorruptionUpgradeHash);
        }
        else if (benedictionUpgradeOnly)
        {
            ResetAligmentDependentAnimTriggers();
            playerController.benedictionUpgradeVFX.GetComponent<VFXStopper>().PlayVFX();
            AudioManager.Instance.PlaySound(playerController.BenedictionUpgradeSFX, transform.position);
            animator.SetTrigger(playerController.BenedictionUpgradeHash);
        }
        else if (hascorruptionDrawbackPositiveToNegative || hascorruptionDrawbackPositiveOnly)
        {
            playerController.DrawbackVFX.SetBool("Corruption", true);
            playerController.DrawbackVFX.Play();
            AudioManager.Instance.PlaySound(playerController.StepDowngradeSFX, transform.position);

            if (hascorruptionDrawbackPositiveToNegative && curStep < 0)
            {
                ResetAligmentDependentAnimTriggers();
                playerController.benedictionUpgradeVFX.GetComponent<VFXStopper>().PlayVFX();
                AudioManager.Instance.PlaySound(playerController.BenedictionUpgradeSFX, transform.position);
                animator.SetTrigger(playerController.BenedictionUpgradeHash);
            }
            else
            {
                playerInput.EnableGameplayInputs();
                State = (int)Entity.EntityState.MOVE;
            }
        }
        else if (hasbenedictionDrawbackNegativeToPositive || hasbenedictionDrawbackNegativeOnly)
        {
            playerController.DrawbackVFX.SetBool("Corruption", false);
            playerController.DrawbackVFX.Play();
            AudioManager.Instance.PlaySound(playerController.StepDowngradeSFX, transform.position);

            if (hasbenedictionDrawbackNegativeToPositive && curStep > 0)
            {
                ResetAligmentDependentAnimTriggers();
                playerController.corruptionUpgradeVFX.GetComponent<VFXStopper>().PlayVFX();
                AudioManager.Instance.PlaySound(playerController.CorruptionUpgradeSFX, transform.position);
                animator.SetTrigger(playerController.CorruptionUpgradeHash);
            }
            else
            {
                playerInput.EnableGameplayInputs();
                State = (int)Entity.EntityState.MOVE;
            }
        }
    }

    private void ResetAligmentDependentAnimTriggers()
    {
        animator.ResetTrigger(playerController.BenedictionUpgradeHash);
        animator.ResetTrigger(playerController.CorruptionUpgradeHash);
        animator.ResetTrigger(playerController.PouringBloodHash);
    }

    private void ManageDrawbacks(int lastStep)
    {
        for (int i = Mathf.Abs(lastStep); i > 0; i--)
        {
            if (lastStep < 0) // benediction drawbacks
            {
                if (i == Mathf.Abs(BENEDICTION_MAX))
                {
                    BenedictionMaxDrawback();
                }
                else
                {
                    BenedictionDrawback();
                }
            }
            else if (lastStep > 0) //corruption drawbacks
            {
                if (i == CORRUPTION_MAX)
                {
                    CorruptionMaxDrawback();
                }
                else
                {
                    CorruptionDrawback();
                }
            }
        }
    }

    private void ManageBenedictionUpgrade(int curStep)
    {
        for (int i = 0; i < Mathf.Abs(curStep); i++)
        {
            if (i == MAX_INDEX_ALIGNMENT_TAB)
            {
                BenedictionMaxUpgrade();
            }
            else
            {
                BenedictionUpgrade();
            }

            foreach (GameObject armorPiece in BenedictionArmorsToActivatePerStep[i].data)
            {
                armorPiece.SetActive(true);
            }
            foreach (GameObject armorPiece in NormalArmorsToActivatePerStep[i].data)
            {
                armorPiece.SetActive(false);
            }
        }
    }

    private void ManageCorruptionUpgrade(int curStep)
    {
        for (int i = 0; i < curStep; i++)
        {
            if (i == MAX_INDEX_ALIGNMENT_TAB)
            {
                CorruptionMaxUpgrade();
            }
            else
            {
                CorruptionUpgrade();
            }

            foreach (GameObject armorPiece in CorruptionArmorsToActivatePerStep[i].data)
            {
                armorPiece.SetActive(true);
            }
            foreach (GameObject armorPiece in NormalArmorsToActivatePerStep[i].data)
            {
                armorPiece.SetActive(false);
            }
        }
    }

    private void BenedictionMaxUpgrade()
    {
        playerController.SpecialAbility = new DivineShield();
        stats.IncreaseValue(Stat.HEAL_COEFF, BENEDICTION_HEAL_COEF_STEP, false);
        BenedictionUpgrade();
        OnBenedictionMaxUpgrade?.Invoke(playerController.SpecialAbility);
        if (!isLoading && !divineShieldVideoShown)
        {
            StartCoroutine(OpenSpecialAbilityTab(playerController.benedictionUpgradeVFX.GetComponent<VFXStopper>().Duration,
            "<color=yellow><b>Divine Shield</b></color>",
            $"On activation, creates a <color=#a52a2aff><b>shield</b></color> around you that <color=#a52a2aff><b>nullifies damages</b></color> for {playerController.DIVINE_SHIELD_DURATION} seconds.",
            "DivineShield",
            "SpecialAbilityBackgroundBenediction"));
            divineShieldVideoShown = true;
        }
    }

    private void BenedictionUpgrade()
    {
        if (CurrentAlignmentStep <= -2 && !isLoading && !ezrealAttackVideoShown)
        {
            StartCoroutine(OpenSpecialAbilityTab(playerController.corruptionUpgradeVFX.GetComponent<VFXStopper>().Duration,
            "<color=yellow><b>Light Arc</b></color>",
            $"When being over <color=#a52a2aff>{playerInput.EZREAL_ATTACK_THRESHOLD * 100f}% HP</color> and doing " +
            $"the <color=#a52a2aff>finisher</color> of your basic attack combo, you can throw " +
            $"a <color=#a52a2aff>light arc</color> that will do " +
            $"{playerController.EZREAL_ATTACK_DAMAGES} damages to all enemies touched during travel.",
            "EzrealAttack",
            "SpecialAbilityBackgroundBenediction"));
            ezrealAttackVideoShown = true;
        }
        Stats.IncreaseMaxValue(Stat.HP, BENEDICTION_HP_STEP);
        Stats.IncreaseValue(Stat.HP, BENEDICTION_HP_STEP);
    }

    private void BenedictionMaxDrawback()
    {
        playerController.SpecialAbility = null;
        stats.DecreaseValue(Stat.HEAL_COEFF, BENEDICTION_HEAL_COEF_STEP, false);
        BenedictionDrawback();
        OnBenedictionMaxDrawback?.Invoke();
    }

    private void BenedictionDrawback()
    {
        Stats.DecreaseMaxValue(Stat.HP, BENEDICTION_HP_STEP);
        Stats.DecreaseValue(Stat.HP, BENEDICTION_HP_STEP);
    }

    private void CorruptionMaxUpgrade()
    {
        CorruptionUpgrade();
        CanHealFromConsumables = false;
        playerController.SpecialAbility = new DamnationVeil();
        OnCorruptionMaxUpgrade?.Invoke(playerController.SpecialAbility);
        if (!isLoading && !damnationVeilVideoShown)
        {
            StartCoroutine(OpenSpecialAbilityTab(playerController.corruptionUpgradeVFX.GetComponent<VFXStopper>().Duration,
            "<color=#44197c><b>Damnation Veil</b></color>",
            "On activation, creates a <color=purple><b>damnation zone</b></color> that applies the <color=purple><b>damnation effect</b></color> " +
            "that <color=red>doubles the damages</color> received to all enemies touched by the zone.",
            "DamnationVeil",
            "SpecialAbilityBackgroundCoruption"));
            damnationVeilVideoShown = true;
        }
    }

    private void CorruptionUpgrade()
    {
        if (CurrentAlignmentStep >= 2 && !isLoading && !damoclesSwordVideoShown)
        {
            StartCoroutine(OpenSpecialAbilityTab(playerController.corruptionUpgradeVFX.GetComponent<VFXStopper>().Duration,
            "<color=#44197c><b>Fate's Blade</b></color>",
            $"When hitting an enemy, you have {playerController.DAMOCLES_SWORD_TRIGGER_PERCENT * 100f}% chance to apply <color=purple><b>Fate's Blade</b></color>, " +
            $"that will create a <color=purple>sword</color> on top of the target that will <color=purple>fall</color> on him after {playerController.DAMOCLES_SWORD_DURATION} seconds, " +
            $"dealing <color=purple>{playerController.DAMOCLES_SWORD_DAMAGES}</color> AOE Damages.",
            "DamoclesSword",
            "SpecialAbilityBackgroundCoruption"));
            damoclesSwordVideoShown = true;
        }
        takeDamageCoeff += CORRUPTION_TAKE_DAMAGE_COEF_STEP;
        Stats.IncreaseValue(Stat.LIFE_STEAL, CORRUPTION_LIFESTEAL_STEP);
        Stats.IncreaseValue(Stat.ATK, CORRUPTION_ATK_STEP);
        Stats.DecreaseMaxValue(Stat.HP, CORRUPTION_HP_STEP);
        Stats.DecreaseValue(Stat.HP, CORRUPTION_HP_STEP);
    }

    private void CorruptionMaxDrawback()
    {
        CorruptionDrawback();
        CanHealFromConsumables = true;
        playerController.SpecialAbility = null;
        OnCorruptionMaxDrawback?.Invoke();
    }

    private void CorruptionDrawback()
    {
        Stats.DecreaseValue(Stat.LIFE_STEAL, CORRUPTION_LIFESTEAL_STEP);
        takeDamageCoeff -= CORRUPTION_TAKE_DAMAGE_COEF_STEP;
        Stats.DecreaseValue(Stat.ATK, CORRUPTION_ATK_STEP);
        Stats.IncreaseMaxValue(Stat.HP, CORRUPTION_HP_STEP);
        Stats.IncreaseValue(Stat.HP, CORRUPTION_HP_STEP);
    }

    private void ReactivateDefaultArmor()
    {
        foreach (NestedList<GameObject> armorPiecesList in CorruptionArmorsToActivatePerStep)
        {
            foreach (GameObject armorPiece in armorPiecesList.data)
            {
                armorPiece.SetActive(false);
            }
        }

        foreach (NestedList<GameObject> armorPiecesList in BenedictionArmorsToActivatePerStep)
        {
            foreach (GameObject armorPiece in armorPiecesList.data)
            {
                armorPiece.SetActive(false);
            }
        }

        foreach (NestedList<GameObject> armorPiecesList in NormalArmorsToActivatePerStep)
        {
            foreach (GameObject armorPiece in armorPiecesList.data)
            {
                armorPiece.SetActive(true);
            }
        }
    }

    private IEnumerator OpenSpecialAbilityTab(float vfxDuration, string title, string description, string videoName, string backgroundName)
    {

        yield return new WaitForSeconds(vfxDuration);

        HudHandler.current.DescriptionTab.SetTab(title,
            description,
            GameResources.Get<VideoClip>(videoName),
            GameResources.Get<Sprite>(backgroundName));

        HudHandler.current.DescriptionTab.CloseTab();
        HudHandler.current.DescriptionTab.OpenTab();
    }

    public static void CallCorruptionBenedictionText(int value)
    {
        FloatingTextGenerator.CreateActionText(Utilities.Player.transform.position, $"+{Mathf.Abs(value)}" + (value < 0 ? " <sprite name=\"benediction\">" : " <sprite name=\"corruption\">"),
            value < 0 ? benedictionColor : corruptionColor);
    }

    public void DebugCallLaunchUpgrade()
    {
        canLaunchUpgrade = true;
    }
    #endregion

    #region SAVE_AND_LOAD

    public void Save(SaveData save)
    {
        save.Set("doneQuestQTThiStage", DoneQuestQTThiStage);
        save.Set("doneQuestQTApprenticeThisStage", DoneQuestQTApprenticeThisStage);
        save.Set("clearedTuto", ClearedTuto);

        save.Set("heroHp", stats.GetValue(Stat.HP));
        save.Set("heroCorruption", stats.GetValue(Stat.CORRUPTION));

        if (currentQuest != null)
        {
            currentQuest.Save(save);
        }
        else
        {
            save.Set("questId", string.Empty);
        }

        Inventory.Save(save);
    }

    public void LoadSave()
    {
        if (!SaveManager.saveData.hasData)
        {
            return;
        }

        isLoading = true;
        SaveData saveData = SaveManager.saveData;

        DoneQuestQTThiStage = saveData.Get<bool>("doneQuestQTThiStage");
        DoneQuestQTApprenticeThisStage = saveData.Get<bool>("doneQuestQTApprenticeThisStage");
        ClearedTuto = saveData.Get<bool>("clearedTuto");

        string questId = saveData.Get<string>("questId");
        if (questId.Any())
        {
            CurrentQuest = Quest.LoadClassWithSave(questId, saveData.Get<Quest.QuestDifficulty>("questDifficulty"), saveData.Get<QuestTalker.TalkerType>("talkerType"), saveData.Get<QuestTalker.TalkerGrade>("talkerGrade"));
            CurrentQuest.LoadSave();
        }

        Inventory.LoadSave();

        stats.SetValue(Stat.CORRUPTION, saveData.Get<float>("heroCorruption"));
        canLaunchUpgrade = true;
        ChangeStatsBasedOnAlignment();

        stats.SetValue(Stat.HP, saveData.Get<float>("heroHp"));

        isLoading = false;
    }

    #endregion
}