using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stats
{
    [SerializeField] List<StatInfo> stats = new();
    [SerializeField] private string name = "Default";
    public delegate void OnStatChangeDelegate(Stat stat);
    public OnStatChangeDelegate onStatChange;

    #region Getters
    // Get number of stats
    public int Size
    {
        get { return stats.Count; }
    }

    public string GetEntityName()
    {
        return name;
    }

    /// <summary>
    /// Return a list with all stats used. Exemple return {ATK, HP, CATCH_RANGE}
    /// </summary>
    public List<Stat> StatsName
    {
        get
        {
            List<Stat> list = new();
            foreach (StatInfo info in stats)
            {
                list.Add(info.stat);
            }

            return list;
        }
    }

    /// <summary>
    /// Get value of a stat, if there is a coeff, returns value * coeff
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public float GetValue(Stat info, bool isFinalResultClampedToMax = true)
    {
        foreach (StatInfo stat in stats)
        {
            if (stat.stat == info)
            {
                float coeff = stat.hasCoeff ? stat.coeff : 1;
                if(isFinalResultClampedToMax)
                {
                    if(stat.hasMaxStat)
                    {
                        return Mathf.Min(stat.value * coeff, stat.maxValue);
                    }
                    else
                    {
                        return stat.value * coeff;
                    }
                }
                return stat.value * coeff;
            }
        }

        Debug.LogWarning($"Can't find {info} in {name}");
        return -1.0f;
    }

    public float GetValueClampedWithCoeff(Stat info)
    {
        return Mathf.Min(GetValue(info), GetMaxValue(info));
    }
    /// <summary>
    /// Returns straight value
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public float GetValueWithoutCoeff(Stat info)
    {
        foreach (StatInfo stat in stats)
        {
            if (stat.stat == info)
            {
                return stat.value;
            }
        }

        Debug.LogWarning($"Can't find {info} in {name}");
        return -1.0f;
    }

    public float GetOverload(Stat info)
    {
        foreach (StatInfo stat in stats)
        {
            if (stat.stat == info)
            {
                return stat.overload;
            }
        }
        Debug.LogWarning($"Can't find {info} in {name}");
        return -1.0f;
    }

    public float GetUnderload(Stat info)
    {
        foreach (StatInfo stat in stats)
        {
            if (stat.stat == info)
            {
                return stat.underload;
            }
        }
        Debug.LogWarning($"Can't find {info} in {name}");
        return -1.0f;
    }
    public float GetLastValue(Stat info)
    {
        foreach (StatInfo stat in stats)
        {
            if (stat.stat == info)
            {
                return stat.lastValue;
            }
        }
        Debug.LogWarning($"Can't find {info} in {name}");
        return -1.0f;
    }
    

    /// <summary>
    /// Get the maximum value of a stat
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public float GetMaxValue(Stat info)
    {
        foreach (StatInfo stat in stats)
        {
            if (stat.stat == info)
            {
                return stat.maxValue;
            }
        }

        Debug.LogWarning($"Can't find {info} in {name}");
        return -1.0f;
    }

    /// <summary>
    /// Get the minimal value of a stat
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public float GetMinValue(Stat info)
    {
        foreach (StatInfo stat in stats)
        {
            if (stat.stat == info)
            {
                return stat.minValue;
            }
        }

        Debug.LogWarning($"Can't find {info} in {name}");
        return -1.0f;
    }

    /// <summary>
    /// If has coeff, returns coeff. Else, returns 1
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public float GetCoeff(Stat info)
    {
        foreach (StatInfo stat in stats)
        {
            if (stat.stat == info)
            {
                if (stat.hasCoeff)
                    return stat.coeff;
                else
                    return 1.0f;
            }
        }

        Debug.LogWarning($"Can't find {info} in {name}");
        return -1.0f;
    }

    public bool HasStat(Stat info)
    {
        foreach (StatInfo stat in stats)
        {
            if (stat.stat == info) return true;
        }
        return false;
    }
    #endregion

    #region ValueChange
    /// <summary>
    /// Increase value by increasingValue. If clampToMaxValue is false,the non-clamped value will be added in an overload member
    /// </summary>
    /// <param name="info"></param>
    /// <param name="increasingValue"></param>
    /// <param name="clampToMaxValue"></param>
    public void IncreaseValue(Stat info, float increasingValue, bool clampToMaxValue = true, bool takeUnderloadIntoAccount = false)
    {
        int index = stats.FindIndex(x => x.stat == info);
        if (index != -1)
        {
            float baseValue = stats[index].value;
            stats[index].lastValue = baseValue;

            if (clampToMaxValue && stats[index].hasMaxStat)
                IncreaseValueClamp(info, increasingValue);

            else if (!clampToMaxValue && stats[index].hasMaxStat)
                IncreaseValueOverload(info, increasingValue);

            else if (clampToMaxValue && !stats[index].hasMaxStat)
            {
                Debug.LogWarning($"Missing max value of {info} in {name}");
                return;
            }
            else
            {
                if (takeUnderloadIntoAccount && stats[index].underload > 0)
                {
                    float realIncrease = increasingValue - stats[index].underload;
                    stats[index].underload -= increasingValue;
                    if (realIncrease > 0.0f)
                    {
                        stats[index].underload = 0;
                        stats[index].value += realIncrease;
                    }
                }

                else
                    stats[index].value += increasingValue;
            }
            if (stats[index].value != baseValue) onStatChange?.Invoke(info);
        }
        else
        {
            Debug.LogWarning($"Can't find {info} in {name}");
        }
    }

    public void IncreaseMaxValue(Stat info, float increasingValue)
    {
        int index = stats.FindIndex(x => x.stat == info);

        if (index != -1)
        {
            if (stats[index].hasMaxStat)
                stats[index].maxValue += increasingValue;
            else
                Debug.LogWarning($"Missing max value of {info} in {name}");
        }
        else
        {
            Debug.LogWarning($"Can't find {info} in {name}");
        }
    }

    public void IncreaseMinValue(Stat info, float increasingValue)
    {
        int index = stats.FindIndex(x => x.stat == info);

        if (index != -1)
        {
            if (stats[index].hasMinStat)
                stats[index].minValue += increasingValue;

            else
                Debug.LogWarning($"Missing min value of {info} in {name}");
        }
        else
        {
            Debug.LogWarning($"Can't find {info} in {name}");
        }
    }

    public void DecreaseValue(Stat info, float decreasingValue, bool clampToMinValue = true, bool takeOverloadIntoAccount = false)
    {
        int index = stats.FindIndex(x => x.stat == info);
        if (index != -1)
        {
            float baseValue = stats[index].value;
            stats[index].lastValue = baseValue;

            if (clampToMinValue && stats[index].hasMinStat)
                DecreaseValueClamp(info, decreasingValue);

            else if (!clampToMinValue && stats[index].hasMinStat)
                DecreaseValueUnderload(info, decreasingValue);

            else if (clampToMinValue && !stats[index].hasMinStat)
            {
                Debug.LogWarning($"Missing min value of {info} in {name}");
                return;
            }


            else
            {
                if (takeOverloadIntoAccount && stats[index].overload > 0)
                {
                    float realIncrease = decreasingValue - stats[index].overload;
                    stats[index].overload -= decreasingValue;
                    if (realIncrease > 0.0f)
                    {
                        stats[index].overload = 0;
                        stats[index].value -= realIncrease;
                    }
                }
                else
                    stats[index].value -= decreasingValue;
            }
                

            if (baseValue != stats[index].value) onStatChange?.Invoke(info);
        }
        else
            Debug.LogWarning($"Can't find {info} in {name}");
    }

    public void DecreaseMaxValue(Stat info, float decreasingValue)
    {
        int index = stats.FindIndex(x => x.stat == info);

        if (index != -1)
        {
            if (stats[index].hasMaxStat)
                stats[index].maxValue -= decreasingValue;
            else
                Debug.LogWarning($"Missing max value of {info} in {name}");
        }
        else
        {
            Debug.LogWarning($"Can't find {info} in {name}");
        }
    }

    public void DecreaseMinValue(Stat info, float decreasingValue)
    {
        int index = stats.FindIndex(x => x.stat == info);

        if (index != -1)
        {
            if (stats[index].hasMinStat)
                stats[index].minValue -= decreasingValue;

            else
                Debug.LogWarning($"Missing min value of {info} in {name}");
        }
        else
        {
            Debug.LogWarning($"Can't find {info} in {name}");
        }
    }

    public void MultiplyValue(Stat info, float multiplyingValue, bool clampToMaxValue = true)
    {
        int index = stats.FindIndex(x => x.stat == info);
        if (index != -1)
        {
            float baseValue = stats[index].value;
            stats[index].lastValue = baseValue;
            if (clampToMaxValue && stats[index].hasMaxStat)
            {
                MultiplyValueClamp(info, multiplyingValue);
            }
            else if (clampToMaxValue && !stats[index].hasMaxStat)
            {
                Debug.LogWarning($"Missing min value of {info} in {name}");
            }
            else
            {
                stats[index].value *= multiplyingValue;
            }
            if (baseValue != stats[index].value) onStatChange?.Invoke(info);

        }
        else
            Debug.LogWarning($"Can't find {info} in {name}");
    }

    public void MultiplyMaxValue(Stat info, float multiplyingValue)
    {
        int index = stats.FindIndex(x => x.stat == info);

        if (index != -1)
        {
            if (stats[index].hasMaxStat)
                stats[index].maxValue *= multiplyingValue;
            else
                Debug.LogWarning($"Missing max value of {info} in {name}");
        }
        else
        {
            Debug.LogWarning($"Can't find {info} in {name}");
        }
    }

    public void MultiplyMinValue(Stat info, float multiplyingValue)
    {
        int index = stats.FindIndex(x => x.stat == info);

        if (index != -1)
        {
            if (stats[index].hasMinStat)
                stats[index].minValue *= multiplyingValue;

            else
                Debug.LogWarning($"Missing min value of {info} in {name}");
        }
        else
        {
            Debug.LogWarning($"Can't find {info} in {name}");
        }
    }

    public void DivideValue(Stat info, float dividingValue, bool clampToMinValue = true)
    {
        int index = stats.FindIndex(x => x.stat == info);
        if (index != -1)
        {
            float baseValue = stats[index].value;
            stats[index].lastValue = baseValue;
            if (clampToMinValue && stats[index].hasMinStat)
            {
                DivideValueClamp(info, dividingValue);
            }

            else if (clampToMinValue && !stats[index].hasMinStat)
            {
                Debug.LogWarning($"Missing min value of {info} in {name}");
            }

            else
            {
                stats[index].value /= dividingValue;
            }
            if (baseValue != stats[index].value) onStatChange?.Invoke(info);
        }
        else
            Debug.LogWarning($"Can't find {info} in {name}");
    }

    public void DivideMaxValue(Stat info, float dividingValue)
    {
        int index = stats.FindIndex(x => x.stat == info);

        if (index != -1)
        {
            if (stats[index].hasMaxStat)
                stats[index].maxValue /= dividingValue;
            else
                Debug.LogWarning($"Missing max value of {info} in {name}");
        }
        else
        {
            Debug.LogWarning($"Can't find {info} in {name}");
        }
    }

    public void DivideMinValue(Stat info, float dividingValue)
    {
        int index = stats.FindIndex(x => x.stat == info);

        if (index != -1)
        {
            if (stats[index].hasMinStat)
                stats[index].minValue /= dividingValue;

            else
                Debug.LogWarning($"Missing min value of {info} in {name}");
        }
        else
        {
            Debug.LogWarning($"Can't find {info} in {name}");
        }
    }

    public void SetValue(Stat info, float value)
    {
        int index = stats.FindIndex(x => x.stat == info);
        if (index != -1)
        {
            float baseValue = stats[index].value;
            stats[index].lastValue = baseValue;
            stats[index].value = value;
            if (baseValue != stats[index].value) onStatChange?.Invoke(info);
        }

        else
            Debug.LogWarning($"Can't find {info} in {name}");
    }

    public void SetMaxValue(Stat info, float value)
    {
        int index = stats.FindIndex(x => x.stat == info);
        if (index != -1)
        {
            if (stats[index].hasMaxStat)
                stats[index].maxValue = value;
            else
                Debug.Log($"Missing max value of {info} in {name}");
        }

        else
            Debug.LogWarning($"Can't find {info} in {name}");
    }

    public void SetMinValue(Stat info, float value)
    {
        int index = stats.FindIndex(x => x.stat == info);
        if (index != -1)
        {
            if (stats[index].hasMinStat)
                stats[index].minValue = value;
            else
                Debug.Log($"Missing min value of {info} in {name}");
        }

        else
            Debug.LogWarning($"Can't find {info} in {name}");
    }

    public void SetCoeffValue(Stat info, float value)
    {
        int index = stats.FindIndex(x => x.stat == info);
        if (index != -1)
        {
            if (stats[index].hasCoeff)
            {
                stats[index].coeff = value;
                onStatChange?.Invoke(info);
            }
            else
                Debug.Log($"Missing coeff value of {info} in {name}");
        }

        else
            Debug.LogWarning($"Can't find {info} in {name}");
    }

    public void IncreaseCoeffValue(Stat info, float increasingValue)
    {
        int index = stats.FindIndex(x => x.stat == info);

        if (index != -1)
        {
            if (stats[index].hasCoeff)
            {
                float baseValue = stats[index].coeff;
                stats[index].coeff += increasingValue;
                if (stats[index].value != baseValue) onStatChange?.Invoke(info);
            }
            else
            {
                Debug.LogWarning($"Can't find {info} coeff in {name}");
            }
        }
        else
        {
            Debug.LogWarning($"Can't find {info} in {name}");
        }
    }

    public void DecreaseCoeffValue(Stat info, float decreasingValue)
    {
        int index = stats.FindIndex(x => x.stat == info);

        if (index != -1)
        {
            if (stats[index].hasCoeff)
            {
                float baseValue = stats[index].coeff;
                stats[index].coeff -= decreasingValue;
                if (stats[index].value != baseValue) onStatChange?.Invoke(info);
            }
            else
            {
                Debug.LogWarning($"Can't find {info} coeff in {name}");
            }
        }
        else
        {
            Debug.LogWarning($"Can't find {info} in {name}");
        }
    }

    public void MultiplyCoeffValue(Stat info, float  multipliedValue)
    {
        int index = stats.FindIndex(x => x.stat == info);

        if (index != -1)
        {
            if (stats[index].hasCoeff)
            {
                float baseValue = stats[index].coeff;
                stats[index].coeff *= multipliedValue;
                if (stats[index].value != baseValue) onStatChange?.Invoke(info);
            }
            else
            {
                Debug.LogWarning($"Can't find {info} coeff in {name}");
            }
        }
        else
        {
            Debug.LogWarning($"Can't find {info} in {name}");
        }
    }

    public void DivideCoeffValue(Stat info, float divideValue)
    {
        int index = stats.FindIndex(x => x.stat == info);

        if (index != -1)
        {
            if (stats[index].hasCoeff)
            {
                float baseValue = stats[index].coeff;
                stats[index].coeff /= divideValue;
                if (stats[index].value != baseValue) onStatChange?.Invoke(info);
            }
            else
            {
                Debug.LogWarning($"Can't find {info} coeff in {name}");
            }
        }
        else
        {
            Debug.LogWarning($"Can't find {info} in {name}");
        }
    }
    #endregion

    #region ClampMaths
    private void IncreaseValueClamp(Stat statToIncrease, float increasingValue)
    {
        int indexIncrease = stats.FindIndex(x => x.stat == statToIncrease);

        if (indexIncrease != -1)
        {
            if (stats[indexIncrease].value + increasingValue > stats[indexIncrease].maxValue)
            {
                stats[indexIncrease].value = stats[indexIncrease].maxValue;
            }
            else
            {
                stats[indexIncrease].value += increasingValue;
            }
        }

        else if (indexIncrease == -1)
        {
            Debug.LogWarning($"Can't find {statToIncrease} in {name}");
        }
    }

    private void DecreaseValueClamp(Stat statToDecrease, float decreasingValue)
    {
        int indexDecrease = stats.FindIndex(x => x.stat == statToDecrease);

        if (indexDecrease != -1)
        {
            if (stats[indexDecrease].value - decreasingValue < stats[indexDecrease].minValue)
            {
                stats[indexDecrease].value = stats[indexDecrease].minValue;
            }
            else
            {
                stats[indexDecrease].value -= decreasingValue;
            }
        }

        else if (indexDecrease == -1)
        {
            Debug.LogWarning($"Can't find {statToDecrease} in {name}");
        }
    }

    private void MultiplyValueClamp(Stat statToIncrease, float increasingValue)
    {
        int indexIncrease = stats.FindIndex(x => x.stat == statToIncrease);

        if (indexIncrease != -1)
        {
            if (stats[indexIncrease].value * increasingValue > stats[indexIncrease].maxValue)
            {
                stats[indexIncrease].value = stats[indexIncrease].maxValue;
            }
            else
            {
                stats[indexIncrease].value *= increasingValue;
            }
        }

        else if (indexIncrease == -1)
        {
            Debug.LogWarning($"Can't find {statToIncrease} in {name}");
        }
    }

    private void DivideValueClamp(Stat statToDecrease, float decreasingValue)
    {
        int indexDecrease = stats.FindIndex(x => x.stat == statToDecrease);

        if (indexDecrease != -1)
        {
            if (stats[indexDecrease].value / decreasingValue < stats[indexDecrease].minValue)
            {
                stats[indexDecrease].value = stats[indexDecrease].minValue;
            }
            else
            {
                stats[indexDecrease].value /= decreasingValue;
            }
        }

        else if (indexDecrease == -1)
        {
            Debug.LogWarning($"Can't find {statToDecrease} in {name}");
        }
    }
    #endregion

    #region Overloard/Underloard
    private void IncreaseValueOverload(Stat statToIncrease, float increasingValue)
    {
        int indexIncrease = stats.FindIndex(x => x.stat == statToIncrease);

        if (indexIncrease != -1)
        {
            float realIncrease = increasingValue;
            // If there is underload
            if (stats[indexIncrease].underload > 0)
            {
                realIncrease -= stats[indexIncrease].underload;
                stats[indexIncrease].underload -= increasingValue;
            }
            // If realIncrease not null
            if (realIncrease > 0f)
            {
                // Security to make underload = 0 even if it should be by entering this if state
                stats[indexIncrease].underload = 0;
                // If increasing value makes it greater than maxValue
                if (stats[indexIncrease].value + realIncrease > stats[indexIncrease].maxValue)
                {
                    // Set overload increase value
                    float overloardValue = stats[indexIncrease].value + realIncrease - stats[indexIncrease].maxValue;
                    // Set value to the max value
                    stats[indexIncrease].value = stats[indexIncrease].maxValue;
                    stats[indexIncrease].overload += overloardValue;
                }
                else
                {
                    stats[indexIncrease].value += realIncrease;
                }
            }

        }

        else if (indexIncrease == -1)
        {
            Debug.LogWarning($"Can't find {statToIncrease} in {name}");
        }
    }

    private void DecreaseValueUnderload(Stat statToDecrease, float decreasingValue)
    {
        int indexDecrease = stats.FindIndex(x => x.stat == statToDecrease);

        if (indexDecrease != -1)
        {
            float realDecrease = decreasingValue;
            if (stats[indexDecrease].underload > 0)
            {
                realDecrease -= stats[indexDecrease].underload;
                stats[indexDecrease].underload -= realDecrease;
            }
            if (realDecrease > 0f)
            {
                stats[indexDecrease].underload = 0;
                if (stats[indexDecrease].value - realDecrease < stats[indexDecrease].minValue)
                {
                    float underloadValue = realDecrease - stats[indexDecrease].value + stats[indexDecrease].minValue;
                    stats[indexDecrease].value = stats[indexDecrease].minValue;
                    stats[indexDecrease].underload += underloadValue;
                }
                else
                {
                    stats[indexDecrease].value -= realDecrease;
                }
            }

        }
    }
    #endregion

}
