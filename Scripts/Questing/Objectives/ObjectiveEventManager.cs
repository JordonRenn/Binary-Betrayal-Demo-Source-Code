using System.Collections.Generic;
using UnityEngine;

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

public static class ObjectiveEventManager
{
    private static string validationLogFilePath = "Assets/Logs/ObjectiveValidationLog.txt";
    private static bool isInitialized = false;
    private static List<ObjectiveType> activeObjectiveTypes = new List<ObjectiveType>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        if (!isInitialized)
        {
            // Force static constructor to run
            var temp = validationLogFilePath;
            isInitialized = true;
            SBGDebug.LogInfo("ObjectiveEventManager initialized", "ObjectiveEventManager");
        }
    }

    static ObjectiveEventManager()
    {
        // Ensure log directory exists
        var directory = System.IO.Path.GetDirectoryName(validationLogFilePath);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        // Create file if it doesn't exist
        if (!System.IO.File.Exists(validationLogFilePath))
        {
            System.IO.File.WriteAllText(validationLogFilePath, "Objective Validation Log\n");
        }

        SubscribeToEvents();
        GetActiveObjectiveTypes();
    }

    static void SubscribeToEvents()
    {
        // Subscribe to GameMaster events for each objective type
        if (GameMaster.Instance != null)
        {
            // OLD EVENTS THE NEED REPLACED
            //GameMaster.Instance.objective_ItemUsed.AddListener(ValidateUseItemEvent);
            //GameMaster.Instance.objective_ExploredLocation.AddListener(ValidateExploreEvent);
            //GameMaster.Instance.objective_NPCKilled.AddListener(ValidateKillEvent);

            //UPDATED
            GameMaster.Instance.oe_ItemAdded.AddListener(ValidateItemAddEvent);
            GameMaster.Instance.oe_ItemRemoved.AddListener(ValidateItemRemoveEvent);
            GameMaster.Instance.oe_DoorLockEvent.AddListener(ValidateDoorLockEvent);
            GameMaster.Instance.oe_TalkEvent.AddListener(ValidateTalkEvent);
            GameMaster.Instance.oe_PhoneCallEvent.AddListener(ValidatePhoneCallEvent);
            GameMaster.Instance.oe_InteractionEvent.AddListener(ValidateInteractionEvent);
        }
        else
        {
            SBGDebug.LogError("GameMaster.Instance is null when trying to subscribe to events", "ObjectiveEventManager");
        }
    }
    
    static void GetActiveObjectiveTypes()
    {
        if (QuestManager.Instance != null)
        {
            activeObjectiveTypes.Clear();
            foreach (var quest in QuestManager.Instance.activeQuests)
            {
                if (quest.Objectives == null) continue;

                foreach (var objective in quest.Objectives)
                {
                    if (!activeObjectiveTypes.Contains(objective.Type))
                    {
                        activeObjectiveTypes.Add(objective.Type);
                    }
                }
            }
        }
        else
        {
            SBGDebug.LogError("QuestManager.Instance is null when trying to get active objective types", "ObjectiveEventManager");
        }
    }

    #region OBSOLETE METHODS

    static void ValidateKillEvent(string objectId, string context = "")
    {
        if (QuestManager.Instance == null) return;

        // Check all active objectives of type Kill
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.Objectives == null) continue;

            foreach (var objective in quest.Objectives)
            {
                if (objective.Type == ObjectiveType.Kill && !objective.IsCompleted)
                {
                    if (objective is Objective_Kill killObjective)
                    {
                        killObjective.OnEnemyKilled(objectId);
                        
                        if (objective.IsCompleted)
                        {
                            LogValidationResult($"Kill objective completed: {objective.Message} (Enemy: {objectId})");
                        }
                    }
                }
            }
        }
    }

    static void ValidateExploreEvent(string objectId, string context = "")
    {
        if (QuestManager.Instance == null) return;

        // Check all active objectives of type Explore
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.Objectives == null) continue;

            foreach (var objective in quest.Objectives)
            {
                if (objective.Type == ObjectiveType.Explore && !objective.IsCompleted)
                {
                    if (objective is Objective_Explore exploreObjective)
                    {
                        exploreObjective.AreaExplored(objectId);
                        
                        if (objective.IsCompleted)
                        {
                            LogValidationResult($"Explore objective completed: {objective.Message} (Area: {objectId})");
                        }
                    }
                }
            }
        }
    }

    static void ValidateUseItemEvent(string objectId, string context = "")
    {
        if (QuestManager.Instance == null) return;

        // Check all active objectives of type UseItem
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.Objectives == null) continue;

            foreach (var objective in quest.Objectives)
            {
                if (objective.Type == ObjectiveType.UseItem && !objective.IsCompleted)
                {
                    if (objective is Objective_UseItem useItemObjective)
                    {
                        useItemObjective.ItemUsed(objectId);
                        
                        if (objective.IsCompleted)
                        {
                            LogValidationResult($"Use item objective completed: {objective.Message} (Item: {objectId})");
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region NEW METHODS

    /// <summary>
    /// Validates an interaction event and updates relevant objectives.
    /// </summary>
    /// <param name="objectID"></param>
    static void ValidateInteractionEvent(string objectID)
    {
        if (QuestManager.Instance == null) return;

        // NEED TO UPDATE OBJECTIVES BEFORE IMPLEMENTATION
    }

    /// <summary>
    /// Validates a phone call event and updates relevant objectives.  
    /// </summary>
    /// <param name="phoneId"></param>
    /// <param name="number"></param>
    /// <param name="callEvent"></param>
    static void ValidatePhoneCallEvent(string phoneId, string number, PhoneCallEvent callEvent)
    {
        if (QuestManager.Instance == null) return;

        // NEED TO UPDATE OBJECTIVES BEFORE IMPLEMENTATION
    }

    /// <summary>
    /// Validates a door lock event and updates relevant objectives.
    /// </summary>
    /// <param name="objectId"></param>
    /// <param name="lockState"></param>
    static void ValidateDoorLockEvent(string objectId, DoorLockState lockState)
    {
        if (QuestManager.Instance == null) return;

        // NEED TO UPDATE OBJECTIVES BEFORE IMPLEMENTATION
    }

    /// <summary>
    /// Validates a dialogue event and updates relevant objectives.
    /// </summary>
    /// <param name="dialogueId"></param>
    static void ValidateTalkEvent(string dialogueId)
    {
        if (QuestManager.Instance == null) return;

        // NEED TO UPDATE OBJECTIVES BEFORE IMPLEMENTATION
    }

    /// <summary>
    /// Validates an item add event and updates relevant objectives.
    /// </summary>
    /// <param name="inventoryType"></param>
    /// <param name="itemId"></param>
    /// <param name="inventoryName"></param>
    static void ValidateItemAddEvent(InventoryType inventoryType, string itemId, string inventoryName)
    {
        if (QuestManager.Instance == null) return;

        // NEED TO UPDATE OBJECTIVES BEFORE IMPLEMENTATION
    }

    /// <summary>
    /// Validates an item remove event and updates relevant objectives.
    /// </summary>
    /// <param name="inventoryType"></param>
    /// <param name="itemId"></param>
    /// <param name="inventoryName"></param>
    static void ValidateItemRemoveEvent(InventoryType inventoryType, string itemId, string inventoryName)
    {
        if (QuestManager.Instance == null) return;

        // NEED TO UPDATE OBJECTIVES BEFORE IMPLEMENTATION
    }
    #endregion


    /// <summary>
    /// Called when an objective is completed.
    /// </summary>
    private static void OnObjectiveCompletion()
    {
        GetActiveObjectiveTypes();

    }

    /// <summary>
    /// Logs the result of a validation check.
    /// </summary>
    /// <param name="message"></param>
    public static void LogValidationResult(string message)
    {
        System.IO.File.AppendAllText(validationLogFilePath, $"{System.DateTime.Now}: {message}\n");
    }
}