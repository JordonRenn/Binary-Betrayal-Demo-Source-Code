using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

public static class DialogueLoader
{
    private const string DIALOGUE_FOLDER = "Dialogues";
    private static string currentLanguage = "english"; // Default language
    private static Dictionary<string, DialogueData> cachedDialogues = new Dictionary<string, DialogueData>();
    private static bool isInitialized = false;

    private static void EnsureInitialized()
    {
        if (isInitialized) return;
        
        string dialoguePath = Path.Combine(Application.streamingAssetsPath, DIALOGUE_FOLDER);
        if (!Directory.Exists(dialoguePath))
        {
            Directory.CreateDirectory(dialoguePath);
            SBGDebug.LogInfo($"Created dialogue directory at: {dialoguePath}", "DialogueLoader");
        }

        // Initialize with current language from GameMaster if available
        if (GameMaster.Instance != null)
        {
            try 
            {
                UpdateLanguage(GameMaster.Instance.GetSettings().language);
                
                // Subscribe to language changes
                GameMaster.Instance.gm_GameUnpaused.AddListener(() =>
                {
                    UpdateLanguage(GameMaster.Instance.GetSettings().language);
                });
            }
            catch (System.Exception e)
            {
                SBGDebug.LogWarning($"Could not initialize language from GameMaster: {e.Message}", "DialogueLoader");
                // Continue with default language
            }
        }
        
        isInitialized = true;
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

    /// <summary>
    /// Test method to debug dialogue loading issues
    /// </summary>
    public static void TestDialogueLoading(string dialogueId)
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
    }
}