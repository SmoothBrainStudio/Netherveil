using UnityEngine;

public class BootOfSwiftness : ItemEffect, IPassiveItem
{
    private readonly float speedStat = 1.5f;

    public void OnRemove()
    {
        Hero player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>();
        player.Stats.DecreaseValue(Stat.SPEED, speedStat, false);
    }

    public void OnRetrieved()
    {
        Hero player = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>();
        player.Stats.IncreaseValue(Stat.SPEED, speedStat, false);
        //RarityTier = Rarity.RARE;
        //Name = "<color=\"blue\">Boots of Swiftness";
        //Description = "Heightens player speed, granting swift agility to outpace challenges in the blink of an eye.\n" +
        //    "<color=\"green\">Speed: +" + speedStat.ToString();
    }
}
