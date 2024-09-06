using System.IO;

public class DontDealWithMe : Quest
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
                timeToFinishQuest = 360f;
                break;
            case QuestDifficulty.MEDIUM:
                MAX_NUMBER = 15;
                timeToFinishQuest = 240f;
                CorruptionModifierValue += 5;
                break;
            case QuestDifficulty.HARD:
                MAX_NUMBER = 20;
                timeToFinishQuest = 180f;
                CorruptionModifierValue += 10;
                break;
        }
        progressText = $"NB MONSTERS HIT WITH CHARGED ATTACK : {currentNumber}/{MAX_NUMBER}";
        Utilities.Hero.OnChargedAttack += UpdateCount;
    }

    public override bool IsQuestFinished()
    {
        return currentNumber >= MAX_NUMBER;
    }

    protected override void ResetQuestValues()
    {
        Utilities.Hero.OnChargedAttack -= UpdateCount;
    }

    private void UpdateCount(IDamageable damageable, IAttacker attacker)
    {
        if (!IsQuestFinished() && damageable is not IDummy && damageable is Mobs && !(damageable as Mobs).IsSpawning)
        {
            currentNumber++;
            progressText = $"NB MONSTERS HIT WITH CHARGED ATTACK : {currentNumber}/{MAX_NUMBER}";
        }

        QuestUpdated();
    }
}
