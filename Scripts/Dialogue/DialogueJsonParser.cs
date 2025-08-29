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
        // Try to parse as array
        int dialogueCount = SimdJsonInterop.get_array_length(json, "");
        if (dialogueCount > 0)
        {
            var dialogueList = new List<DialogueData>(dialogueCount);
            for (int i = 0; i < dialogueCount; i++)
            {
                var dialogue = new DialogueData();
                dialogue.dialogueId = SimdJsonInterop.GetArrayString(json, "", i, "dialogueId");
                string entriesJson = SimdJsonInterop.GetArrayString(json, "", i, "entries");
                dialogue.entries = ParseEntries(entriesJson);
                dialogueList.Add(dialogue);
            }
            return dialogueList.ToArray();
        }
        // Try to parse as single object
        var singleDialogue = new DialogueData();
        singleDialogue.dialogueId = SimdJsonInterop.GetString(json, "dialogueId");
        string singleEntriesJson = SimdJsonInterop.GetString(json, "entries");
        singleDialogue.entries = ParseEntries(singleEntriesJson);
        return new DialogueData[] { singleDialogue };
    }

    private static List<DialogueEntry> ParseEntries(string entriesJson)
    {
        var entries = new List<DialogueEntry>();
        if (string.IsNullOrEmpty(entriesJson)) return entries;
        int entryCount = SimdJsonInterop.get_array_length(entriesJson, "");
        for (int i = 0; i < entryCount; i++)
        {
            var entry = new DialogueEntry();
            entry.characterName = SimdJsonInterop.GetArrayString(entriesJson, "", i, "characterName");
            entry.avatarPath = SimdJsonInterop.GetArrayString(entriesJson, "", i, "avatarPath");
            entry.message = SimdJsonInterop.GetArrayString(entriesJson, "", i, "message");
            entries.Add(entry);
        }
        return entries;
    }
}