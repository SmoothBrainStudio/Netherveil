//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class MonsterHunter : Quest
{
    int currentNumber = 0;
    int MAX_NUMBER;

    public override void Save(SaveData saveData)
    {
        base.Save(saveData);
        saveData.Set("questEvolution", currentNumber);
    }

    public override void LoadSave()
    {
        base.LoadSave();

        currentNumber = SaveManager.saveData.Get<int>("questEvolution");
    }


    public override void AcceptQuest()
    {
        base.AcceptQuest();
        switch (difficulty)
        {
            case QuestDifficulty.EASY:
                MAX_NUMBER = 10;
                timeToFinishQuest = 450f;
                break;
            case QuestDifficulty.MEDIUM:
                MAX_NUMBER = 15;
                CorruptionModifierValue += 5;
                timeToFinishQuest = 360f;
                break;
            case QuestDifficulty.HARD:
                MAX_NUMBER = 20;
                timeToFinishQuest = 240f;
                CorruptionModifierValue += 10;
                break;
        } 
        progressText = $"NB MONSTERS KILLED : {currentNumber}/{MAX_NUMBER}";
        Utilities.Hero.OnKill += UpdateCount;
    }

    public override bool IsQuestFinished()
    {
        return currentNumber >= MAX_NUMBER;
    }

    protected override void ResetQuestValues()
    {
        Utilities.Hero.OnKill -= UpdateCount;
    }

    private void UpdateCount(IDamageable damageable)
    {
        if (!IsQuestFinished() && damageable is not IDummy)
        {
            currentNumber++;
            progressText = $"NB MONSTERS KILLED : {currentNumber}/{MAX_NUMBER}";
        }
        QuestUpdated();
    }
}
