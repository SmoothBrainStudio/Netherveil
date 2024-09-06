using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ResearchItemWindow : EditorWindow
{
    static private bool FocusOnOpen;
    ItemDatabase database;
    string search;
    List<string> searchItem = new List<string>();
    Vector2 scrollPos = Vector2.zero;


    private void OnEnable()
    {
        database = Resources.Load<ItemDatabase>("ItemDatabase");
        search = string.Empty;
        FocusOnOpen = true;
    }

    private void OnGUI()
    {
        SearchItems();
        EditorGUILayout.BeginHorizontal();
        GUI.SetNextControlName("search");
        search = EditorGUILayout.TextField(search);
        EditorGUILayout.EndHorizontal();
        FocusSearch();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach (var item in searchItem)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(item))
            {
                ItemEditor.ChosenName = item;
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void SearchItems()
    {
        searchItem = database.datas.Select(x => x.idName).
            Where(x =>
            {
                for(int i = 0; i < search.Length; i++)
                {
                    if (x.ToLower()[i] != search[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            ).ToList();
        searchItem.Sort();
    }

    private void FocusSearch()
    {
        if(FocusOnOpen)
        {
            FocusOnOpen = false;
            GUI.FocusControl("search");
        }
        
    }
}