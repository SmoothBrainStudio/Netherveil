using DialogueSystem.Runtime;
using Map;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using static QuestTalker;

//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public abstract class Quest : ISavable
{
    public enum QuestDifficulty
    {
        EASY,
        MEDIUM,
        HARD,
        NB
    }

    public QuestData Datas { get; private set; }
    public string progressText = string.Empty;
    static QuestDatabase database;
    public static event Action OnQuestUpdated;
    public static event Action OnQuestFinished;
    protected Hero player;
    protected QuestTalker.TalkerType talkerType;
    protected QuestTalker.TalkerGrade talkerGrade;
    protected QuestDifficulty difficulty;
    protected bool questLost = false;
    private int startFloor;

    private Coroutine timeManagerRoutine = null;
    protected float timeToFinishQuest;

    public float CurrentQuestTimer { get; protected set; }
    public int CorruptionModifierValue { get; protected set; } = 0;
    public QuestTalker.TalkerType TalkerType { get => talkerType; }
    public QuestTalker.TalkerGrade TalkerGrade { get => talkerGrade; }
    public QuestDifficulty Difficulty { get => difficulty; }

    public abstract bool IsQuestFinished();
    protected abstract void ResetQuestValues();

    public virtual void AcceptQuest()
    {
        AudioManager.Instance.PlaySound(AudioManager.Instance.QuestObtainedSFX);
        MapUtilities.onEarlyAllEnemiesDead += CheckQuestFinished;
        MapUtilities.onEarlyAllChestOpen += CheckQuestFinished;
        MapUtilities.onFirstEnter += CheckQuestFinished;
        Utilities.Hero.OnQuestObtained += CheckQuestFinished;

        startFloor = MapUtilities.Stage;
    }

    public void LateAcceptQuest()
    {
        if (Datas.LimitedTime)
        {
            timeManagerRoutine = CoroutineManager.Instance.StartCoroutine(TimeToFinishRoutine());
        }
    }

    protected void QuestFinished()
    {
        AudioManager.Instance.PlaySound(AudioManager.Instance.QuestFinishedSFX);
        player.CurrentQuest = null;

        if (startFloor == MapUtilities.Stage)
        {
            if (talkerGrade == QuestTalker.TalkerGrade.BOSS)
            {
                Utilities.Hero.DoneQuestQTThiStage = true;
            }
            else
            {
                Utilities.Hero.DoneQuestQTApprenticeThisStage = true;
            }
        }

        if (talkerType == QuestTalker.TalkerType.CLERIC)
        {
            player.Stats.DecreaseValue(Stat.CORRUPTION, CorruptionModifierValue);
        }
        else
        {
            player.Stats.IncreaseValue(Stat.CORRUPTION, CorruptionModifierValue);
        }


        Hero.CallCorruptionBenedictionText(talkerType == QuestTalker.TalkerType.CLERIC ? -CorruptionModifierValue : CorruptionModifierValue);
        OnQuestFinished?.Invoke();

        MapUtilities.onEarlyAllEnemiesDead -= CheckQuestFinished;
        MapUtilities.onEarlyAllChestOpen -= CheckQuestFinished;
        MapUtilities.onFirstEnter -= CheckQuestFinished;
        Utilities.Hero.OnQuestObtained -= CheckQuestFinished;
        ResetQuestValues();

        if (timeManagerRoutine != null)
            CoroutineManager.Instance.StopCoroutine(timeManagerRoutine);
        timeManagerRoutine = null;

        HudHandler.current.MessageInfoHUD.Display($"You finished the quest <color=yellow>\"{Datas.idName.SeparateAllCase()}\"</color>.");
    }

    protected void QuestLost()
    {
        player.CurrentQuest = null;
        questLost = true;
        QuestUpdated();
        AudioManager.Instance.PlaySound(AudioManager.Instance.QuestLostSFX);
        MapUtilities.onEarlyAllEnemiesDead -= CheckQuestFinished;
        MapUtilities.onEarlyAllChestOpen -= CheckQuestFinished;
        MapUtilities.onFirstEnter -= CheckQuestFinished;
        Utilities.Hero.OnQuestObtained -= CheckQuestFinished;
        ResetQuestValues();

        if (timeManagerRoutine != null)
            CoroutineManager.Instance.StopCoroutine(timeManagerRoutine);
        timeManagerRoutine = null;

        HudHandler.current.MessageInfoHUD.Display($"You lost the quest <color=yellow>\"{Datas.idName.SeparateAllCase()}\"</color>.");
    }

    protected void QuestUpdated()
    {
        if (IsQuestFinished())
        {
            HudHandler.current.QuestHUD.EmptyQuestTexts();
            HudHandler.current.QuestHUD.LostOrFinishedText.SetText("<color=yellow>Quest Completed!</color>\n Clear the current room to receive rewards!");
        }
        else if (questLost)
        {
            HudHandler.current.QuestHUD.EmptyQuestTexts();
            HudHandler.current.QuestHUD.LostOrFinishedText.SetText("<color=red>Quest Lost...</color>");
        }
        else
        {
            OnQuestUpdated?.Invoke();
        }
    }

    protected void CheckQuestFinished()
    {
        if (questLost)
        {
            QuestLost();
        }
        else if (IsQuestFinished())
        {
            QuestFinished();
        }
    }

    private IEnumerator TimeToFinishRoutine()
    {
        CurrentQuestTimer = timeToFinishQuest;
        while (CurrentQuestTimer > 0)
        {
            if (player == null || IsQuestFinished())
            {
                timeManagerRoutine = null;
                yield break;
            }

            CurrentQuestTimer -= Time.deltaTime;
            QuestUpdated();
            yield return null;
        }
        QuestLost();
        yield break;
    }

    public virtual void Save(SaveData save)
    {
        save.Set("questId", Datas.idName);
        save.Set("questDifficulty", difficulty);
        save.Set("talkerType", talkerType);
        save.Set("talkerGrade", talkerGrade);

        save.Set("questTimer", CurrentQuestTimer);
    }

    public virtual void LoadSave()
    {
        // Quest values
        CurrentQuestTimer = SaveManager.saveData.Get<float>("questTimer");
    }

    #region STATIC_METHODS
    static public Quest LoadClass(string name, QuestDialogueDifficulty difficulty, QuestTalker questTalker)
    {
        if (database == null)
        {
            database = GameResources.Get<QuestDatabase>("QuestDatabase");
        }

        Quest quest = Assembly.GetExecutingAssembly().CreateInstance(name.GetPascalCase()) as Quest;
        quest.Datas = database.GetQuest(name);
        quest.player = GameObject.FindWithTag("Player").GetComponent<Hero>();
        quest.talkerType = questTalker.Type;
        quest.talkerGrade = questTalker.Grade;
        quest.CorruptionModifierValue = quest.Datas.CorruptionModifierValue;
        quest.difficulty = quest.Datas.HasDifferentGrades ? (QuestDifficulty)difficulty : QuestDifficulty.MEDIUM;
        InitDescription(ref quest.Datas.Description);

        return quest;
    }

    static public Quest LoadClassWithSave(string name, QuestDifficulty difficulty, TalkerType talkerType, TalkerGrade talkerGrade)
    {
        if (database == null)
        {
            database = GameResources.Get<QuestDatabase>("QuestDatabase");
        }

        Quest quest = Assembly.GetExecutingAssembly().CreateInstance(name.GetPascalCase()) as Quest;
        quest.Datas = database.GetQuest(name);
        quest.player = GameObject.FindWithTag("Player").GetComponent<Hero>();
        quest.talkerType = talkerType;
        quest.talkerGrade = talkerGrade;
        quest.CorruptionModifierValue = quest.Datas.CorruptionModifierValue;
        quest.difficulty = difficulty;
        InitDescription(ref quest.Datas.Description);
        quest.LoadSave();

        return quest;
    }

    static public string GetRandomQuestName()
    {
        if (database == null)
        {
            database = GameResources.Get<QuestDatabase>("QuestDatabase");
        }

        int indexRandom = UnityEngine.Random.Range(0, database.datas.Count);
        return database.datas[indexRandom].idName;
    }

    static private void InitDescription(ref string description)
    {
        string finalDescription = string.Empty;
        char[] separators = new char[] { ' ', '\n' };
        string[] splitDescription = description.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        foreach (var test in splitDescription)
        {
            finalDescription += test + ' ';
        }

        description = finalDescription;
    }
    #endregion
}
