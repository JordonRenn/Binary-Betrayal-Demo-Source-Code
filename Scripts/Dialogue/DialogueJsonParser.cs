using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System;

public static unsafe class DialogueJsonParser
{
    public static DialogueData[] ParseDialogueData(TextAsset jsonTextAsset)
    {
        if (jsonTextAsset == null)
        {
            Debug.LogError("Dialogue JSON asset is null!");
            return new DialogueData[0];
        }

        string json = jsonTextAsset.text;
        SBGDebug.LogInfo($"Parsing JSON:\n{json}", "DialogueJsonParser");
        
        // Validate JSON first
        if (SimdJsonInterop.validate_json(json) != 1)
        {
            SBGDebug.LogError("Invalid JSON format", "DialogueJsonParser");
            return new DialogueData[0];
        }

        // Try to parse as single object since our dialogues are single objects
        var singleDialogue = new DialogueData();
        
        // Get dialogueId
        string dialogueId = SimdJsonInterop.GetString(json, "dialogueId");
        if (string.IsNullOrEmpty(dialogueId))
        {
            SBGDebug.LogError("Failed to parse dialogueId", "DialogueJsonParser");
            return new DialogueData[0];
        }
        singleDialogue.dialogueId = dialogueId;
        
        // Get entries array
        int entriesCount = SimdJsonInterop.get_array_length(json, "entries");
        SBGDebug.LogInfo($"Found {entriesCount} entries", "DialogueJsonParser");
        
        if (entriesCount <= 0)
        {
            SBGDebug.LogError("No entries found in dialogue", "DialogueJsonParser");
            return new DialogueData[0];
        }

        // Parse entries directly from the root object
        var entries = new List<DialogueEntry>();
        for (int i = 0; i < entriesCount; i++)
        {
            var entry = new DialogueEntry();
            entry.characterName = SimdJsonInterop.GetArrayString(json, "entries", i, "characterName");
            entry.avatarPath = SimdJsonInterop.GetArrayString(json, "entries", i, "avatarPath");
            entry.message = SimdJsonInterop.GetArrayString(json, "entries", i, "message");
            
            if (!string.IsNullOrEmpty(entry.characterName) && !string.IsNullOrEmpty(entry.message))
            {
                entries.Add(entry);
                SBGDebug.LogInfo($"Parsed entry {i}: {entry.characterName}: {entry.message}", "DialogueJsonParser");
            }
            else
            {
                SBGDebug.LogWarning($"Skipped invalid entry {i}", "DialogueJsonParser");
            }
        }
        
        if (entries.Count == 0)
        {
            SBGDebug.LogError("No valid entries parsed", "DialogueJsonParser");
            return new DialogueData[0];
        }
        
        singleDialogue.entries = entries;
        return new DialogueData[] { singleDialogue };
    }

    private static List<DialogueEntry> ParseEntries(string entriesJson)
    {
        var entries = new List<DialogueEntry>();
        if (string.IsNullOrEmpty(entriesJson)) return entries;
        int entryCount = SimdJsonInterop.get_array_length(entriesJson, "entries");
        for (int i = 0; i < entryCount; i++)
        {
            var entry = new DialogueEntry();
            entry.characterName = SimdJsonInterop.GetArrayString(entriesJson, "entries", i, "characterName");
            entry.avatarPath = SimdJsonInterop.GetArrayString(entriesJson, "entries", i, "avatarPath");
            entry.message = SimdJsonInterop.GetArrayString(entriesJson, "entries", i, "message");
            entries.Add(entry);
        }
        return entries;
    }
}