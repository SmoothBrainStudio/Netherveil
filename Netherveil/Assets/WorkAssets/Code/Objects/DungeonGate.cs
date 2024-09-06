using FMODUnity;
using Map;
using System.Threading.Tasks;
using UnityEngine;

public class DungeonGate : MonoBehaviour
{
    private Material material;
    private const float soundPlayDistance = 5f;
    [SerializeField] private BoxCollider boxCollider;

    private void Awake()
    {
        material = GetComponent<MeshRenderer>().material;

        // set value to default
        material.SetFloat("_Dissolve", 0f);
    }

    private void Start()
    {
        MapUtilities.onFirstEnter += Close;
        MapUtilities.onAllEnemiesDead += Open;
    }

    private void OnDestroy()
    {
        MapUtilities.onFirstEnter -= Close;
        MapUtilities.onAllEnemiesDead -= Open;
    }

    private void GateSound(EventReference _sound)
    {
        if (Vector2.Distance(transform.position.ToCameraOrientedVec2(), transform.position.ToCameraOrientedVec2()) <= soundPlayDistance)
        {
            AudioManager.Instance.PlaySound(_sound, transform.position);
        }
    }

    private void Open()
    {
        boxCollider.enabled = false;
        GateSound(AudioManager.Instance.GateOpenSFX);
        DisolveGate();
    }

    private void Close()
    {
        if (MapUtilities.currentRoomData.Enemies.Count <= 0)
        {
            return;
        }

        boxCollider.enabled = true;
        GateSound(AudioManager.Instance.GateCloseSFX);
        AppearGate();
    }

    async void AppearGate()
    {
        float disolveMat = material.GetFloat("_Dissolve");
        while ((disolveMat - 1f) < 0.05f)
        {
            disolveMat += Time.deltaTime;

            if (material == null)
            {
                return;
            }
            material.SetFloat("_Dissolve", disolveMat);

            await Task.Yield();
        }

        material.SetFloat("_Dissolve", 1f);
    }

    async void DisolveGate()
    {
        float disolveMat = material.GetFloat("_Dissolve");
        while (disolveMat > 0.05f)
        {
            disolveMat -= Time.deltaTime;

            if (material == null)
            {
                return;
            }
            material.SetFloat("_Dissolve", disolveMat);

            await Task.Yield();
        }

        material.SetFloat("_Dissolve", 0f);
    }
}
