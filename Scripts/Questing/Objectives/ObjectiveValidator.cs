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
 */

 /*
CONTEXT NEEDED FOR EACH OBJECTIVE TYPE:
- Talk:         NPC's objectId,         context?
- Collect:      Item's objectId,        context?
- DoorLock:     Door's objectId,        Key ID
- PhoneCall:    Phone's objectId,       phoneNumber
- Kill:         Enemy's objectId,       context?
- Explore:      Area's objectId,        context?
- UseItem:      Item's objectId,        context?
- Interact:     Object's objectId,      context?
 */

public static class ObjectiveValidator
{
    // subscribes to relevant events in GameMaster and checks them against current objectives
    // this is used to validate if an objective has been completed or not

    // text file used for logging validated objectives (maybe csv or other format may be better?)
    private static string validationLogFilePath = "Assets/Logs/ObjectiveValidationLog.txt";
    private static bool isInitialized = false;

    // Initialize method to ensure the validator is set up
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        if (!isInitialized)
        {
            // Force static constructor to run
            var temp = validationLogFilePath;
            isInitialized = true;
            SBGDebug.LogInfo("ObjectiveValidator initialized", "ObjectiveValidator");
        }
    }

    static ObjectiveValidator()
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
    }

    static void SubscribeToEvents()
    {
        // Subscribe to GameMaster events for each objective type
        if (GameMaster.Instance != null)
        {
            GameMaster.Instance.objective_DoorLocked.AddListener(ValidateDoorLockEvent);
            GameMaster.Instance.objective_DoorUnlocked.AddListener(ValidateDoorLockEvent);
            GameMaster.Instance.objective_ItemUsed.AddListener(ValidateUseItemEvent);
            GameMaster.Instance.objective_InteractableInteracted.AddListener(ValidateInteractEvent);
            GameMaster.Instance.objective_ItemCollected.AddListener(ValidateCollectEvent);
            GameMaster.Instance.objective_NPCTalkedTo.AddListener(ValidateTalkEvent);
            GameMaster.Instance.objective_PhoneCallMade.AddListener(ValidatePhoneCallEvent);
            GameMaster.Instance.objective_PhoneCallAnswered.AddListener(ValidatePhoneCallEvent);
            GameMaster.Instance.objective_ExploredLocation.AddListener(ValidateExploreEvent);
            GameMaster.Instance.objective_NPCKilled.AddListener(ValidateKillEvent);
        }
        else
        {
            SBGDebug.LogError("GameMaster.Instance is null when trying to subscribe to events", "ObjectiveValidator");
        }
    }

    static void ValidateTalkEvent(string objectId, string context = "")
    {
        if (QuestManager.Instance == null) return;

        // Check all active objectives of type Talk
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.Objectives == null) continue;

            foreach (var objective in quest.Objectives)
            {
                if (objective.Type == ObjectiveType.Talk && !objective.IsCompleted)
                {
                    if (objective is Objective_Talk talkObjective)
                    {
                        // For talk objectives, we might need to check dialogue ID instead of objectId
                        // This depends on how you implement it - you might pass dialogueID as context
                        talkObjective.OnDialogueCompleted(string.IsNullOrEmpty(context) ? objectId : context);

                        if (objective.IsCompleted)
                        {
                            LogValidationResult($"Talk objective completed: {objective.Message} (ObjectId: {objectId}, Context: {context})");
                        }
                    }
                }
            }
        }
    }

    static void ValidateCollectEvent(string objectId, string context = "")
    {
        if (QuestManager.Instance == null) return;

        // Check all active objectives of type Collect
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.Objectives == null) continue;

            foreach (var objective in quest.Objectives)
            {
                if (objective.Type == ObjectiveType.Collect && !objective.IsCompleted)
                {
                    if (objective is Objective_Collect collectObjective)
                    {
                        collectObjective.OnItemCollected(objectId);
                        
                        if (objective.IsCompleted)
                        {
                            LogValidationResult($"Collect objective completed: {objective.Message} (Item: {objectId})");
                        }
                    }
                }
            }
        }
    }

    static void ValidateDoorLockEvent(string objectId, string context = "")
    {
        if (QuestManager.Instance == null) return;

        // Check all active objectives of type DoorLock
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.Objectives == null) continue;

            foreach (var objective in quest.Objectives)
            {
                if (objective.Type == ObjectiveType.DoorLock && !objective.IsCompleted)
                {
                    if (objective is Objective_DoorLock doorObjective)
                    {
                        doorObjective.DoorUnlocked(objectId);
                        
                        if (objective.IsCompleted)
                        {
                            LogValidationResult($"Door unlock objective completed: {objective.Message} (Door: {objectId}, Key: {context})");
                        }
                    }
                }
            }
        }
    }

    static void ValidatePhoneCallEvent(string objectId, string context = "")
    {
        if (QuestManager.Instance == null) return;

        // Check all active objectives of type PhoneCall
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.Objectives == null) continue;

            foreach (var objective in quest.Objectives)
            {
                if (objective.Type == ObjectiveType.PhoneCall && !objective.IsCompleted)
                {
                    if (objective is Objective_PhoneCall phoneObjective)
                    {
                        // Use context as phone number if provided, otherwise use objectId
                        string phoneNumber = string.IsNullOrEmpty(context) ? objectId : context;
                        phoneObjective.CallMade(phoneNumber);
                        
                        if (objective.IsCompleted)
                        {
                            LogValidationResult($"Phone call objective completed: {objective.Message} (Number: {phoneNumber})");
                        }
                    }
                }
            }
        }
    }

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

    static void ValidateInteractEvent(string objectId, string context = "")
    {
        if (QuestManager.Instance == null) return;

        // Check all active objectives of type Interact
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.Objectives == null) continue;

            foreach (var objective in quest.Objectives)
            {
                if (objective.Type == ObjectiveType.Interact && !objective.IsCompleted)
                {
                    if (objective is Objective_Interact interactObjective)
                    {
                        interactObjective.OnInteractionComplete(objectId);
                        
                        if (objective.IsCompleted)
                        {
                            LogValidationResult($"Interact objective completed: {objective.Message} (Object: {objectId})");
                        }
                    }
                }
            }
        }
    }

    public static void LogValidationResult(string message)
    {
        // Append the validation result to the log file
        System.IO.File.AppendAllText(validationLogFilePath, $"{System.DateTime.Now}: {message}\n");
    }
}