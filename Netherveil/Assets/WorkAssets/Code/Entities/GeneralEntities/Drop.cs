using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[Serializable]
public class Drop
{
    private static readonly float radiusDropRandom = 2.0f;
    [SerializeField] List<DropInfo> dropList = new();
    public void DropLoot(Vector3 position)
    {
        foreach (DropInfo dropInfo in dropList)
        {
            if (dropInfo.isChanceShared)
            {
                DropChanceShared(position, dropInfo);
            }
            else
            {
                DropBasic(position, dropInfo);
            }
        }
    }

    private void DropBasic(Vector3 position, DropInfo dropInfo)
    {
        float baseChance = dropInfo.chance;
        DropMinimum(position, dropInfo);
        for (int i = dropInfo.minQuantity; i < dropInfo.maxQuantity; i++)
        {
            if (UnityEngine.Random.value <= dropInfo.chance)
            {
                GameObject go = GameObject.Instantiate(dropInfo.loot, position, Quaternion.identity);
                Vector3 pos3D;
                Vector2 pos = MathsExtension.GetRandomPointInCircle(new Vector2(go.transform.position.x, go.transform.position.z), radiusDropRandom);
                pos3D = new Vector3(pos.x, go.transform.position.y, pos.y);
                CoroutineManager.Instance.StartCoroutine(DropMovement(go, pos3D, 1f));
                dropInfo.chance -= dropInfo.decreasingValuePerDrop;
            }
        }
        dropInfo.chance = baseChance;
    }

    /// <summary>
    /// Drop a certain amount of drop once
    /// </summary>
    /// <param name="position"></param>
    /// <param name="dropInfo"></param>
    /// <returns>True if it has dropped, else false</returns>
    private bool DropChanceShared(Vector3 position, DropInfo dropInfo)
    {
        if (UnityEngine.Random.value <= dropInfo.chance)
        {
            for (int i = 0; i < dropInfo.maxQuantity; i++)
            {
                GameObject go = GameObject.Instantiate(dropInfo.loot, position, Quaternion.identity);
                Vector3 pos3D;
                Vector2 pos = MathsExtension.GetRandomPointInCircle(new Vector2(go.transform.position.x, go.transform.position.z), radiusDropRandom);
                pos3D = new Vector3(pos.x, go.transform.position.y, pos.y);
                CoroutineManager.Instance.StartCoroutine(DropMovement(go, pos3D, 1f));
            }
            return true;
        }
        return false;
    }

    private void DropMinimum(Vector3 position, DropInfo dropInfo)
    {
        for (int i = 0; i < dropInfo.minQuantity; i++)
        {
            GameObject go = GameObject.Instantiate(dropInfo.loot, position, Quaternion.identity);
            Vector3 pos3D;
            Vector2 pos = MathsExtension.GetRandomPointInCircle(new Vector2(go.transform.position.x, go.transform.position.z), radiusDropRandom);
            pos3D = new Vector3(pos.x, go.transform.position.y, pos.y);
            CoroutineManager.Instance.StartCoroutine(DropMovement(go, pos3D, 1f));
        }
        dropInfo.chance -= dropInfo.decreasingValuePerDrop;
    }
    private IEnumerator DropMovement(GameObject go, Vector3 pos, float throwTime)
    {
        if (go == null) yield break;
        float timer = 0;
        Vector3 basePos = go.transform.position;
        Vector3 position3D = Vector3.zero;
        float a = -16, b = 16;
        float c = go.transform.position.y;
        float timerToReach = MathsExtension.Resolve2ndDegree(a, b, c, 0).Max();
        while (timer < timerToReach)
        {
            yield return null;
            if (go == null) yield break;
            timer = timer > timerToReach ? timerToReach : timer;
            if (timer < 1.0f)
            {
                timer = timer > 1 ? 1 : timer;

                position3D = Vector3.Lerp(basePos, pos, timer);
            }
            position3D.y = MathsExtension.SquareFunction(a, b, c, timer);
            go.transform.position = position3D;
            timer += Time.deltaTime / throwTime;
        }
    }
}