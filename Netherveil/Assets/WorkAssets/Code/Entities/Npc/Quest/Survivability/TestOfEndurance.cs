using Map;
//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class TestOfEndurance : Quest
{
    int currentSurvivedRoom = 0;
    int NB_ROOM_SURVIVING;
    const float HP_PERCENTAGE_THRESHOLD = 0.25f;

    public override void Save(SaveData saveData)
    {
        base.Save(saveData);
        saveData.Set("questEvolution", currentSurvivedRoom);
    }

    public override void LoadSave()
    {
        base.LoadSave();

        currentSurvivedRoom = SaveManager.saveData.Get<int>("questEvolution");
    }


    public override void AcceptQuest()
    {
        base.AcceptQuest();
        switch (difficulty)
        {
            case QuestDifficulty.EASY:
                NB_ROOM_SURVIVING = 2;
                break;
            case QuestDifficulty.MEDIUM:
                CorruptionModifierValue += 5;
                NB_ROOM_SURVIVING = 4;
                break;
            case QuestDifficulty.HARD:
                CorruptionModifierValue += 10;
                NB_ROOM_SURVIVING = 6;
                break;
        }
        progressText = $"DON'T FALL UNDER {HP_PERCENTAGE_THRESHOLD * 100}% HP DURING {NB_ROOM_SURVIVING} FIGHT ROOMS : {currentSurvivedRoom}/{NB_ROOM_SURVIVING}";
        MapUtilities.onEarlyAllEnemiesDead += UpdateCount;
        Utilities.Hero.OnTakeDamage += TestHp;
    }

    protected override void ResetQuestValues()
    {
        MapUtilities.onEarlyAllEnemiesDead -= UpdateCount;
        Utilities.Hero.OnTakeDamage -= TestHp;
    }

    private void TestHp(int _arg, IAttacker _attacker)
    {
        if (IsQuestLost())
        {
            questLost = true;
        }
        QuestUpdated();
    }

    private void UpdateCount()
    {
        if (!IsQuestFinished() && !questLost)
        {
            currentSurvivedRoom++;
            progressText = $"DON'T FALL UNDER {HP_PERCENTAGE_THRESHOLD * 100}% HP DURING {NB_ROOM_SURVIVING} FIGHT ROOMS : {currentSurvivedRoom}/{NB_ROOM_SURVIVING}";
        }
        QuestUpdated();

        CheckQuestFinished();
    }

    public override bool IsQuestFinished()
    {
        return currentSurvivedRoom >= NB_ROOM_SURVIVING;
    }

    protected bool IsQuestLost()
    {
        return Utilities.Hero.Stats.GetValue(Stat.HP) / Utilities.Hero.Stats.GetMaxValue(Stat.HP) < HP_PERCENTAGE_THRESHOLD;
    }
}