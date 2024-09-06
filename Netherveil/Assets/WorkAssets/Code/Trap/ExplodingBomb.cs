using System.Collections;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.VFX;

public class ExplodingBomb : MonoBehaviour
{
    [Header("Gameobjects & Components")]
    [SerializeField] private GameObject graphics;
    [SerializeField] private GameObject VFXObject;
    [SerializeField] private VisualEffect VFX;
    [SerializeField] private Sound bombSFX;
    [SerializeField] private Sound bombMonsterSFX;
    public Type type;
    [Header("Bomb Parameter")]
    [SerializeField] private bool activateOnAwake;
    [SerializeField] private float timerBeforeExplode;
    [SerializeField] private float blastDiameter;
    public float BlastDiameter { get => blastDiameter; }
    [SerializeField] private int blastDamage;
    [SerializeField] private LayerMask damageLayer;
    //private bool isActive;
    //private bool isMoving => throwRoutine != null;
    private Coroutine throwRoutine;
    private Coroutine explosionRoutine;
    IAttacker launcher = null;

    bool damageToEnemy = false;

    public enum Type
    {
        ITEM,
        MONSTER
    }

    private void Start()
    {
        VFX.SetFloat("ExplosionTime", 1.0f);
        VFX.SetFloat("ExplosionRadius", blastDiameter);

        if (activateOnAwake)
            Activate();
    }

    public void SetTimeToExplode(float _timeToExplode)
    {
        timerBeforeExplode = _timeToExplode;
        VFX.SetFloat("TimeToExplode", timerBeforeExplode);
    }

    public void SetBlastDamages(int damages)
    {
        blastDamage = damages;
    }

    private IEnumerator ThrowToPosCoroutine(Vector3 pos, float throwTime)
    {
        VFXObject.transform.parent = null;
        VFXObject.transform.position = pos;
        VFX.Play();

        float timer = 0;
        Vector3 basePos = this.transform.position;
        Vector3 position3D = Vector3.zero;
        float a = -16, b = 16;
        float c = this.transform.position.y;
        float timerToReach = MathsExtension.Resolve2ndDegree(a, b, c, 0).Max();
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

    public void ThrowToPos(IAttacker attacker, Vector3 pos, float throwTime)
    {
        launcher = attacker;
        StartCoroutine(ThrowToPosCoroutine(pos, throwTime));
    }

    public void Activate(bool _damageToEnemy = false)
    {
        //isActive = true;
        damageToEnemy = _damageToEnemy;
        StartCoroutine(ActivateRoutine());
    }

    private IEnumerator ActivateRoutine()
    {
        yield return new WaitForSeconds(timerBeforeExplode);
        Explode();
    }

    public void Explode()
    {
        
        if (explosionRoutine == null)
            explosionRoutine = StartCoroutine(ExplodeRoutine());
    }

    private IEnumerator ExplodeRoutine()
    {
        Physics.OverlapSphere(this.transform.position, BlastDiameter / 2f - BlastDiameter / 8f, damageLayer)
            .Select(entity => entity.GetComponent<IDamageable>())
            .Where((entity) =>
            {
                if(damageToEnemy)
                {
                    return entity != null && (entity as Mobs);
                }
                else
                {
                    return entity != null && (entity as Hero);
                }
            }
            )
            .ToList()
            .ForEach(currentEntity =>
            {
                currentEntity.ApplyDamage(blastDamage, launcher);
            });

        graphics.SetActive(false);
        switch (type)
        {
            case Type.ITEM:
                bombSFX.Play(this.transform.position);
                break;
            case Type.MONSTER:
                bombMonsterSFX.Play(this.transform.position);
                break;
        }
        
        float timer = VFX.GetFloat("ExplosionTime");

        while (timer > 0f)
        {
            yield return null;
            timer -= Time.deltaTime;
        }

        Destroy(gameObject);
        Destroy(VFX.gameObject);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void ApplyDamage(int _value, bool isCrit = false, bool hasAnimation = true)
    {
        Activate();
    }

    public void Death()
    {
        throw new System.NotImplementedException();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        //Handles.color = new Color(1, 0, 0, 0.25f);

        //Gizmos.DrawSphere(this.transform.position, BlastDiameter / 2);

        //Handles.color = Color.white;
        //Handles.Label(transform.position + Vector3.up,
        //    $"Bomb" +
        //    $"\nActivate : {isActive}" +
        //    $"\nBefore explode : {timerBeforeExplode - Time.time + elapsedExplosionTime}");
    }
#endif
}
