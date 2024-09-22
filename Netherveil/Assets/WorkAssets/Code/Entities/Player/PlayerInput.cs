using Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.VFX;

//Copyright 2024 Property of Olivier Maurin.All rights reserved.

[RequireComponent(typeof(PlayerController))]
public class PlayerInput : MonoBehaviour
{
    Hero hero;
    Animator animator;
    PlayerController controller;
    HitMaterialApply flashMaterial;
    CameraUtilities cameraUtilities;
    PlayerInteractions playerInteractions;
    UnityEngine.InputSystem.PlayerInput playerInputMap;

    public event Action<Vector3> OnThrowSpear;
    public event Action OnRetrieveSpear;
    public event Action OnStartDash;
    public event Action<Vector3> OnEndDash;
    public event Action<Vector3> OnEndDashAttack;

    Coroutine dashCoroutine = null;
    Coroutine chargedAttackCoroutine = null;

    public Vector2 Direction { get; private set; } = Vector2.zero;
    public Vector3 DashDir { get; private set; } = Vector3.zero;

    //dash values
    bool dashInCooldown = false;
    readonly float DASH_COOLDOWN_TIME = 0.3f;

    //attack values
    bool attackQueue = false;
    float chargedAttackTime = 0f;
    bool chargedAttackMax = false;
    readonly float CHARGED_ATTACK_MAX_TIME = 1f;
    readonly float CHARGED_ATTACK_CAN_RELEASE_TIME = 0.35f;
    float chargedAttackScaleSize = 0f;
    float chargedAttackVFXMaxSize = 0f;
    bool hasLaunchBlink = false;
    public readonly float EZREAL_ATTACK_THRESHOLD = 0.75f;
    public float ChargedAttackCoef { get; private set; } = 0f;
    public bool LaunchedChargedAttack { get; private set; } = false;
    readonly List<Collider> dashAttackAlreadyAttacked = new();
    bool applyVibrationsDashAttack = true;
    public bool LaunchedDashAttack { get; private set; } = false;
    private bool triggeredDashAttack = false;
    public bool TriggeredDash { get; private set; } = false;

    readonly float ZOOM_DEZOOM_TIME = 0.2f;

    //used to cancel queued attacks when pressing another button during attack sequence
    bool ForceReturnToMove = false;
    public bool IsSpawning { get; private set; } = false;

