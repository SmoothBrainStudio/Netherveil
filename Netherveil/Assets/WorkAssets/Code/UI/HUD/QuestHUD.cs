using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private TMP_Text lostOrFinishedText;


    [SerializeField] private RectTransform questTransform;
    private bool questEnable = false;
    public bool QuestEnable { get => questEnable; }
    public TMP_Text LostOrFinishedText { get => lostOrFinishedText; }
    private Coroutine questRoutine;
    public float progressTextSize;

    void Awake()
    {
        lostOrFinishedText.SetText("No Quests...");
        EmptyQuestTexts();

        Utilities.Hero.OnQuestObtained += UpdateUI;
        Utilities.Hero.OnQuestFinished += UpdateUI;
        progressTextSize = progressText.fontSize;
    }

    public void EmptyQuestTexts()
    {
        title.SetText(string.Empty);
        description.SetText(string.Empty);
        rewardText.SetText(string.Empty);
        progressText.SetText(string.Empty);
        difficultyText.SetText(string.Empty);
    }

    public void Toggle()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (!questEnable)
        {
            if (questRoutine != null)
                StopCoroutine(questRoutine);

            questTransform.anchoredPosition = new Vector2(questTransform.sizeDelta.x, questTransform.anchoredPosition.y);
            questRoutine = StartCoroutine(questTransform.TranslateX(0.1f, -questTransform.sizeDelta.x));
        }
        else
        {
            if (questRoutine != null)
                StopCoroutine(questRoutine);

            questTransform.anchoredPosition = new Vector2(0.0f, questTransform.anchoredPosition.y);
            questRoutine = StartCoroutine(questTransform.TranslateX(0.1f, questTransform.sizeDelta.x));
        }
        questEnable = !questEnable;
    }

    private void OnEnable()
    {
        Quest.OnQuestUpdated += UpdateUI;
    }

    private void OnDisable()
    {
        if (questRoutine != null)
        {
            StopCoroutine(questRoutine);

            Vector3 anchoredPos = questTransform.anchoredPosition;
            anchoredPos.x = questTransform.sizeDelta.x;
            questTransform.anchoredPosition = anchoredPos;

            questEnable = false;
        }

        Quest.OnQuestUpdated -= UpdateUI;
    }

    public void UpdateUI()
    {
        bool hasQuest = Utilities.Hero.CurrentQuest != null;

        if (hasQuest)
        {
            lostOrFinishedText.SetText(string.Empty);

            string rewardName = Utilities.Hero.CurrentQuest.TalkerType == QuestTalker.TalkerType.SHAMAN ? 
                $"<sprite name=\"corruption\">" : 
                $"<sprite name=\"benediction\">";

            int absValue = Mathf.Abs(Utilities.Hero.CurrentQuest.CorruptionModifierValue);

            if (Utilities.Hero.CurrentQuest.Datas.HasDifferentGrades)
            {
                switch (Utilities.Hero.CurrentQuest.Difficulty)
                {
                    case Quest.QuestDifficulty.EASY:
                        difficultyText.SetText("<color=green>Easy</color>");
                        break;
                    case Quest.QuestDifficulty.MEDIUM:
                        difficultyText.SetText("<color=orange>Medium</color>");
                        break;
                    case Quest.QuestDifficulty.HARD:
                        difficultyText.SetText("<color=red>Hard</color>");
                        break;
                    default:
                        difficultyText.SetText("ERROR");
                        break;
                }
            }
            else
            {
                difficultyText.SetText(string.Empty);
            }

            //
            string titleText = Utilities.Hero.CurrentQuest.TalkerType == QuestTalker.TalkerType.SHAMAN ?
                $"<color=purple>{Utilities.Hero.CurrentQuest.Datas.idName}</color>" :
                $"<color=yellow>{Utilities.Hero.CurrentQuest.Datas.idName}</color>";

            title.SetText(titleText);
            description.SetText(Utilities.Hero.CurrentQuest.Datas.Description);
            rewardText.SetText($"\n <color=yellow>Reward : </color> {absValue} {rewardName}");
            progressText.SetText(Utilities.Hero.CurrentQuest.progressText + "\n" + GetTimeString());

            description.GetComponent<ContentSizeFitter>().SetLayoutVertical();
        }
        else
        {
            EmptyQuestTexts();
        }
    }

    private string GetTimeString()
    {
        if(!Utilities.Hero.CurrentQuest.Datas.LimitedTime || Utilities.Hero.CurrentQuest.IsQuestFinished())
            return string.Empty;

        if (Utilities.Hero.CurrentQuest.CurrentQuestTimer < 60)
            return "<color=red>" + Math.Round(Utilities.Hero.CurrentQuest.CurrentQuestTimer, Utilities.Hero.CurrentQuest.CurrentQuestTimer < 1 ? 1 : 0) + " seconds remaining</color>";
        else
            return Math.Round(Utilities.Hero.CurrentQuest.CurrentQuestTimer, 0) + " seconds remaining";
    }
}
