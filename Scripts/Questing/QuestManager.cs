using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class QuestsWrapper
{
    public QuestData[] quests;
}

public sealed class QuestManager : MonoBehaviour
{
    private static QuestManager _instance;
    public static QuestManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError($"Attempting to access {nameof(QuestManager)} before it is initialized.");
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Quest Data")]
    [SerializeField] private TextAsset questJSON;

    public Dictionary<int, RuntimeQuest> QuestByID { get; private set; }
    public List<RuntimeQuest> activeQuests = new List<RuntimeQuest>();
    public Dictionary<int, RuntimeQuest> completedQuests { get; private set; }

    void Awake()
    {
        // Initialize as singleton and persist across scenes since quest data is persistent
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            // Initialize collections
            QuestByID = new Dictionary<int, RuntimeQuest>();
            completedQuests = new Dictionary<int, RuntimeQuest>();

            // Validate required data
            if (questJSON == null)
            {
                Debug.LogError($"{nameof(QuestManager)}: Quest JSON data file is missing!");
                return;
            }

            LoadQuestData();
        }
    }

    private void LoadQuestData()
    {
        try 
        {
            var questDataArray = JsonUtility.FromJson<QuestsWrapper>(questJSON.text).quests;
            if (questDataArray == null || questDataArray.Length == 0)
            {
                Debug.LogWarning($"{nameof(QuestManager)}: No quests found in JSON data!");
                return;
            }

            foreach (var questData in questDataArray)
            {
                AddQuest(questData);
            }
            SBGDebug.LogInfo($"Loaded {questDataArray.Length} quests successfully", "QuestManager");
        }
        catch (System.Exception e)
        {
            SBGDebug.LogError($"Failed to parse quest JSON: {e.Message}", "QuestManager");
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (questJSON == null)
        {
            Debug.LogWarning($"{nameof(QuestManager)}: Quest JSON data file reference is required!");
        }
        else
        {
            // Validate quest data structure in editor
            try 
            {
                var questDataArray = JsonUtility.FromJson<QuestsWrapper>(questJSON.text).quests;
                if (questDataArray == null || questDataArray.Length == 0)
                {
                    Debug.LogWarning($"{nameof(QuestManager)}: Quest JSON file contains no quest data!");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{nameof(QuestManager)}: Invalid quest JSON data structure: {e.Message}");
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        // Reset static instance when entering play mode in editor
        _instance = null;
    }
#endif

    private void AddQuest(QuestData questData)
    {
        var quest = new RuntimeQuest(questData);
        QuestByID[quest.ID] = quest;
    }

    private void MarkQuestAsComplete(int questId)
    {
        if (QuestByID.TryGetValue(questId, out var quest))
        {
            quest.Complete();
            
            // Move from active to completed
            if (activeQuests.Contains(quest))
            {
                activeQuests.Remove(quest);
            }
            completedQuests[questId] = quest;
            
            SBGDebug.LogInfo($"Quest {questId} marked as complete", "QuestManager");
        }
    }

    public void StartQuest(int questID)
    {
        if (QuestByID.TryGetValue(questID, out var quest))
        {
            if (!completedQuests.ContainsKey(questID) && !activeQuests.Contains(quest))
            {
                quest.Start();
                activeQuests.Add(quest);
                SBGDebug.LogInfo($"Started quest: {quest.Title}", "QuestManager");
            }
            else if (completedQuests.ContainsKey(questID))
            {
                SBGDebug.LogWarning($"Cannot start quest {questID}: Already completed", "QuestManager");
            }
            else
            {
                SBGDebug.LogWarning($"Cannot start quest {questID}: Already active", "QuestManager");
            }
        }
    }

    public bool IsQuestCompleted(int questID)
    {
        return completedQuests.ContainsKey(questID);
    }

    public IEnumerable<RuntimeQuest> GetQuestsByType(QuestType type)
    {
        return QuestByID.Values.Where(q => q.Type == type);
    }

    public IEnumerable<RuntimeQuest> GetActiveQuestsByType(QuestType type)
    {
        return activeQuests.Where(q => q.Type == type);
    }

    public IEnumerable<RuntimeQuest> GetCompletedQuestsByType(QuestType type)
    {
        return completedQuests.Values.Where(q => q.Type == type);
    }

    public float GetQuestCompletionPercentage(int questID)
    {
        if (QuestByID.TryGetValue(questID, out var quest))
        {
            if (quest.Objectives == null || quest.Objectives.Count == 0) return 0f;
            return (float)quest.Objectives.Count(o => o.IsCompleted) / quest.Objectives.Count * 100f;
        }
        return 0f;
    }

    public void CompleteObjective(int objectiveID, ObjectiveStatus status = ObjectiveStatus.CompletedSuccess)
    {
        foreach (var quest in activeQuests)
        {
            var objective = quest.Objectives?.FirstOrDefault(obj => obj.ObjectiveID == objectiveID);
            if (objective != null)
            {
                objective.ForceComplete(status);
                SBGDebug.LogInfo($"Objective {objectiveID} completed with status {status} in quest {quest.ID}.", "QuestManager");
                CheckIfAllObjectivesAreComplete(quest);
                return;
            }
        }
        SBGDebug.LogError($"Objective {objectiveID} not found.", "QuestManager");
    }

    public void CompleteQuest(int questID, ObjectiveStatus status = ObjectiveStatus.CompletedSuccess)
    {
        if (QuestByID.TryGetValue(questID, out var quest))
        {
            if (quest.Objectives != null)
            {
                foreach (var objective in quest.Objectives)
                {
                    if (!objective.IsCompleted)
                    {
                        objective.ForceComplete(status);
                    }
                }
            }
            quest.Complete();

            if (status == ObjectiveStatus.Abandoned)
            {
                activeQuests.Remove(quest);
                SBGDebug.LogInfo($"Quest {questID} abandoned.", "QuestManager");
            }
            else
            {
                SBGDebug.LogInfo($"Quest {questID} completed with status {status}.", "QuestManager");
            }
        }
    }

    // Helper query methods
    public bool HasFailedObjectives(int questID)
    {
        if (QuestByID.TryGetValue(questID, out var quest))
        {
            return quest.Objectives?.Any(o => o.Status == ObjectiveStatus.CompletedFailure) ?? false;
        }
        return false;
    }

    public IEnumerable<IObjective> GetFailedObjectives(int questID)
    {
        if (QuestByID.TryGetValue(questID, out var quest))
        {
            return quest.Objectives?.Where(o => o.Status == ObjectiveStatus.CompletedFailure) ?? Enumerable.Empty<IObjective>();
        }
        return Enumerable.Empty<IObjective>();
    }

    public IEnumerable<IObjective> GetObjectivesByStatus(int questID, ObjectiveStatus status)
    {
        if (QuestByID.TryGetValue(questID, out var quest))
        {
            return quest.Objectives?.Where(o => o.Status == status) ?? Enumerable.Empty<IObjective>();
        }
        return Enumerable.Empty<IObjective>();
    }

    public ObjectiveStatus GetQuestOverallStatus(int questID)
    {
        if (QuestByID.TryGetValue(questID, out var quest))
        {
            if (quest.Objectives == null || quest.Objectives.Count == 0)
                return ObjectiveStatus.CompletedSuccess;

            if (quest.Objectives.All(o => o.Status == ObjectiveStatus.Abandoned))
                return ObjectiveStatus.Abandoned;

            if (quest.Objectives.Any(o => o.Status == ObjectiveStatus.CompletedFailure))
                return ObjectiveStatus.CompletedFailure;

            if (quest.Objectives.All(o => o.IsCompleted))
                return ObjectiveStatus.CompletedSuccess;

            return ObjectiveStatus.InProgress;
        }
        return ObjectiveStatus.CompletedFailure;
    }

    private void CheckIfAllObjectivesAreComplete(RuntimeQuest quest)
    {
        if (quest.Objectives == null) return;
        
        var allCompleted = quest.Objectives.All(obj => obj.IsCompleted);
        if (allCompleted)
        {
            quest.Complete();
        }
    }
}
