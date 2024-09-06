using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class SaveData
{
    private readonly Dictionary<Type, Dictionary<string, object>> data = new()
    {
        { typeof(int), new Dictionary<string, object>() },
        { typeof(float), new Dictionary<string, object>() },
        { typeof(string), new Dictionary<string, object>() },
        { typeof(bool), new Dictionary<string, object>() },
    };

    public bool hasData;

    public void Reset()
    {
        foreach (var dic in data.Values)
        {
            dic.Clear();
        }
        hasData = false;
    }

    public void Set<T>(string key, T value)
    {
        Type type = typeof(T).IsEnum ? typeof(int) : typeof(T);

        object objectValue;
        if (typeof(T).IsEnum)
        {
            objectValue = (int)(object)value; // cast enum into an int
        }
        else
        {
            objectValue = value;
        }

        if (!data.ContainsKey(typeof(string)))
        {
            throw new Exception("Can't set " + key + " : " + type + " is not supported");
        }

        if (data[type].ContainsKey(key))
        {
            data[type][key] = objectValue;
        }
        else
        {
            data[type].Add(key, objectValue);
        }
    }

    public T Get<T>(string key)
    {
        Type type = typeof(T).IsEnum ? typeof(int) : typeof(T);
        if (!data.ContainsKey(type))
        {
            throw new Exception("Can't get " + key + " : " + type + " is not supported");
        }

        return (T)data[type][key];
    }

    public void Save(string filePath)
    {
        using var stream = File.Open(filePath, FileMode.Create);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, false);

        writer.Write(data.Count);
        foreach (Type type in data.Keys)
        {
            if (type != typeof(int) && type != typeof(float) && type != typeof(string) && type != typeof(bool))
            {
                Debug.LogWarning("Can't save " + type.Name + " : " + type + " is not supported");

                // need to write the count to 0 or the saveMng will load invalid datas
                writer.Write(type.AssemblyQualifiedName);
                writer.Write(0);

                continue;
            }

            writer.Write(type.AssemblyQualifiedName);
            writer.Write(data[type].Count);

            foreach (string objectKey in data[type].Keys)
            {
                object objectValue = data[type][objectKey];

                writer.Write(objectKey);
                if (type == typeof(int))
                {
                    writer.Write((int)objectValue);
                }
                else if (type == typeof(float))
                {
                    writer.Write((float)objectValue);
                }
                else if (type == typeof(string))
                {
                    writer.Write((string)objectValue);
                }
                else if (type == typeof(bool))
                {
                    writer.Write((bool)objectValue);
                }
            }
        }
    }

    public void Load(string filePath)
    {
        Reset();

        if (!File.Exists(filePath))
        {
            return;
        }

        using var stream = File.Open(filePath, FileMode.Open);
        using var reader = new BinaryReader(stream, Encoding.UTF8, false);

        int typeCount = reader.ReadInt32();
        for (int j = 0; j < typeCount; j++)
        {
            Type objectType = Type.GetType(reader.ReadString());

            int objectCount = reader.ReadInt32();
            for (int i = 0; i < objectCount; i++)
            {
                string objectKey = reader.ReadString();
                if (objectType == typeof(int))
                {
                    data[objectType].Add(objectKey, reader.ReadInt32());
                }
                else if (objectType == typeof(float))
                {
                    data[objectType].Add(objectKey, reader.ReadSingle());
                }
                else if (objectType == typeof(string))
                {
                    data[objectType].Add(objectKey, reader.ReadString());
                }
                else if (objectType == typeof(bool))
                {
                    data[objectType].Add(objectKey, reader.ReadBoolean());
                }
                else
                {
                    Debug.LogError(objectType);
                    throw new Exception("wtf");
                }
            }
        }

        hasData = true;
    }
}

static public class SaveManager
{
    public delegate void OnSave(SaveData saveData);
    static public event OnSave onSave;

    static public string CurrentSavePath { private set; get; } = string.Empty;
    static public SaveData saveData = new SaveData();

    static public string GetSavePath(int selectedSave)
    {
        return Application.persistentDataPath + "/Save/" + selectedSave.ToString() + ".s";
    }

    static public void EraseCurrentSave()
    {
        if (!CurrentSavePath.Any())
        {
            return;
        }

        try
        {
            File.Delete(CurrentSavePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Can't delete current save : " + ex.Message);
        }
    }

    static public void EraseSave(int selectedSave)
    {
        string selectedSavePath = GetSavePath(selectedSave);

        File.Delete(selectedSavePath);

        if (selectedSavePath == CurrentSavePath)
        {
            saveData.Reset();
        }
    }

    static public void SelectSave(int selectedSave)
    {
        CurrentSavePath = GetSavePath(selectedSave);

        if (!Directory.Exists(Application.persistentDataPath + "/Save"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Save");
        }

        Load();
    }

    static private void Load()
    {
        try
        {
            saveData.Load(CurrentSavePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Can't load file : " + e);

            File.Delete(CurrentSavePath);
        }
    }

    static public void Save()
    {
        if (!CurrentSavePath.Any())
        {
            return;
        }

        onSave?.Invoke(saveData);

        try
        {
            saveData.Save(CurrentSavePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Can't save : " + e);

            File.Delete(CurrentSavePath);
        }
    }
}
