using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemCreatorEditor : EditorWindow
{
    private ObjectField iconField;
    private TextField idNameField;
    private EnumField rarityField;
    private EnumField typeField;
    private FloatField cooldownField;
    private ObjectField meshField;
    private ObjectField materialField;
    private TextField descriptionField;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private static ItemDatabase database;
    private ItemData item;
    private float activeCooldown = 0;

    [MenuItem("Tools/Item/Add Item")]
    public static void OpenWindow()
    {
        ItemCreatorEditor wnd = GetWindow<ItemCreatorEditor>();
        wnd.titleContent = new GUIContent("Item creator");
        wnd.CreateItem();
        database = Resources.Load<ItemDatabase>("ItemDatabase");
    }

    public void CreateItem()
    {
        item = new ItemData();
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        iconField = root.Q<ObjectField>("Icon-Field");
        idNameField = root.Q<TextField>("ID-Field");
        rarityField = root.Q<EnumField>("Rarity-Field");
        typeField = root.Q<EnumField>("Type-Field");
        cooldownField = root.Q<FloatField>("Cooldown-Field");
        meshField = root.Q<ObjectField>("Mesh-Field");
        materialField = root.Q<ObjectField>("Material-Field");
        descriptionField = root.Q<TextField>("Description-Field");


        iconField.RegisterValueChangedCallback(evt =>
        {
            item.icon = (Texture)evt.newValue;
        });

        idNameField.RegisterValueChangedCallback(evt =>
        {
            item.idName = evt.newValue;
        });

        rarityField.RegisterValueChangedCallback((EventCallback<ChangeEvent<Enum>>)(evt =>
        {
            ItemData.Rarity type = (ItemData.Rarity)evt.newValue;
            item.RarityTier = type;
        }));

        typeField.RegisterValueChangedCallback(evt =>
        {
            ItemData.ItemType type = (ItemData.ItemType)evt.newValue;
            bool isCooldownable = type == ItemData.ItemType.ACTIVE || type == ItemData.ItemType.PASSIVE_ACTIVE;
            cooldownField.style.display = isCooldownable ? DisplayStyle.Flex : DisplayStyle.None;

            item.Type = type;
        });

        cooldownField.RegisterValueChangedCallback(evt =>
        {
            activeCooldown = evt.newValue;
        });

        meshField.RegisterValueChangedCallback(evt =>
        {
            item.mesh = (Mesh)evt.newValue;
        });

        materialField.RegisterValueChangedCallback(evt =>
        {
            item.mat = (Material)evt.newValue;
        });

        descriptionField.RegisterValueChangedCallback(evt =>
        {
            item.Description = evt.newValue;
        });


        root.Q<Button>("Save-Button").clicked += () =>
        {
            database.datas.Add(item);
            CreateItemScript();
            Close();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssetIfDirty(database);
        };
    }

    void CreateItemScript()
    {
        string itemName = item.idName.GetPascalCase();
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
        string path = Application.dataPath + "/SampleSceneAssets/Code/Items/" + itemType + $"/{itemName}.cs";
        StreamReader sr = new StreamReader(path + "/../../ItemSample.txt");
        StreamWriter sw = new StreamWriter(path);
        List<Type> typeList = new List<Type>();
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
                        wordToAdd = itemName;
                        break;
                    case "sampleInterface":
                        switch (item.Type)
                        {
                            case ItemData.ItemType.PASSIVE:
                                wordToAdd = "IPassiveItem";
                                typeList.Add(typeof(IPassiveItem));
                                break;
                            case ItemData.ItemType.ACTIVE:
                                typeList.Add(typeof(IActiveItem));
                                wordToAdd = "IActiveItem";
                                break;
                            case ItemData.ItemType.PASSIVE_ACTIVE:
                                typeList.Add(typeof(IPassiveItem));
                                typeList.Add(typeof(IActiveItem));
                                wordToAdd = "IPassiveItem, IActiveItem";
                                break;
                        }
                        break;
                    case "functionSample":
                        wordToAdd = string.Empty;
                        for (int i = 0; i < typeList.Count; i++)
                        {
                            string methodToWrite = "    ";
                            for (int j = 0; j < typeList[i].GetMethods().Length; j++)
                            {
                                var method = typeList[i].GetMethods()[j];
                                if (!method.IsStatic)
                                {
                                    if (method.Name.Split("_")[0] == "get" || method.Name.Split("_")[0] == "set")
                                    {
                                        var name = method.Name.Split("_")[1];
                                        var returnType = method.ReturnType;
                                        methodToWrite += "public " + returnType + " " + name + " { ";
                                        do
                                        {
                                            methodToWrite += method.Name.Split("_")[0] + "; ";
                                            j++;
                                            method = typeList[i].GetMethods()[j];
                                        } while (method.Name.Split("_").Length > 1 && (method.Name.Split("_")[1] == name || method.Name.Split("_")[1] == name));
                                        j--;
                                        methodToWrite += "} = " + activeCooldown + ";\n\n    ";
                                    }
                                    else
                                    {
                                        if (method.IsPublic) methodToWrite += "public ";
                                        else if (method.IsPrivate) methodToWrite += "private ";
                                        else methodToWrite += "protected ";

                                        Type type = method.ReturnType;
                                        string typeString = type.ToString() == "System.Void" ? "void" : type.ToString();
                                        methodToWrite += typeString + " ";
                                        methodToWrite += method.Name + "(";
                                        methodToWrite += ")\n    {\n        throw new System.NotImplementedException();\n    }\n    ";
                                    }
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
