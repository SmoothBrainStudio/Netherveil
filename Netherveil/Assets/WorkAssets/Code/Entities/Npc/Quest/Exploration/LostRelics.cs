using Map;

//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class LostRelics : Quest
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
        switch (difficulty)
        {
            case QuestDifficulty.EASY:
                MAX_NUMBER = 1;
                break;
            case QuestDifficulty.MEDIUM:
                CorruptionModifierValue += 5;
                MAX_NUMBER = 2;
                break;
            case QuestDifficulty.HARD:
                CorruptionModifierValue += 10;
                MAX_NUMBER = 3;
                break;
        }

        currentNumber = SaveManager.saveData.Get<int>("questEvolution");
    }


    public override void AcceptQuest()
    {
        base.AcceptQuest();

        currentNumber = MapUtilities.nbEnterRoomByType[RoomType.Treasure];
        MAX_NUMBER = MapUtilities.nbRoomByType[RoomType.Treasure];
        progressText = $"NB TREASURE/SHOP ROOM DISCOVERED : {currentNumber}/{MAX_NUMBER}";
        MapUtilities.onFirstEnter += UpdateCount;
    }

    public override bool IsQuestFinished()
    {
        return currentNumber >= MAX_NUMBER;
    }

    protected override void ResetQuestValues()
    {
        MapUtilities.onFirstEnter -= UpdateCount;
    }

    private void UpdateCount()
    {
        if (!IsQuestFinished() && (MapUtilities.currentRoomData.Type == RoomType.Treasure || MapUtilities.currentRoomData.Type == RoomType.Merchant))
        {
            currentNumber = MapUtilities.nbEnterRoomByType[RoomType.Treasure] + MapUtilities.nbEnterRoomByType[RoomType.Merchant];
            progressText = $"NB TREASURE ROOM DISCOVERED : {currentNumber}/{MAX_NUMBER}";
        }
        QuestUpdated();
    }
}
