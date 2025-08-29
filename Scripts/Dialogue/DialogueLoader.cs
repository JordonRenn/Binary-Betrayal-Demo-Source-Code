using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

public class DialogueLoader : MonoBehaviour
{
    public static DialogueLoader Instance { get; private set; }

    private const string DIALOGUE_FOLDER = "Dialogues";
    private string currentLanguage;
    private Dictionary<string, DialogueData> cachedDialogues = new Dictionary<string, DialogueData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        string dialoguePath = Path.Combine(Application.streamingAssetsPath, DIALOGUE_FOLDER);
        if (!Directory.Exists(dialoguePath))
        {
            Directory.CreateDirectory(dialoguePath);
            SBGDebug.LogInfo($"Created dialogue directory at: {dialoguePath}", "DialogueLoader");
        }

        // Initialize with current language from GameMaster
        UpdateLanguage(GameMaster.Instance.GetSettings().language);

        // Subscribe to language changes
        GameMaster.Instance.gm_GameUnpaused.AddListener(() =>
        {
            UpdateLanguage(GameMaster.Instance.GetSettings().language);
        });
    }

    private void UpdateLanguage(Language language)
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

    public DialogueData LoadDialogue(string dialogueId)
    {
        // Check cache first
        if (cachedDialogues.TryGetValue(dialogueId, out DialogueData cachedDialogue))
        {
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
            TextAsset jsonAsset = new TextAsset(System.Text.Encoding.UTF8.GetString(fileBytes));
            
            // Parse using our SimdJsonSharp parser
            var dialogues = DialogueJsonParser.ParseDialogueData(jsonAsset);
            if (dialogues != null && dialogues.Length > 0)
            {
                // Cache the result
                cachedDialogues[dialogueId] = dialogues[0];
                return dialogues[0];
            }
            
            SBGDebug.LogError($"Failed to parse dialogue: {dialogueId}", "DialogueLoader");
            return null;
        }
        catch (System.Exception e)
        {
            SBGDebug.LogException(e, "DialogueLoader");
            return null;
        }
    }
    
    public void ClearCache()
    {
        cachedDialogues.Clear();
        SBGDebug.LogInfo("Dialogue cache cleared", "DialogueLoader");
    }
}

[Serializable]
public class DialogueData
{
    public string dialogueId;
    public List<DialogueEntry> entries;
}

[Serializable]
public class DialogueEntry
{
    public string characterName;
    public string avatarPath;
    public string message;
}