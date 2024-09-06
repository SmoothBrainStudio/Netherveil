using System.IO;

public class VulcanHunter : Quest
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
        MAX_NUMBER = 3;

        switch (difficulty)
        {
            case QuestDifficulty.EASY:
                timeToFinishQuest = 600f;
                break;
            case QuestDifficulty.MEDIUM:
                timeToFinishQuest = 450f;
                CorruptionModifierValue += 5;
                break;
            case QuestDifficulty.HARD:
                timeToFinishQuest = 300f;
                CorruptionModifierValue += 10;
                break;
        }
        progressText = $"NB <size={HudHandler.current.QuestHUD.progressTextSize + 15}><sprite name=\"gorgon\"><size={HudHandler.current.QuestHUD.progressTextSize}> KILLED : {currentNumber}/{MAX_NUMBER}";
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
        if (!IsQuestFinished() && damageable as IGorgon != null)
        {
            currentNumber++;
            progressText = $"NB GORGONS KILLED : {currentNumber}/{MAX_NUMBER}";
        }
        QuestUpdated();
    }
}
