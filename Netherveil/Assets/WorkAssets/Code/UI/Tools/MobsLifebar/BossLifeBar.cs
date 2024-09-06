using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossLifeBar : MonoBehaviour
{
    [Header("Gameobjects & Components")]
    [SerializeField] private CanvasGroup bossBar;
    [SerializeField] private Image lifeBarSlider;
    [SerializeField] private Image damageBarSlider;

    [Header("Parameters")]
    [SerializeField, Range(0.1f, 1.0f)] private float damageDisplayTime = 0.3f;

    private float maxValue;
    private float value;

    // Routines
    private Coroutine damageRoutine = null;

    // getter and setters
    public float MaxValue
    {
        get => maxValue;
        set
        {
            maxValue = value;
        }
    }
    public float Value => value;
    private float FactorValue => value / maxValue;

    private void Start()
    {
        value = maxValue;
    }

    private void OnDisable()
    {
        if (damageRoutine != null)
            StopCoroutine(damageRoutine);

        bossBar.alpha = 0.0f;
    }

    public void ValueChanged(float value)
    {
        if (gameObject.activeInHierarchy)
        {
            // update life bar
            this.value = value;
            lifeBarSlider.fillAmount = FactorValue;

            // stop damage update if is running
            if (damageRoutine != null)
                StopCoroutine(damageRoutine);

            // start damage update
            damageRoutine = StartCoroutine(DamageBarCoroutine(damageDisplayTime));
        }
    }

    public void ResetBars()
    {
        if (damageRoutine != null)
            StopCoroutine(damageRoutine);

        lifeBarSlider.fillAmount = 1;
        damageBarSlider.fillAmount = 1;
    }

    #region COROUTINE

    private IEnumerator DamageBarCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        float barDiff = damageBarSlider.fillAmount - lifeBarSlider.fillAmount;

        while (lifeBarSlider.fillAmount < damageBarSlider.fillAmount)
        {
            damageBarSlider.fillAmount -= Time.deltaTime * barDiff;

            yield return null;
        }

        damageRoutine = null;
    }

    #endregion
}