    [Header("Easing")]
    [SerializeField] EasingFunctions.EaseName easeUnzoom;
    [SerializeField] EasingFunctions.EaseName easeZoom;
    [SerializeField] EasingFunctions.EaseName easeShake;
    readonly List<System.Func<float, float>> easeFuncs = new();

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        playerInteractions = GetComponent<PlayerInteractions>();
        animator = GetComponentInChildren<Animator>();
        cameraUtilities = Camera.main.GetComponent<CameraUtilities>();
        flashMaterial = GetComponent<HitMaterialApply>();
    }

    private IEnumerator Start()
    {
        playerInputMap = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        EaseFuncsLoad();
        InputSetup();
        hero = GetComponent<Hero>();
        hero.OnChangeState += ResetForceReturnToMove;
        chargedAttackScaleSize = controller.ChargedAttack.gameObject.transform.localScale.x;
        chargedAttackVFXMaxSize = controller.ChargedAttackVFX.GetFloat("VFX Size");
        PauseMenu.OnPause += DisableGameplayInputs;
        PauseMenu.OnUnpause += EnableGameplayInputs;
        MapUtilities.onFinishStage += RetrieveSpear;
        IsSpawning = true;

        // Wait 2.8 seconds before move to Start cinematic
        DisableGameplayInputs();
        hero.State = (int)Hero.PlayerState.MOTIONLESS;
        yield return new WaitForSeconds(2.8f);
        IsSpawning = false;
        EnableGameplayInputs();
        hero.State = (int)Entity.EntityState.MOVE;
    }

    private void OnDestroy()
    {
        hero.OnChangeState -= ResetForceReturnToMove;
        PauseMenu.OnPause -= DisableGameplayInputs;
        PauseMenu.OnUnpause -= EnableGameplayInputs;
        MapUtilities.onFinishStage -= RetrieveSpear;
        InputActionMap kbMap = playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true);
        InputManagement(kbMap, unsubscribe: true);
        InputActionMap gamepadMap = playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true);
        InputManagement(gamepadMap, unsubscribe: true);
    }

    #region INPUTS

    private void ReadDirection(InputAction.CallbackContext ctx)
    {
        Direction = ctx.ReadValue<Vector2>().normalized;
    }

    private void ChargedAttack(InputAction.CallbackContext ctx)
    {
        if (!CanCastChargedAttack())
            return;

        animator.SetBool(controller.ChargedAttackCastingHash, true);
        hero.State = (int)Entity.EntityState.ATTACK;
        LaunchedChargedAttack = true;
    }

    private void ChargedAttackCanceled(InputAction.CallbackContext ctx)
    {
        animator.ResetTrigger(controller.ChargedAttackReleaseHash);
        animator.SetBool(controller.ChargedAttackCastingHash, false);

        if (!LaunchedChargedAttack)
            return;

        if (CanReleaseChargedAttack())
        {
            StopChargedAttackCoroutine();
            controller.ComboCount = 0;
            animator.SetTrigger(controller.ChargedAttackReleaseHash);
        }
        else
        {
            StopChargedAttackCoroutine();
            cameraUtilities.ChangeFov(cameraUtilities.defaultFOV, ZOOM_DEZOOM_TIME, easeFuncs[(int)easeZoom]);
            DeviceManager.Instance.ForceStopVibrations();
            hero.State = (int)Entity.EntityState.MOVE;
            controller.ResetValues();
        }
    }

    public void ChargedAttackRelease()
    {
        ChargedAttackCoef = chargedAttackMax ? 1 : chargedAttackTime / CHARGED_ATTACK_MAX_TIME;

        hasLaunchBlink = false;
        //set up collider and vfx size based on maintained time of charged attack
        Vector3 scale = controller.ChargedAttack.gameObject.transform.localScale;
        scale.x = ChargedAttackCoef * chargedAttackScaleSize * 0.9f;
        scale.z = ChargedAttackCoef * chargedAttackScaleSize * 0.9f;
        controller.ChargedAttack.gameObject.transform.localScale = scale;
        controller.ChargedAttackVFX.SetFloat("VFX Size", ChargedAttackCoef * chargedAttackVFXMaxSize);

        controller.AttackCollide(controller.ChargedAttack, false);
        chargedAttackMax = false;
        chargedAttackTime = 0f;

        //apply visual effects and controller vibrations
        DeviceManager.Instance.ApplyVibrations(0.8f * ChargedAttackCoef, 0.8f * ChargedAttackCoef, 0.25f);

        cameraUtilities.ShakeCamera(0.3f * ChargedAttackCoef, 0.25f, easeFuncs[(int)easeShake]);
        cameraUtilities.ChangeFov(cameraUtilities.defaultFOV, ZOOM_DEZOOM_TIME, easeFuncs[(int)easeZoom]);

        controller.ChargedAttackVFX.Reinit();
        controller.PlayVFXAtPlayerPos(controller.ChargedAttackVFX);
        AudioManager.Instance.PlaySound(controller.ChargedAttackReleaseSFX);
    }

    private IEnumerator ChargedAttackCoroutine()
    {
        DeviceManager.Instance.ApplyVibrationsInfinite(0f, 0.005f);

        while (chargedAttackTime < CHARGED_ATTACK_MAX_TIME)
        {
            chargedAttackTime += Time.deltaTime;
            if (CanReleaseChargedAttack() && !hasLaunchBlink && !flashMaterial.IsEnable)
            {
                flashMaterial.EnableMat();
                flashMaterial.SetAlpha(0, 1, 0.15f, () => flashMaterial.SetAlpha(1, 0, 0.15f, () => flashMaterial.DisableMat()));
                hasLaunchBlink = true;
            }

            if (DeviceManager.Instance.IsPlayingKB())
            {
                controller.MouseOrientation();
            }
            else
            {
                controller.JoystickOrientation();
            }
            yield return null;
        }

        DeviceManager.Instance.ApplyVibrationsInfinite(0.005f, 0.005f);
        chargedAttackMax = true;
        flashMaterial.EnableMat();
        flashMaterial.SetAlpha(0, 1, 0.15f, () => flashMaterial.SetAlpha(1, 0, 0.15f, () => flashMaterial.DisableMat()));
        FloatingTextGenerator.CreateActionText(transform.position, "Max!");
        AudioManager.Instance.PlaySound(controller.ChargedAttackMaxSFX);
        yield return null;

        while (true)
        {
            if (DeviceManager.Instance.IsPlayingKB())
            {
                controller.MouseOrientation();
            }
            else
            {
                controller.JoystickOrientation();
            }
            yield return null;
        }
    }

    private void Attack(InputAction.CallbackContext ctx)
    {
        if (CanAttack())
        {
            if (hero.State == (int)Entity.EntityState.ATTACK)
            {
                attackQueue = true;
            }
            else
            {
                animator.ResetTrigger(controller.BasicAttackHash);
                animator.SetTrigger(controller.BasicAttackHash);
            }
            hero.State = (int)Entity.EntityState.ATTACK;
        }
        else if (CanRetrieveSpear())
        {
            ThrowOrRetrieveSpear(ctx);
        }
        else if (CanDashAttack())
        {
            animator.ResetTrigger(controller.DashAttackHash);
            animator.SetTrigger(controller.DashAttackHash);
            triggeredDashAttack = true;
        }
    }

    private void Dash(InputAction.CallbackContext ctx)
    {
        if (!CanDash())
            return;

        ResetComboWhenMoving(ctx);

        if(!GameManager.Instance.dashWithMouse)
        {
            if (Direction != Vector2.zero)
            {
                DashDir = Direction.ToCameraOrientedVec3().normalized;
            }
            else
            {
                DashDir = transform.forward;
            }
            controller.OverridePlayerRotation(Quaternion.LookRotation(DashDir).eulerAngles.y, true);
        }
        else
        {
            controller.RotatePlayerToDeviceAndMargin(orientationErrorMargin: false);
            DashDir = transform.forward;
        }

        TriggeredDash = true;
        animator.ResetTrigger(controller.DashHash);
        animator.SetTrigger(controller.DashHash);
    }

    private void ActiveItemActivation(InputAction.CallbackContext ctx)
    {
        IActiveItem item = hero.Inventory.ActiveItem;
        if (item != null && (item as ItemEffect).CurrentEnergy >= item.Cooldown)
        {
            (item as ItemEffect).CurrentEnergy = 0;
            hero.Inventory.ActiveItem.Activate();
            StartCoroutine(item.WaitToUse());
        }
    }

    private void SpecialAbilityActivation(InputAction.CallbackContext ctx)
    {
        if (controller.SpecialAbility != null && controller.SpecialAbility.CurrentEnergy >= controller.SpecialAbility.Cooldown)
        {
            controller.SpecialAbility.CurrentEnergy = 0;
            controller.SpecialAbility.Activate();
            StartCoroutine(controller.SpecialAbility.WaitToUse());
        }
    }

    private void ThrowOrRetrieveSpear(InputAction.CallbackContext ctx)
    {
        // If spear is being thrown we can't recall this attack
        if (hero.State != (int)Entity.EntityState.MOVE || controller.Spear.IsThrowing)
            return;

        controller.RotatePlayerToDeviceAndMargin();

        if (!controller.Spear.IsThrown)
        {
            Vector3 posToReach = transform.position + transform.forward * hero.Stats.GetValue(Stat.ATK_RANGE);
            OnThrowSpear?.Invoke(posToReach);
            controller.Spear.Throw(posToReach);
            controller.PlayVFXAtPlayerPos(controller.SpearLaunchVFX);
        }
        else
        {
            OnRetrieveSpear?.Invoke();
            controller.Spear.Return();
        }
    }

    private void RetrieveSpear()
    {
        if(controller.Spear.IsThrown && !controller.Spear.IsThrowing)
        {
            OnRetrieveSpear?.Invoke();
            controller.Spear.Return();
        }
    }

    private void Interract(InputAction.CallbackContext ctx)
    {
        Vector2 playerPos = transform.position.ToCameraOrientedVec2();
        IInterractable closestInteractable = playerInteractions.InteractablesInRange.OrderBy(interactable =>
        {
            Vector2 itemPos = (interactable as MonoBehaviour).transform.position.ToCameraOrientedVec2();
            return Vector2.Distance(playerPos, itemPos);
        })
        .Where(interactable => interactable != null)
        .FirstOrDefault();

        closestInteractable?.Interract();
    }

    private void SkipDialogue(InputAction.CallbackContext ctx)
    {
        if (controller == null || !controller.DialogueTreeRunnerStarted)
            return;

        controller.DialogueTreeRunnerGet.UpdateDialogue();
    }

    private void ToggleItemDescription(InputAction.CallbackContext ctx)
    {
        HudHandler.current.ItemBar.Toggle();
        HudHandler.current.ItemBar.SelectPassiveItemInUI();
    }

    private void ResetComboWhenMoving(InputAction.CallbackContext ctx)
    {
        if (CanResetCombo())
        {
            ForceReturnToMove = true;
            controller.ResetValues();
        }
    }

    private void Pause(InputAction.CallbackContext ctx)
    {
        if (IsSpawning)
            return;

        HudHandler.current.PauseMenu.Toggle();
    }

    private void ToggleQuest(InputAction.CallbackContext ctx)
    {
        HudHandler.current.QuestHUD.Toggle();
    }

    private void ToggleMap(InputAction.CallbackContext ctx)
    {
        HudHandler.current.MapHUD.Toggle();
    }
    #endregion

    #region ANIMATION_EVENTS
    public void StartOfDashAnimation()
    {
        hero.IsInvincibleCount++;
        controller.DashVFX.Play();
        hero.State = (int)Hero.PlayerState.DASH;
        AudioManager.Instance.PlaySound(controller.DashSFX);
        OnStartDash?.Invoke();
    }

    public void EndOfDashAnimation()
    {
        if(!triggeredDashAttack)
        {
            RestartDashCoroutine();
            controller.DashVFX.Stop();
            hero.State = (int)Entity.EntityState.MOVE;
            controller.ResetValues();
            OnEndDash?.Invoke(transform.position);
        }
        hero.IsInvincibleCount--;
        triggeredDashAttack = false;
        TriggeredDash = false;
    }

    public void StartChargedAttackCasting()
    {
        cameraUtilities.ChangeFov(cameraUtilities.defaultFOV + 0.2f, ZOOM_DEZOOM_TIME, easeFuncs[(int)easeUnzoom]);
        chargedAttackCoroutine = StartCoroutine(ChargedAttackCoroutine());
    }

    public void EndOfChargedAttack()
    {
        hero.State = (int)Entity.EntityState.MOVE;
        controller.ResetValues();
    }

    public void StartOfBasicAttack()
    {
        hero.OnAttack?.Invoke();
        controller.AttackCollide(controller.SpearAttacks[controller.ComboCount].data, false);

        //stop all VFX of the combo attacks to prevent them to overlap each other
        foreach (VisualEffect vfx in controller.SpearAttacksVFX)
        {
            vfx.Reinit();
            vfx.Stop();
        }

        controller.PlayVFXAtPlayerPos(controller.SpearAttacksVFX[controller.ComboCount]);
        AudioManager.Instance.PlaySound(controller.AttacksSFX[controller.ComboCount]);

        if(CanLaunchEzrealAttack())
        {
            GameObject ezrealAttack = Instantiate(controller.ezrealAttackPrefab, transform.position + Vector3.up, transform.rotation);
        }
    }

    /// <summary>
    /// Triggers on attack animations to reset combo.
    /// </summary>
    public void EndOfBasicAttack()
    {
        animator.ResetTrigger(controller.BasicAttackHash);

        if (!attackQueue)
        {
            if (!LaunchedChargedAttack)
            {
                hero.State = (int)Entity.EntityState.MOVE;
                controller.ResetValues();
            }
            controller.ComboCount = 0;
        }
        else
        {
            animator.SetTrigger(controller.BasicAttackHash);
            hero.State = (int)Entity.EntityState.ATTACK;
            controller.ComboCount = (++controller.ComboCount) % controller.MAX_COMBO_COUNT;
        }

        attackQueue = false;

        foreach (NestedList<Collider> spearColliders in controller.SpearAttacks)
        {
            foreach (Collider spearCollider in spearColliders.data)
            {
                spearCollider.gameObject.SetActive(false);
            }
        }
    }

    public void StartOfDashAttackAnimation()
    {
        hero.State = (int)Hero.PlayerState.DASH;
        controller.RotatePlayerToDeviceAndMargin();
        DashDir = transform.forward;
        dashAttackAlreadyAttacked.Clear();
        applyVibrationsDashAttack = true;
        controller.PlayVFXAtPlayerPos(controller.DashAttackVFX);
        LaunchedDashAttack = true;
        AudioManager.Instance.PlaySound(controller.DashAttackSFX);
    }

    public void UpdateDashAttackAnimation()
    {
        hero.State = (int)Hero.PlayerState.DASH;
        controller.UpdateMovableVFXTransform(controller.DashAttackVFX);
        controller.ApplyCollide(controller.DashAttackCollider, dashAttackAlreadyAttacked, ref applyVibrationsDashAttack, false);
    }

    public void EndOfDashAttackAnimation()
    {
        hero.State = (int)Entity.EntityState.MOVE;
        RestartDashCoroutine();
        controller.DashVFX.Stop();
        LaunchedDashAttack = false;
        controller.ResetValues();
        OnEndDashAttack?.Invoke(transform.position);
    }

    #endregion

    #region INPUT_CONDITIONS

    private bool CanReleaseChargedAttack()
    {
        return (chargedAttackTime / CHARGED_ATTACK_MAX_TIME) > CHARGED_ATTACK_CAN_RELEASE_TIME;
    }

    private bool CanAttack()
    {
        return (hero.State == (int)Entity.EntityState.MOVE ||
            (hero.State == (int)Entity.EntityState.ATTACK && !attackQueue))
             && !controller.Spear.IsThrown && !ForceReturnToMove && !controller.DialogueTreeRunnerStarted;
    }

    private bool CanDashAttack()
    {
        if (Utilities.CharacterController == null || !Utilities.CharacterController.enabled)
            return false;

        Transform playerTr = Utilities.Player.transform;
        Vector3 capsuleBase = playerTr.position;
        Vector3 capsuleTop = new Vector3(capsuleBase.x, capsuleBase.y + Utilities.CharacterController.height, capsuleBase.z);
        return hero.State == (int)Hero.PlayerState.DASH && Physics.OverlapCapsule(capsuleBase, capsuleTop, Utilities.CharacterController.radius, LayerMask.GetMask("AvoidDashCollide")).Length == 0
        && !controller.Spear.IsThrown && !ForceReturnToMove && !controller.DialogueTreeRunnerStarted;
    }

    private bool CanCastChargedAttack()
    {
        return (hero.State == (int)Entity.EntityState.MOVE
            || hero.State == (int)Entity.EntityState.ATTACK)
            && !controller.Spear.IsThrown && !LaunchedChargedAttack && !controller.DialogueTreeRunnerStarted;
    }

    private bool CanRetrieveSpear()
    {
        return hero.State == (int)Entity.EntityState.MOVE && controller.Spear.IsThrown && !controller.DialogueTreeRunnerStarted;
    }

    private bool CanDash()
    {
        return (hero.State == (int)Entity.EntityState.MOVE
            || hero.State == (int)Entity.EntityState.ATTACK) && !dashInCooldown && !LaunchedChargedAttack && !controller.DialogueTreeRunnerStarted;
    }

    private bool CanResetCombo()
    {
        return (
                (DeviceManager.Instance.IsPlayingKB() && Keyboard.current.anyKey.isPressed) ||
                (!DeviceManager.Instance.IsPlayingKB() && Gamepad.current.allControls.Any(x => x is ButtonControl button && x.IsPressed() && !x.synthetic))
               )
               && !playerInputMap.currentActionMap["Basic Attack"].IsPressed() && hero.State == (int)Entity.EntityState.ATTACK && !LaunchedChargedAttack;
    }

    private bool CanLaunchEzrealAttack()
    {
        return hero.CurrentAlignmentStep <= -2 &&
            hero.Stats.GetValue(Stat.HP) / hero.Stats.GetMaxValue(Stat.HP) >= EZREAL_ATTACK_THRESHOLD &&
            controller.ComboCount == controller.MAX_COMBO_COUNT - 1;
    }
    #endregion

    #region MISCELLANEOUS
    private void EaseFuncsLoad()
    {
        easeFuncs.Add(EasingFunctions.EaseInBack);
        easeFuncs.Add(EasingFunctions.EaseInBounce);
        easeFuncs.Add(EasingFunctions.EaseInCirc);
        easeFuncs.Add(EasingFunctions.EaseInCubic);
        easeFuncs.Add(EasingFunctions.EaseInElastic);
        easeFuncs.Add(EasingFunctions.EaseInExpo);
        easeFuncs.Add(EasingFunctions.EaseInOutBack);
        easeFuncs.Add(EasingFunctions.EaseInOutBounce);
        easeFuncs.Add(EasingFunctions.EaseInOutCirc);
        easeFuncs.Add(EasingFunctions.EaseInOutCubic);
        easeFuncs.Add(EasingFunctions.EaseInOutElastic);
        easeFuncs.Add(EasingFunctions.EaseInOutExpo);
        easeFuncs.Add(EasingFunctions.EaseInOutQuad);
        easeFuncs.Add(EasingFunctions.EaseInOutQuart);
        easeFuncs.Add(EasingFunctions.EaseInOutQuint);
        easeFuncs.Add(EasingFunctions.EaseInOutSin);
        easeFuncs.Add(EasingFunctions.EaseInQuad);
        easeFuncs.Add(EasingFunctions.EaseInQuint);
        easeFuncs.Add(EasingFunctions.EaseInSin);
        easeFuncs.Add(EasingFunctions.EaseOutBack);
        easeFuncs.Add(EasingFunctions.EaseOutBounce);
        easeFuncs.Add(EasingFunctions.EaseOutCirc);
        easeFuncs.Add(EasingFunctions.EaseOutCubic);
        easeFuncs.Add(EasingFunctions.EaseOutElastic);
        easeFuncs.Add(EasingFunctions.EaseOutExpo);
        easeFuncs.Add(EasingFunctions.EaseOutQuad);
        easeFuncs.Add(EasingFunctions.EaseOutQuart);
        easeFuncs.Add(EasingFunctions.EaseOutQuint);
        easeFuncs.Add(EasingFunctions.EaseOutSin);
    }

    private void InputSetup()
    {
        InputActionMap kbMap = playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true);
        InputManagement(kbMap, unsubscribe: false);
        InputActionMap gamepadMap = playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true);
        InputManagement(gamepadMap, unsubscribe: false);
    }

    void InputManagement(InputActionMap map, bool unsubscribe)
    {
        if (unsubscribe)
        {
            map["Move"].performed -= ReadDirection;
            map["Move"].started -= ResetComboWhenMoving;
            map["Move"].canceled -= ReadDirection;
            map["Basic Attack"].performed -= Attack;
            map["Dash"].performed -= Dash;
            map["Interact"].performed -= Interract;
            map["Throw/Retrieve Spear"].performed -= ThrowOrRetrieveSpear;
            map["Charged Attack"].performed -= ChargedAttack;
            map["Charged Attack"].canceled -= ChargedAttackCanceled;
            map["Active Item"].performed -= ActiveItemActivation;
            map["Special Ability"].performed -= SpecialAbilityActivation;
            map["Map"].performed -= ToggleMap;
            map["Quest"].performed -= ToggleQuest;
            map["Pause"].started -= Pause;
            map["SkipDialogue"].started -= SkipDialogue;
            map["Inventory"].started -= ToggleItemDescription;
        }
        else
        {
            map["Move"].performed += ReadDirection;
            map["Move"].started += ResetComboWhenMoving;
            map["Move"].canceled += ReadDirection;
            map["Basic Attack"].performed += Attack;
            map["Dash"].performed += Dash;
            map["Interact"].performed += Interract;
            map["Throw/Retrieve Spear"].performed += ThrowOrRetrieveSpear;
            map["Charged Attack"].performed += ChargedAttack;
            map["Charged Attack"].canceled += ChargedAttackCanceled;
            map["Active Item"].performed += ActiveItemActivation;
            map["Special Ability"].performed += SpecialAbilityActivation;
            map["Map"].performed += ToggleMap;
            map["Quest"].performed += ToggleQuest;
            map["Pause"].started += Pause;
            map["SkipDialogue"].started += SkipDialogue;
            map["Inventory"].started += ToggleItemDescription;
        }
    }

    public void DisableGameplayInputs()
    {
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Move"].Disable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Basic Attack"].Disable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Dash"].Disable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Interact"].Disable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Throw/Retrieve Spear"].Disable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Charged Attack"].Disable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Active Item"].Disable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Special Ability"].Disable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Quest"].Disable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Map"].Disable();

        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Move"].Disable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Basic Attack"].Disable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Dash"].Disable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Interact"].Disable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Throw/Retrieve Spear"].Disable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Charged Attack"].Disable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Active Item"].Disable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Special Ability"].Disable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Quest"].Disable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Map"].Disable();
    }

    public void EnableGameplayInputs()
    {
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Move"].Enable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Basic Attack"].Enable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Dash"].Enable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Interact"].Enable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Throw/Retrieve Spear"].Enable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Charged Attack"].Enable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Active Item"].Enable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Special Ability"].Enable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Quest"].Enable();
        playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true)["Map"].Enable();

        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Move"].Enable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Basic Attack"].Enable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Dash"].Enable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Interact"].Enable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Throw/Retrieve Spear"].Enable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Charged Attack"].Enable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Active Item"].Enable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Special Ability"].Enable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Quest"].Enable();
        playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true)["Map"].Enable();

        if(DeviceManager.Instance.IsPlayingKB())
        {
            playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true).Enable();
            playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true).Disable();
        }
        else
        {
            playerInputMap.actions.FindActionMap("Keyboard", throwIfNotFound: true).Disable();
            playerInputMap.actions.FindActionMap("Gamepad", throwIfNotFound: true).Enable();
        }
    }

    private void ResetForceReturnToMove()
    {
        if (hero.State == (int)Entity.EntityState.MOVE)
        {
            ForceReturnToMove = false;
        }
    }
    public void ResetValuesInput()
    {
        StopChargedAttackCoroutine();
        attackQueue = false;
        LaunchedChargedAttack = false;
        chargedAttackMax = false;
        chargedAttackTime = 0f;
        LaunchedDashAttack = false;
        triggeredDashAttack = false;
        controller.DashVFX.Stop();
        animator.ResetTrigger(controller.DashAttackHash);
    }

    private void StopChargedAttackCoroutine()
    {
        if (chargedAttackCoroutine != null)
        {
            StopCoroutine(chargedAttackCoroutine);
            chargedAttackCoroutine = null;
        }
    }

    private IEnumerator DashCoroutine()
    {
        dashInCooldown = true;
        yield return new WaitForSeconds(DASH_COOLDOWN_TIME);
        dashInCooldown = false;
        dashCoroutine = null;
    }

    private void RestartDashCoroutine()
    {
        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
        }
        dashCoroutine = StartCoroutine(DashCoroutine());
    }
    #endregion
}