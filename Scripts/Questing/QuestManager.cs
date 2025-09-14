using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

/* 
888     888 888b    888 88888888888 8888888888 .d8888b. 88888888888 8888888888 8888888b.  
888     888 8888b   888     888     888       d88P  Y88b    888     888        888  "Y88b 
888     888 88888b  888     888     888       Y88b.         888     888        888    888 
888     888 888Y88b 888     888     8888888    "Y888b.      888     8888888    888    888 
888     888 888 Y88b888     888     888           "Y88b.    888     888        888    888 
888     888 888  Y88888     888     888             "888    888     888        888    888 
Y88b. .d88P 888   Y8888     888     888       Y88b  d88P    888     888        888  .d88P 
 "Y88888P"  888    Y888     888     8888888888 "Y8888P"     888     8888888888 8888888P"  

 UNTESTED-UNTESTED-UNTESTED-UNTESTED-UNTESTED-UNTESTED-UNTESTED-UNTESTED-UNTESTED-UNTESTED
 */

 /*
    Need to update objective to work with new Objective Event System
 */



public static class QuestManager
{
    private static TextAsset questJSON;

    public static Dictionary<string, RuntimeQuest> QuestByID { get; private set; }
    public static List<RuntimeQuest> activeQuests = new List<RuntimeQuest>();
    public static Dictionary<string, RuntimeQuest> completedQuests { get; private set; }

    public static bool questsLoaded { get; private set; }
    public static bool jsonValidated { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static async void Init()
    {
        QuestByID = new Dictionary<string, RuntimeQuest>();
        completedQuests = new Dictionary<string, RuntimeQuest>();
        await Task.Run(() => new WaitUntil(() => GameMaster.Instance != null));
        LoadQuestJson();
        //LoadQuestData();
        await Task.Run(() => new WaitUntil(() => questJSON != null));
        //ValidateJSON();
    }
    
    private static void LoadQuestJson()
    {
        try
        {
            string questFilePath = Path.Combine(Application.streamingAssetsPath, "Quests/Quests.json");
            
            if (File.Exists(questFilePath))
            {
                string jsonContent = File.ReadAllText(questFilePath);
                questJSON = new TextAsset(jsonContent);
                SBGDebug.LogInfo($"Successfully loaded quest file from {questFilePath}", "QuestManager");
            }
            else
            {
                SBGDebug.LogError($"Quest file not found at path: {questFilePath}", "QuestManager");
            }
        }
        catch (System.Exception e)
        {
            SBGDebug.LogError($"Failed to load quest JSON file: {e.Message}", "QuestManager");
        }
    }
    private static void LoadQuestData()
    {
        try
        {
            var questDataArray = QuestJsonParser.ParseQuestsData(questJSON);
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

        questsLoaded = true;
    }
    private static void ValidateJSON()
    {
        if (questJSON == null)
        {
            Debug.LogWarning($"{nameof(QuestManager)}: Quest JSON data file reference is required!");
        }
        else
        {
            var questDataArray = null as QuestData[];
            try
            {
                questDataArray = QuestJsonParser.ParseQuestsData(questJSON);
                if (questDataArray == null || questDataArray.Length == 0)
                {
                    Debug.LogWarning($"{nameof(QuestManager)}: Quest JSON file contains no quest data!");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{nameof(QuestManager)}: Invalid quest JSON data structure: {e.Message}");
            }
            finally
            {
                if (questDataArray != null && questDataArray.Length > 0) jsonValidated = true;
            }
        }
    }

    private static void AddQuest(QuestData questData)
    {
        var quest = new RuntimeQuest(questData);
        QuestByID[quest.ID] = quest;
    }

    private static void MarkQuestAsComplete(string questId)
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

    public static void StartQuest(string questID)
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
        else
        {
            SBGDebug.LogError($"Quest with ID {questID} not found", "QuestManager");
        }
    }

    public static void StartQuest(int questID)
    {
        // Convert int to string for backwards compatibility
        StartQuest(questID.ToString());
    }

    public static bool IsQuestCompleted(string questID)
    {
        return completedQuests.ContainsKey(questID);
    }

    public static RuntimeQuest GetQuestByID(string questID)
    {
        QuestByID.TryGetValue(questID, out var quest);
        return quest;
    }

    public static RuntimeQuest[] GetQuestsByType(QuestType type)
    {
        return QuestByID.Values.Where(q => q.Type == type).ToArray();
    }

    public static RuntimeQuest[] GetActiveQuestsByType(QuestType type)
    {
        return activeQuests.Where(q => q.Type == type).ToArray();
    }

    public static RuntimeQuest[] GetCompletedQuestsByType(QuestType type)
    {
        return completedQuests.Values.Where(q => q.Type == type).ToArray();
    }

    public static float GetQuestCompletionPercentage(string questID)
    {
        if (QuestByID.TryGetValue(questID, out var quest))
        {
            if (quest.Objectives == null || quest.Objectives.Count == 0) return 0f;
            return (float)quest.Objectives.Count(o => o.IsCompleted) / quest.Objectives.Count * 100f;
        }
        return 0f;
    }

    public static void CompleteObjective(int objectiveID, ObjectiveStatus status = ObjectiveStatus.CompletedSuccess)
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

    public static void CompleteQuest(string questID, ObjectiveStatus status = ObjectiveStatus.CompletedSuccess)
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
    public static bool HasFailedObjectives(string questID)
    {
        if (QuestByID.TryGetValue(questID, out var quest))
        {
            return quest.Objectives?.Any(o => o.Status == ObjectiveStatus.CompletedFailure) ?? false;
        }
        return false;
    }

    public static IEnumerable<IObjective> GetFailedObjectives(string questID)
    {
        if (QuestByID.TryGetValue(questID, out var quest))
        {
            return quest.Objectives?.Where(o => o.Status == ObjectiveStatus.CompletedFailure) ?? Enumerable.Empty<IObjective>();
        }
        return Enumerable.Empty<IObjective>();
    }

    public static IEnumerable<IObjective> GetObjectivesByStatus(string questID, ObjectiveStatus status)
    {
        if (QuestByID.TryGetValue(questID, out var quest))
        {
            return quest.Objectives?.Where(o => o.Status == status) ?? Enumerable.Empty<IObjective>();
        }
        return Enumerable.Empty<IObjective>();
    }

    public static ObjectiveStatus GetQuestOverallStatus(string questID)
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

    private static void CheckIfAllObjectivesAreComplete(RuntimeQuest quest)
    {
        if (quest.Objectives == null) return;

        var allCompleted = quest.Objectives.All(obj => obj.IsCompleted);
        if (allCompleted)
        {
            quest.Complete();
        }
    }
}
