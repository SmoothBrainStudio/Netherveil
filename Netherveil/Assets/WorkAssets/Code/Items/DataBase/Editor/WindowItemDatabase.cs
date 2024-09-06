using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

public class WindowItemDatabase : EditorWindow
{
    ItemDatabase database;
    List<ItemData> searchItems = new List<ItemData>();
    Vector2 scrollPos = Vector2.zero;
    string search = "";
    const int SizeArea = 100;

    [UnityEditor.MenuItem("Tools/Item/Database")]
    public static void OpenWindow()
    {
        GetWindow<WindowItemDatabase>("Item Database");
    }

    private void OnEnable()
    {
        database = Resources.Load<ItemDatabase>("ItemDatabase");
    }

    private void OnGUI()
    {
        SearchInDatabase();
        
        // Search Field
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search :", GUILayout.Width(60));
        search = GUILayout.TextField(search);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUI.color = Color.green;
        if (GUILayout.Button("Add Item"))
        {
            ItemCreatorEditor.OpenWindow();
        }
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();
        
        // Infos
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Id_Name", GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Rarity", GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Type", GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Description", GUILayout.Width(SizeArea*2), GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Icon", GUILayout.Width(SizeArea/3), GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Material", GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Mesh", GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();


        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        for (int i = 0; i < searchItems.Count; i++)
        {
            ItemData item = searchItems[i];
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            item.isInGame = EditorGUILayout.Toggle(item.isInGame, GUILayout.Width(40));
            
            EditorGUILayout.BeginVertical(GUILayout.Width(SizeArea));
            GUI.backgroundColor = new Color(0.882f, 0.58f, 1f);
            
            EditorGUILayout.LabelField(item.idName.SeparateAllCase(), GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Change id", GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true)))
            {
                IdNameWindow.OpenWindow(item.idName, database);
            }
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
            
            item.RarityTier = (ItemData.Rarity)EditorGUILayout.EnumPopup(item.RarityTier, GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
            item.Type = (ItemData.ItemType)EditorGUILayout.EnumPopup(item.Type, GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical();
            item.Description = EditorGUILayout.TextArea(item.Description, GUILayout.Height(100), GUILayout.Width(SizeArea*2), GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Edit description", GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true)))
            {
                TextEditor.OpenWindow(item);
            }
            EditorGUILayout.EndVertical();
            item.icon = (Texture)EditorGUILayout.ObjectField("", item.icon, typeof(Texture), false, GUILayout.Width(SizeArea/3), GUILayout.ExpandWidth(true));
            if(item.icon == null)
            {
                item.icon = Resources.Load<Texture>("EmojiOne");
            }
            item.mat = (Material)EditorGUILayout.ObjectField("", item.mat, typeof(Material), false, GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
            item.mesh = (Mesh)EditorGUILayout.ObjectField("", item.mesh, typeof(Mesh), false, GUILayout.Width(SizeArea), GUILayout.ExpandWidth(true));
            GUI.color = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(50)))
            {
                database.DeleteInDatabase(item);
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
        searchItems = database.datas.
           Where(x =>
           {
               for (int i = 0; i < search.Length; i++)
               {
                   if (x.idName.ToLower()[i] != search[i])
                   {
                       return false;
                   }
               }
               return true;
           }
           ).ToList();
        searchItems.Sort();
    }

}
