using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using GlobalEvents;

#region Dialogue Loader
/// <summary>
/// Responsible for loading and managing dialogue data.
/// </summary>
public static class DialogueLoader
{
    private const string DIALOGUE_FOLDER = "Dialogues";
    private static string currentLanguage = "english"; // Default language
    private static Dictionary<string, DialogueData> cachedDialogues = new Dictionary<string, DialogueData>();
    private static bool isInitialized = false;

    static DialogueLoader()
    {
        EnsureInitialized();
    }

    private static void EnsureInitialized()
    {
        if (isInitialized) return;

        string dialoguePath = Path.Combine(Application.streamingAssetsPath, DIALOGUE_FOLDER);
        if (!Directory.Exists(dialoguePath))
        {
            Directory.CreateDirectory(dialoguePath);
            SBGDebug.LogInfo($"Created dialogue directory at: {dialoguePath}", "DialogueLoader");
        }

        try
        {
            UpdateLanguage(GameMaster.Instance.GetSettings().language);

            ConfigEvents.LanguageSettingsChanged -= OnLanguageSettingsChanged; // Prevent double-subscription
            ConfigEvents.LanguageSettingsChanged += OnLanguageSettingsChanged;
        }
        catch (System.Exception e)
        {
            SBGDebug.LogWarning($"Could not initialize language from GameMaster: {e.Message}", "DialogueLoader");
            // Continue with default language
        }

        isInitialized = true;
    }

    #region Language
    private static void OnLanguageSettingsChanged()
    {
        if (GameMaster.Instance != null)
        {
            UpdateLanguage(GameMaster.Instance.GetSettings().language);
        }
    }

    private static void UpdateLanguage(Language language)
    {
        currentLanguage = language.ToString().ToLower();
        string langPath = Path.Combine(Application.streamingAssetsPath, DIALOGUE_FOLDER, currentLanguage);

        if (!Directory.Exists(langPath))
        {
            Directory.CreateDirectory(langPath);
            SBGDebug.LogInfo($"Created language directory at: {langPath}", "DialogueLoader");
        }

        // Clear cache when language changes
        cachedDialogues.Clear();
    }
    #endregion

    #region Public API
    public static DialogueData LoadDialogue(string dialogueId)
    {
        EnsureInitialized();

        // Check cache first
        if (cachedDialogues.TryGetValue(dialogueId, out DialogueData cachedDialogue))
        {
            SBGDebug.LogInfo($"Loading dialogue from cache: {dialogueId}", "DialogueLoader");
            return cachedDialogue;
        }

        string filePath = Path.Combine(Application.streamingAssetsPath, DIALOGUE_FOLDER, currentLanguage, $"{dialogueId}.json");

        if (!File.Exists(filePath))
        {
            SBGDebug.LogError($"Dialogue file not found: {filePath}", "DialogueLoader");
            return null;
        }

        try
        {
            // Load the file as a TextAsset
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string jsonContent = System.Text.Encoding.UTF8.GetString(fileBytes);
            TextAsset jsonAsset = new TextAsset(jsonContent);

            SBGDebug.LogInfo($"Loading dialogue file: {filePath} (graph format only)", "DialogueLoader");

            // Parse using our SimdJsonSharp parser
            var dialogues = DialogueJsonParser.ParseDialogueData(jsonAsset);

            if (dialogues == null)
            {
                SBGDebug.LogError("DialogueJsonParser returned null", "DialogueLoader");
                return null;
            }

            // Validate dialogue structure
            if (!ValidateDialogueData(dialogues))
            {
                SBGDebug.LogError($"Dialogue validation failed for {filePath}", "DialogueLoader");
                return null;
            }

            SBGDebug.LogInfo($"Parsed {dialogues.Length} dialogue(s)", "DialogueLoader");

            if (dialogues.Length > 0)
            {
                var dialogue = dialogues[0];

                if (dialogue.entries == null)
                {
                    SBGDebug.LogError("Parsed dialogue has null entries list", "DialogueLoader");
                    return null;
                }

                SBGDebug.LogInfo($"Dialogue '{dialogueId}' loaded with {dialogue.entries.Count} entries", "DialogueLoader");

                // Cache the result
                cachedDialogues[dialogueId] = dialogue;
                return dialogue;
            }

            SBGDebug.LogError($"No dialogues found in file: {dialogueId}", "DialogueLoader");
            return null;
        }
        catch (System.Exception e)
        {
            SBGDebug.LogException(e, "DialogueLoader");
            return null;
        }
    }

