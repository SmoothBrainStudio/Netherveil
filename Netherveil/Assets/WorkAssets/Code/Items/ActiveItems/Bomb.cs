using System.Collections;
using UnityEngine;

public class Bomb : ItemEffect, IActiveItem
{
    public float Cooldown { get; set; } = 10f;
    public bool TimeBased { get; set; } = true;

    public static bool bombIsThrow;
    private GameObject bombPf;
    readonly int damages = 15;
#pragma warning disable IDE0052 // Supprimer les membres privés non lus
    private readonly float displayValue;
#pragma warning restore IDE0052 // Supprimer les membres privés non lus

    public Bomb()
    {
        displayValue = Cooldown;
    }

    public void Activate()
    {
        GameObject player = Utilities.Player;
        player.GetComponent<PlayerController>().PlayLaunchBombAnim();
        player.GetComponent<PlayerController>().RotatePlayerToDeviceAndMargin();

        CoroutineManager.Instance.StartCoroutine(WaitLaunch(player));
    }

    private IEnumerator WaitLaunch(GameObject player)
    {
        Transform hand = player.GetComponent<PlayerController>().LeftHandTransform;
        Vector3 direction = player.transform.forward;
        var bomb = GameObject.Instantiate(bombPf, hand);
        var explodingBomb = bomb.GetComponent<ExplodingBomb>();

        yield return new WaitUntil(() => bombIsThrow);

        explodingBomb.gameObject.transform.parent = null;
        explodingBomb.gameObject.transform.rotation = Quaternion.identity;
        explodingBomb.ThrowToPos(Utilities.Hero, player.transform.position + direction * Utilities.Hero.Stats.GetValue(Stat.ATK_RANGE), 0.5f);
        explodingBomb.SetTimeToExplode(0.5f * 1.5f);

        explodingBomb.SetBlastDamages(damages);
        explodingBomb.Activate(true);
        bombIsThrow = false;
    }

    public void OnRetrieved()
    {
        bombPf = GameResources.Get<GameObject>("Bomb");
    }

    public void OnRemove()
    {
      
    }
}
