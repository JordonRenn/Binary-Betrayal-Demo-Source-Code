using System.Collections.Generic;
using System.Linq;

public abstract class Quest
{
    public int ID { get; set; }
    public QuestType Type { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<IObjective> Objectives { get; protected set; }
    public QuestState CurrentState { get; protected set; }

    public virtual void Start()
    {
        // Initialize the quest
        CurrentState = QuestState.Incomplete;
    }

    public virtual bool IsCompleted() => CurrentState == QuestState.Complete;

    public List<int> Prerequisites { get; set; }

    public virtual void Complete()
    {
        if (CurrentState != QuestState.Complete)
        {
            CurrentState = QuestState.Complete;
            SBGDebug.LogInfo($"Completed quest: {Title}", GetType().Name);
        }
    }

    public void CheckPrerequisites()
    {
        // Logic to check if prerequisites are met
    }
}

[System.Serializable]
public class QuestData
{
    public int quest_id;
    public QuestType type;
    public string title;
    public string description;
    public Objective[] objectives;
    public int[] prerequisiteQuestIds; // For quest chains
    public QuestReward[] rewards;
    public bool isRepeatable;
    public float timeLimit;
    public QuestState state { get; private set; } = QuestState.Incomplete;

    public float Progress =>
        objectives?.Length > 0
            ? objectives.Average(o => o.Progress)
            : 0f;

    public void UpdateState()
    {
        if (state == QuestState.Complete) return;

        if (objectives == null || objectives.Length == 0)
        {
            state = QuestState.Complete;
            return;
        }

        bool allComplete = objectives.All(o => o.IsCompleted);
        state = allComplete ? QuestState.Complete :
               objectives.Any(o => o.Progress > 0) ? QuestState.InProgress :
               QuestState.Incomplete;
    }
}

[System.Serializable]
public class QuestReward
{
    public RewardType type;
    public string itemId;
    public int quantity;
    public int experiencePoints;
    public float currency;
}