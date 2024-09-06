using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LifeBarHUD : MonoBehaviour
{
    [SerializeField] private Slider lifeJauge;
    [SerializeField] private TMP_Text lifeRatioText;
    private Hero player;

    private void Start()
    {
        player = FindObjectOfType<Hero>();
    }

    void Update()
    {
        lifeJauge.value = player.Stats.GetValue(Stat.HP);
        lifeJauge.maxValue = player.Stats.GetMaxValue(Stat.HP);
        lifeRatioText.text = lifeJauge.value.ToString() + " / " + player.Stats.GetMaxValue(Stat.HP);
    }
}
