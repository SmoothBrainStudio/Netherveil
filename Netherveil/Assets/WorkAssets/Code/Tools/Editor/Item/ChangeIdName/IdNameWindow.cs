using Codice.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

public class IdNameWindow : EditorWindow
{
    private string idName;
    private string newId = string.Empty;
    private ItemDatabase itemDatabase;
    ItemData newData = new ItemData();
    ItemData oldData;
    public static void OpenWindow(string _idName, ItemDatabase database)
    {
        IdNameWindow wnd = GetWindow<IdNameWindow>();
        wnd.titleContent = new GUIContent("Item creator");
        wnd.idName = _idName;
        wnd.itemDatabase = database;


        wnd.oldData = database.GetItem(_idName);
        wnd.newData.Description = wnd.oldData.Description;
        wnd.newData.icon = wnd.oldData.icon;
        wnd.newData.mat = wnd.oldData.mat;
        wnd.newData.mesh = wnd.oldData.mesh;
        wnd.newData.Type = wnd.oldData.Type;
        wnd.newData.RarityTier = wnd.oldData.RarityTier;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Old id : ", idName);
        newId = EditorGUILayout.TextField("New id : ", newId);
        GUI.backgroundColor = Color.green;
        if(GUILayout.Button("Save"))
        {
            Debug.Log("Save");
            ChangeName();
            newData.idName = newId;
            itemDatabase.datas.Add(newData);
            itemDatabase.DeleteInDatabase(oldData);
            Close();
            AssetDatabase.Refresh();
        }
        GUI.backgroundColor = Color.white;
    }

    private void ChangeName()
    {
        newId = newId.GetPascalCase();
        ItemData item = itemDatabase.GetItem(idName);
        string itemType = string.Empty;

        switch (item.Type)
        {
            case ItemData.ItemType.PASSIVE:
                itemType = "PassiveItems";
                break;
            case ItemData.ItemType.ACTIVE:
                itemType = "ActiveItems";
                break;
            case ItemData.ItemType.PASSIVE_ACTIVE:
                itemType = "ActivePassiveItems";
                break;
            default:
                break;
        }
        string path = Application.dataPath + "/SampleSceneAssets/Code/Items/" + itemType + $"/{idName}.cs";
        string newPath = Application.dataPath + "/SampleSceneAssets/Code/Items/" + itemType + $"/{newId}.cs";

        StreamReader reader = new(path);
        StreamWriter writer = new(newPath);
       // string line;
        string completeclass = reader.ReadToEnd();
        completeclass = completeclass.Replace(idName, newId);


        writer.Write(completeclass);

        writer.Close();
        reader.Close();

        //optionnel
        writer.Dispose();
        reader.Dispose();

        GC.Collect();
    }
}
