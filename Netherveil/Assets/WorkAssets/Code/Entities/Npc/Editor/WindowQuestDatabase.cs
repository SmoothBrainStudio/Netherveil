using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WindowQuestDatabase : EditorWindow
{
    QuestDatabase database;
    List<QuestData> searchQuests = new List<QuestData>();
    Vector2 scrollPos = Vector2.zero;
    string search = "";
    const int SizeArea = 25;

    [UnityEditor.MenuItem("Tools/QuestDatabase")]
    public static void OpenWindow()
    {
        GetWindow<WindowQuestDatabase>("Quest Database");
    }

    private void OnEnable()
    {
        database = Resources.Load<QuestDatabase>("QuestDatabase");
    }

    private void OnGUI()
    {
        SearchInDatabase();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        // Search Field
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search :", GUILayout.Width(60));
        search = GUILayout.TextField(search);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUI.color = Color.green;
        if (GUILayout.Button("Add Quest"))
        {
            GetWindow<CreateQuestWindow>("Create Quest");
        }
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();
        // Infos
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Id_Name", GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Type", GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("CorruptionModifierValue", GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("HasDifferentGrades", GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("LimitedTime", GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Description", GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();

        
        
        for (int i = 0; i < searchQuests.Count; i++)
        {
            QuestData quest = searchQuests[i];

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(quest.idName.SeparateAllCase(), GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
            quest.Type = (QuestData.QuestType)EditorGUILayout.EnumPopup(quest.Type, GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
            quest.CorruptionModifierValue = EditorGUILayout.IntField(quest.CorruptionModifierValue, GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
            GUILayout.Space(150f);
            quest.HasDifferentGrades = EditorGUILayout.Toggle(quest.HasDifferentGrades, GUILayout.Width(0),GUILayout.ExpandWidth(true));
            quest.LimitedTime = EditorGUILayout.Toggle(quest.LimitedTime, GUILayout.Width(0),GUILayout.ExpandWidth(true));
            quest.Description = EditorGUILayout.TextArea(quest.Description, GUILayout.Height(100), GUILayout.Width(SizeArea*2), GUILayout.ExpandWidth(true));
            GUI.color = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(50)))
            {
                DeleteInDatabase(quest);
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssetIfDirty(database);
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorUtility.SetDirty(database);
    }
    void SearchInDatabase()
    {
        searchQuests = database.datas.Where(quest => quest.idName.SeparateAllCase().ToLower().Contains(search.ToLower()) || quest.idName.SeparateAllCase().ToLower().Contains(search.ToLower())).ToList();
    }

    void DeleteInDatabase(QuestData quest)
    {
        database.datas.Remove(quest);
        string questName = quest.idName.GetCamelCase();
        string path = Application.dataPath + "/SampleSceneAssets/Code/Entities/Npc/Quest/";

        switch (quest.Type)
        {
            case QuestData.QuestType.RESTRICTIVE:
                path += "Restrictive";
                break;
            case QuestData.QuestType.EXPLORATION:
                path += "Exploration";
                break;
            case QuestData.QuestType.FIGHTING:
                path += "Fighting";
                break;
            case QuestData.QuestType.SURVIVABILITY:
                path += "Survivability";
                break;
            default:
                break;
        }

        path += $"/{questName}.cs";

        File.Delete(path);
        AssetDatabase.Refresh();
    }
}
