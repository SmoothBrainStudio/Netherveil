using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

public class ZiggoProjectile : MonoBehaviour
{
    Hero player;
    float effectCooldown = 0f;
    [SerializeField] float flaqueRadius = 0f;

    public float FlaqueRadius { get => flaqueRadius; }
    public VisualEffect PoisonPuddleVFX;
    public VisualEffect PoisonBallVFX;

    private void OnEnable()
    {
        player = Utilities.Hero;
    }

    public void ApplyPoison()
    {
        if (effectCooldown <= 0)
        {
            if (Vector3.SqrMagnitude(player.transform.position - transform.position) <= flaqueRadius * flaqueRadius)
            {
                Physics.OverlapSphere(transform.position, flaqueRadius, LayerMask.GetMask("Entity"))
                    .Select(entity => entity.GetComponent<Hero>())
                    .Where(entity => entity != null)
                    .ToList()
                    .ForEach(currentEntity =>
                    {
                        currentEntity.AddStatus(new Poison(1f, 1));
                    });

                effectCooldown = 0.5f;
            }
        }
        else effectCooldown -= Time.deltaTime;
    }

    public void ThrowToPos(Vector3 pos, float throwTime, float height)
    {
        StartCoroutine(ThrowToPosCoroutine(pos, throwTime, height));
    }

    private IEnumerator ThrowToPosCoroutine(Vector3 pos, float throwTime, float height)
    {
        float timer = 0;
        Vector3 basePos = this.transform.position;
        Vector3 position3D = Vector3.zero;
        float a = -4 * height, b = 4 * height;
        float c = this.transform.position.y;
        float timerToReach = MathsExtension.Resolve2ndDegree(a, b, c, pos.y).Max();
        while (timer < timerToReach)
        {
            yield return null;
            timer = timer > timerToReach ? timerToReach : timer;
            if (timer < 1.0f)
            {
                timer = timer > 1 ? 1 : timer;

                position3D = Vector3.Lerp(basePos, pos, timer);
            }
            position3D.y = MathsExtension.SquareFunction(a, b, c, timer);
            this.transform.position = position3D;
            timer += Time.deltaTime / throwTime;
        }
    }
}
