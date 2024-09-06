using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class CreateQuestWindow : EditorWindow
{
    QuestDatabase database;
    QuestData quest = new QuestData();
    public static void OpenWindow()
    {
        GetWindow<CreateQuestWindow>("CreateQuest");
    }

    private void OnEnable()
    {
        database = Resources.Load<QuestDatabase>("QuestDatabase");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Id name : ");
        quest.idName = EditorGUILayout.TextField(quest.idName);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Description : ", GUILayout.Height(30));
        quest.Description = EditorGUILayout.TextField(quest.Description, GUILayout.Height(30));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Type : ");
        quest.Type = (QuestData.QuestType)EditorGUILayout.EnumPopup(quest.Type);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUI.color = Color.green;
        if (GUILayout.Button("Save"))
        {
            database.datas.Add(quest);
            CreateScript();
            Close();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssetIfDirty(database);
        }
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();

    }

    void CreateScript()
    {
        string questName = quest.idName.GetPascalCase();
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
        StreamReader sr = new StreamReader(path + "/../../QuestSample.txt");
        StreamWriter sw = new StreamWriter(path);
        List<Type> typeList = new List<Type>()
        {
            typeof(Quest)
        };
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            List<string> splitLine = line.Split(' ').ToList();

            string finalLine = string.Empty;
            foreach (var word in splitLine)
            {
                string wordToAdd = word;
                switch (word)
                {
                    case "classSampleName":
                        wordToAdd = questName;
                        break;
                    case "functionSample":
                        wordToAdd = string.Empty;
                        for (int i = 0; i < typeList.Count; i++)
                        {
                            string methodToWrite = "    ";
                            MethodInfo[] infos = typeList[i].GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
                            
                            for (int j = 0; j < infos.Length; j++)
                            {
                                var method = infos[j];
                                if (method.Name.Split("_").Length < 2 && !method.IsStatic && (method.IsVirtual || method.IsAbstract))
                                {
                                    if (method.IsPublic) methodToWrite += "public ";
                                    else if (method.IsPrivate) methodToWrite += "private ";
                                    else methodToWrite += "protected ";

                                    if (method.IsVirtual || method.IsAbstract) methodToWrite += "override ";
                                    string typeString = method.ReturnType.ToString() == "System.Void" ? "void" : method.ReturnType.ToString();
                                    methodToWrite += typeString + " ";
                                    methodToWrite += method.Name + "(";

                                    methodToWrite += ")\n    {\n        throw new System.NotImplementedException();\n    }\n    ";
                                }
                                
                            }
                            sw.WriteLine(methodToWrite);
                        }
                        break;
                }
                finalLine += wordToAdd + ' ';
            }
            sw.WriteLine(finalLine);
        }
        sr.Close();
        sw.Close();

        AssetDatabase.Refresh();
    }
}
