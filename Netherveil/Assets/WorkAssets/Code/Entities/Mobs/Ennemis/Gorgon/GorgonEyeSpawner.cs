using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GorgonEyeSpawner : MonoBehaviour
{
    [SerializeField] GameObject gorgonEye;
    [SerializeField] GorgonStateMachine gorgon;
    [SerializeField] Transform handPos;
    ExplodingBomb currentEye = null;
    bool spawnEyeCoroutineOn = false;

    private void Awake()
    {
        currentEye = GetComponentInChildren<ExplodingBomb>();
    }
    void Update()
    {
        if (gorgon.HasLaunchAnim)
        {
            if (!spawnEyeCoroutineOn)
            {
                currentEye.gameObject.transform.parent = null;
            }

        }
        else if (gorgon.HasRemovedHead)
        {
            if (!spawnEyeCoroutineOn)
            {
                currentEye.gameObject.transform.parent = handPos;
                StartCoroutine(SpawnEye());
            }
        }

    }

    private IEnumerator SpawnEye()
    {
        spawnEyeCoroutineOn = true;
        float timer = 0;
        GameObject go = Instantiate(gorgonEye, this.transform);
        currentEye = go.GetComponent<ExplodingBomb>();
        float wantedScale = 1f;
        go.transform.localScale = Vector3.zero;
        while (timer < 1.0f)
        {
            if (!gorgon.IsFreeze)
            {
                timer += Time.deltaTime / 2.0f;
                timer = timer > 1f ? 1f : timer;
                float curScale = Mathf.Lerp(0, wantedScale, timer);
                go.transform.localScale = new Vector3(curScale, curScale, curScale);
            }

            yield return null;
        }
        spawnEyeCoroutineOn = false;
        yield break;
    }
}
