using UnityEngine;

public class DesactivateOnBossAndPlayerDeath : MonoBehaviour
{
    [SerializeField] private Entity boss;

    private void Desactive(Vector3 _)
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Utilities.Hero.OnDeath += Desactive;
        boss.OnDeath += Desactive;
    }

    private void OnDisable()
    {
        Utilities.Hero.OnDeath -= Desactive;
        boss.OnDeath -= Desactive;
    }
}
