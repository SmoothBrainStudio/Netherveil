using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Copyright 2024 Property of Olivier Maurin.All rights reserved.
[CreateAssetMenu(menuName = "QuestDatabase")]
public class QuestDatabase : ScriptableObject
{
    //[HideInInspector]
    public List<QuestData> datas = new();

    public QuestData GetQuest(string name)
    {
        return datas.Where(x => x.idName == name).FirstOrDefault();
    }
}