    public static void ClearCache()
    {
        cachedDialogues.Clear();
        SBGDebug.LogInfo("Dialogue cache cleared", "DialogueLoader");
    }
    #endregion

    #region Debugging
    /// <summary>
    /// Test method to debug dialogue loading issues
    /// </summary>
    private static bool ValidateDialogueData(DialogueData[] dialogues)
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            SBGDebug.LogError("No dialogues found in data", "DialogueLoader | ValidateDialogueData");
            return false;
        }

        foreach (var dialogue in dialogues)
        {
            if (dialogue.entries == null || dialogue.entries.Count == 0)
            {
                SBGDebug.LogError($"Dialogue {dialogue.dialogueId} has no entries", "DialogueLoader | ValidateDialogueData");
                return false;
            }

            // Verify all entries have valid nodeIds
            var nodeIds = new HashSet<string>();
            foreach (var entry in dialogue.entries)
            {
                if (string.IsNullOrEmpty(entry.nodeId))
                {
                    SBGDebug.LogError($"Entry in dialogue {dialogue.dialogueId} has no nodeId", "DialogueLoader | ValidateDialogueData");
                    return false;
                }

                if (!nodeIds.Add(entry.nodeId))
                {
                    SBGDebug.LogError($"Duplicate nodeId {entry.nodeId} found in dialogue {dialogue.dialogueId}", "DialogueLoader | ValidateDialogueData");
                    return false;
                }
            }

            // Verify choice node connections
            foreach (var entry in dialogue.entries)
            {
                if (entry is DialogueEntryChoice choiceEntry)
                {
                    if (choiceEntry.choices == null || choiceEntry.choices.Count == 0)
                    {
                        SBGDebug.LogError($"Choice node {entry.nodeId} in dialogue {dialogue.dialogueId} has no choices", "DialogueLoader | ValidateDialogueData");
                        return false;
                    }

                    // Verify each choice has either an outputNodeId or is an end node
                    foreach (var choice in choiceEntry.choices)
                    {
                        if (!string.IsNullOrEmpty(choice.outputNodeId) && !nodeIds.Contains(choice.outputNodeId))
                        {
                            SBGDebug.LogError($"Choice in node {entry.nodeId} references invalid outputNodeId {choice.outputNodeId}", "DialogueLoader | ValidateDialogueData");
                            return false;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(entry.outputNodeId) && !nodeIds.Contains(entry.outputNodeId))
                {
                    // Verify text node connections
                    SBGDebug.LogError($"Text node {entry.nodeId} references invalid outputNodeId {entry.outputNodeId}", "DialogueLoader | ValidateDialogueData");
                    return false;
                }
            }
        }

        return true;
    }
    #endregion

    /* public static void TestDialogueLoading(string dialogueId)
    {
        SBGDebug.LogInfo($"=== Testing dialogue loading for: {dialogueId} (graph format only) ===", "DialogueLoader");
        
        EnsureInitialized();
        
        string filePath = Path.Combine(Application.streamingAssetsPath, DIALOGUE_FOLDER, currentLanguage, $"{dialogueId}.json");
        SBGDebug.LogInfo($"File path: {filePath}", "DialogueLoader");
        SBGDebug.LogInfo($"File exists: {File.Exists(filePath)}", "DialogueLoader");
        
        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            SBGDebug.LogInfo($"JSON content length: {jsonContent.Length} characters", "DialogueLoader");
            SBGDebug.LogInfo($"JSON preview: {jsonContent.Substring(0, Math.Min(200, jsonContent.Length))}...", "DialogueLoader");
            
            try
            {
                TextAsset jsonAsset = new TextAsset(jsonContent);
                var result = DialogueJsonParser.ParseDialogueData(jsonAsset);
                
                if (result != null && result.Length > 0)
                {
                    SBGDebug.LogInfo($"Graph parsing successful! Got {result.Length} dialogue(s)", "DialogueLoader");
                    SBGDebug.LogInfo($"First dialogue ID: {result[0].dialogueId}", "DialogueLoader");
                    SBGDebug.LogInfo($"First dialogue entries count: {result[0].entries?.Count ?? 0}", "DialogueLoader");
                }
                else
                {
                    SBGDebug.LogError("Graph parsing failed - got null or empty result", "DialogueLoader");
                }
            }
            catch (Exception e)
            {
                SBGDebug.LogException(e, "DialogueLoader | TestDialogueLoading");
            }
        }
        
        SBGDebug.LogInfo("=== End dialogue loading test ===", "DialogueLoader");
    } */
}
#endregion