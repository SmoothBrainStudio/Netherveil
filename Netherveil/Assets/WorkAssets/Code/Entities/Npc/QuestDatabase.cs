using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
