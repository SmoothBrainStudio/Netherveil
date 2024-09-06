using Map.Generation;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour, ISavable
{
    public static InGameManager current;

    public Transition publicFade;

    public int altarCountRegister = 0;
    public int seedIterationRegister = 0;
    public List<string> itemsPoolRegister = new List<string>();

    private ItemPool itemPool;
    public static ItemPool ItemPool { get { return current.itemPool; } }
    private void Awake()
    {
        current = this;
        itemPool = new();
        ItemAltar.altarCount = 0;

        LoadSave();
        SaveManager.onSave += Save;
    }

    public void RegisterGameValues()
    {
        seedIterationRegister = Seed.Iteration;
        altarCountRegister = ItemAltar.altarCount;
        //itemsPoolRegister = new List<string>(ItemPool.itemPool);
    }

    /// <summary>
    /// Load all the statics variables of the game
    /// </summary>
    public void LoadSave()
    {
        if (!SaveManager.saveData.hasData)
        {
            return;
        }

        ItemAltar.altarCount = SaveManager.saveData.Get<int>("altarCount");
        Seed.Iterate(SaveManager.saveData.Get<int>("seedIteration"));
        //itemPool.itemPool = new Stack<string>(SaveManager.saveData.itemsPool);

        altarCountRegister = ItemAltar.altarCount;
        seedIterationRegister = Seed.Iteration;
        //itemsPoolRegister = new List<string>(ItemPool.itemPool);
    }

    /// <summary>
    /// Save all the statics variables of the game
    /// </summary>
    public void Save(SaveData save)
    {
        save.Set("altarCount", altarCountRegister);
        save.Set("seedIteration", seedIterationRegister);
        //save.itemsPool = itemsPoolRegister;
    }
}