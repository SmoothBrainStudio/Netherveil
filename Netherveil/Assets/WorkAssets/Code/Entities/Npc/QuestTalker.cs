using DialogueSystem.Runtime;
using Map.Generation;
using UnityEngine;

//Copyright 2024 Property of Olivier Maurin.All rights reserved.
public class QuestTalker : Npc
{
    [Header("Talker parameters")]
    [SerializeField] protected DialogueTree questDT;
    [SerializeField] protected DialogueTree refusesDialogueDT;
    [SerializeField] protected DialogueTree alreadyHaveQuestDT;
    [SerializeField] protected DialogueTree alreadyDoneQuestDT;
    [SerializeField] protected DialogueTree waitClearTutoDT;
    protected DialogueTreeRunner dialogueTreeRunner;
    protected Hero player;
    static QuestDatabase database;
    public enum TalkerType
    {
        CLERIC,
        SHAMAN
    }

    public enum TalkerGrade
    {
        BOSS,
        APPRENTICE
    }

    [SerializeField] protected TalkerType type;
    [SerializeField] protected TalkerGrade grade;
    public TalkerType Type => type;
    public TalkerGrade Grade => grade;
    public int QuestIndex { get; private set; }

    protected override void Awake()
    {
        if (database == null)
        {
            database = GameResources.Get<QuestDatabase>("QuestDatabase");
        }

        QuestIndex = Seed.Range(0, database.datas.Count);
        //QuestIndex = database.datas.FindIndex(x => x.idName == "MonsterHunter");
    }

    protected override void Start()
    {
        base.Start();
        dialogueTreeRunner = FindObjectOfType<DialogueTreeRunner>();
        player = GameObject.FindWithTag("Player").GetComponent<Hero>();
    }

    public override void Interract()
    {
        if (!dialogueTreeRunner.IsStarted)
        {
            StartDialogue();
        }
    }

    protected virtual void StartDialogue()
    {
        DialogueTree dialogue = questDT;

        if(!player.ClearedTuto)
        {
            dialogue = waitClearTutoDT;
        }
        else if (PlayerInvestedInOppositeWay())
        {
            dialogue = refusesDialogueDT;
        }
        else if (player.CurrentQuest != null)
        {
            dialogue = alreadyHaveQuestDT;
        }
        else if (player.DoneQuestQTThiStage)
        {
            dialogue = alreadyDoneQuestDT;
        }

        dialogueTreeRunner.NameBackgroundImage.color = type == TalkerType.CLERIC ? Hero.benedictionColor2 : Hero.corruptionColor2;
        dialogueTreeRunner.StartDialogue(dialogue, this);
    }

    protected bool PlayerInvestedInOppositeWay()
    {
        if (type == TalkerType.CLERIC && player.Stats.GetValue(Stat.CORRUPTION) >= Hero.STEP_VALUE)
        {
            return true;
        }
        else if (type == TalkerType.SHAMAN && player.Stats.GetValue(Stat.CORRUPTION) <= -Hero.STEP_VALUE)
        {
            return true;
        }
        return false;
    }

    public string GetQuestName()
    {
        return database.datas[QuestIndex].idName;
    }
}
