using System.IO;

public class KitingMyDearLove : Quest 
{
    int currentNumber = 0;
    bool distanceAttackCalled = false;
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
                MAX_NUMBER = 5;
                timeToFinishQuest = 400f;
                break;
            case QuestDifficulty.MEDIUM:
                MAX_NUMBER = 8;
                timeToFinishQuest = 320f;
                CorruptionModifierValue += 5;
                break;
            case QuestDifficulty.HARD:
                MAX_NUMBER = 10;
                timeToFinishQuest = 240f;
                CorruptionModifierValue += 10;
                break;
        }
        progressText = $"NB MONSTERS KILLED WITH SPEAR LAUNCH ATTACK : {currentNumber}/{MAX_NUMBER}";
        Utilities.Hero.OnSpearAttack += SetBool;
        Utilities.Hero.OnKill += UpdateCount;
    }

    public override bool IsQuestFinished()
    {
        return currentNumber >= MAX_NUMBER;
    }

    protected override void ResetQuestValues()
    {
        Utilities.Hero.OnSpearAttack -= SetBool;
        Utilities.Hero.OnKill -= UpdateCount;
    }

    private void SetBool(IDamageable damageable, IAttacker attacker)
    {
        distanceAttackCalled = true;
    }

    private void UpdateCount(IDamageable damageable)
    {
        if (!IsQuestFinished() && damageable is not IDummy)
        {
            Entity monster = (damageable as Entity);
            if (distanceAttackCalled && monster != null && monster.Stats.GetValue(Stat.HP) <= 0)
            {
                currentNumber++;
                progressText = $"NB MONSTERS KILLED WITH SPEAR LAUNCH ATTACK : {currentNumber}/{MAX_NUMBER}";
            }
            distanceAttackCalled = false;
        }
        QuestUpdated();
    }
} 
