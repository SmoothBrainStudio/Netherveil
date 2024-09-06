using System.Collections;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.VFX;

public class PoisonousPuddle : MonoBehaviour
{
    [Header("Gameobjects & Components")]
    [SerializeField] private GameObject graphics;
    [SerializeField] private GameObject VFXObject;
    [SerializeField] private VisualEffect VFX;
    [SerializeField] private Sound puddleSFX;
    [Header("Puddle Parameter")]
    [SerializeField] private bool activateOnAwake;
    [SerializeField] private float timerBeforeRemoved;
    [SerializeField] private float puddleDiameter;
    private float PuddleDiameter { get => puddleDiameter; }
    [SerializeField] private int impactDamage;
    [SerializeField] private LayerMask damageLayer;
    private bool isActive;
    private bool isMoving => throwRoutine != null;
    private Coroutine throwRoutine;
    private Coroutine impactRoutine;
    IAttacker launcher = null;

    private void Start()
    {
        VFX.SetFloat("StayOnFieldTime", 1.0f);
        VFX.SetFloat("PuddleRadius", puddleDiameter);

        if (activateOnAwake)
            Activate();
    }

    public void SetTimeStayingOnField(float _timeStaying)
    {
        timerBeforeRemoved = _timeStaying;
        VFX.SetFloat("StayOnFieldTime", timerBeforeRemoved);
    }

    public void SetImpactDamages(int damages)
    {
        impactDamage = damages;
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
        while (timer < timerToReach && transform.position.y > pos.y)
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

    public void Activate()
    {
        isActive = true;
        StartCoroutine(ActivateRoutine());
    }

    public void Impact()
    {
        if (impactRoutine == null)
            impactRoutine = StartCoroutine(ImpactRoutine());
    }

    private IEnumerator ActivateRoutine()
    {
        yield return new WaitForSeconds(timerBeforeRemoved);
        Impact();
    }

    private IEnumerator ImpactRoutine()
    {
        Physics.OverlapSphere(this.transform.position, PuddleDiameter / 2f - PuddleDiameter / 8f, damageLayer)
            .Select(entity => entity.GetComponent<IBlastable>())
            .Where(entity => entity != null)
            .ToList()
            .ForEach(currentEntity =>
            {
                currentEntity.ApplyDamage(impactDamage, launcher);
            });

        graphics.SetActive(false);
        puddleSFX.Play(this.transform.position);
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = new Color(1, 0, 0, 0.25f);

        Gizmos.DrawSphere(this.transform.position, PuddleDiameter / 2);

        Handles.color = Color.white;
        Handles.Label(transform.position + Vector3.up,
            $"Bomb" +
            $"\nActivate : {isActive}" +
            $"\nBefore explode : {timerBeforeRemoved}");
    }
#endif
}
