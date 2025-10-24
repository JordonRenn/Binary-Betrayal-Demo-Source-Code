using UnityEngine;
using System.Linq;

namespace SBG.Questing
{
    [System.Serializable]
    public enum QuestState
    {
        Incomplete,
        InProgress,
        Complete,
        Failed,
        Abandoned
    }

    [System.Serializable]
    public class QuestData
    {
        public string quest_id;
        public QuestType type;
        public string title;
        public string description;
        public Objective[] objectives;
        public string[] prerequisiteQuestIds; // For quest chains
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
    public struct QuestReward
    {
        public string itemId;
        public int quantity;
        public float experiencePoints;
        public float currency;
    }

    [System.Serializable]
    public class RuntimeQuest : Quest
    {
        private QuestData data;

        public RuntimeQuest(QuestData questData)
        {
            data = questData;
            ID = questData.quest_id;
            Type = questData.type;  // Using the Type property from base class
            Title = questData.title;
            Description = questData.description;
            Objectives = questData.objectives.Select(obj => ObjectiveFactory.CreateObjective(obj)).ToList();
            CurrentState = QuestState.Incomplete;
        }

        public override void Start()
        {
            if (CurrentState == QuestState.Incomplete)
            {
                CurrentState = QuestState.InProgress;
                SBGDebug.LogInfo($"Started quest: {Title}", "RuntimeQuest");
            }
        }
    }

    [System.Serializable]
    public class Objective
    {
        public int objective_id;
        public ObjectiveType type;
        public string[] item;  // For collect objectives
        public int quantity;
        public int currentQuantity;
        public string[] enemy; // For kill objectives
        public string dialogID; // For talk objectives
        public ObjectiveTarget[] target;  // Use a specific type (e.g., NPC or item) later
        public string message;
        public bool IsCompleted { get; private set; }

        public float Progress =>
            quantity > 0 ? Mathf.Clamp01((float)currentQuantity / quantity) :
            IsCompleted ? 1f : 0f;

        public void MarkAsComplete()
        {
            IsCompleted = true;
        }

        public void UpdateProgress(int newCount)
        {
            currentQuantity = Mathf.Min(newCount, quantity);
            if (currentQuantity >= quantity)
                MarkAsComplete();
        }
    }

    [System.Serializable]
    public class ObjectiveTarget
    {
        public string targetId;
        public Vector3 position;
        public TargetType type;
        public bool isInteracted;

        // Additional data can be stored in a serialized format
        public string data;
    }

    public enum QuestType
    {
        Main,
        Side,
        Secret,
        Encounter
    }

    public enum ObjectiveType
    {
        Talk,
        Collect,
        DoorLock,
        PhoneCall,
        Kill,
        Explore,
        UseItem,
        Interact
    }

    public enum ObjectiveStatus
    {
        InProgress,
        CompletedSuccess,
        CompletedFailure,
        Abandoned
    }

    public enum RewardType
    {
        Item,
        Experience,
        Currency,
        Skill,
        Reputation
    }

    public enum TargetType
    {
        NPC,
        Item,
        Location,
        Interactable
    }
}