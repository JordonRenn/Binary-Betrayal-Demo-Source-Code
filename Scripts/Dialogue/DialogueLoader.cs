using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

public class DialogueLoader : MonoBehaviour
{
    public static DialogueLoader Instance { get; private set; }

    private const string DIALOGUE_FOLDER = "Dialogues";
    private string currentLanguage;

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
    }

    public DialogueData LoadDialogue(string dialogueId)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, DIALOGUE_FOLDER, currentLanguage, $"{dialogueId}.json");

        if (!File.Exists(filePath))
        {
            SBGDebug.LogError($"Dialogue file not found: {filePath}", "DialogueLoader");
            return null;
        }

        try
        {
            string jsonContent = File.ReadAllText(filePath);
            return JsonUtility.FromJson<DialogueData>(jsonContent);
        }
        catch (System.Exception e)
        {
            SBGDebug.LogException(e, "DialogueLoader");
            return null;
        }
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