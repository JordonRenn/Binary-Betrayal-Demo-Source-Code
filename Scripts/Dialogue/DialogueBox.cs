using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;
using FMODUnity;

/* * DialogueBox.cs
 * This script manages the dialogue box UI and handles loading and displaying dialogues.
 * It uses a queue to manage dialogue messages and provides methods to open/close the dialogue box.

 * Place dialogue JSON files in the StreamingAssets/Dialogues folder
 * File names should match the dialogue ID
 * Load dialogues using DialogueLoader.LoadDialogue("dialogue_id");

 * Place avatar images in the Resources folder
 * Load avatars using Resources.Load<Sprite>("path_to_avatar");
 */

public class DialogueBox : MonoBehaviour
{
    public static DialogueBox Instance { get; private set; }
    private FPS_InputHandler input;

    [SerializeField] private GameObject dialogueBoxUI;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text characterMessageText;
    [SerializeField] private Image avatarImage;

    private DialogueLoader dialogueLoader;
    public Queue<DialogueEntry> dialogueQueue = new Queue<DialogueEntry>();

    //EVENTS
    [HideInInspector] public UnityEvent dialogueBoxOpened;
    [HideInInspector] public UnityEvent dialogueBoxClosed;
    [HideInInspector] public UnityEvent dialogueFinished;

    #region Initialization
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        dialogueLoader = DialogueLoader.Instance;
        input = FPS_InputHandler.Instance;

        if (dialogueBoxUI == null || characterNameText == null ||
            characterMessageText == null || avatarImage == null)
        {
            SBGDebug.LogError("Required UI components are missing!", "DialogueBox");
            enabled = false;
            return;
        }
    }
    #endregion

    #region Queue Management
    public void LoadDialogue(string dialogueId)
    {
        DialogueData data = dialogueLoader.LoadDialogue(dialogueId);
        if (data == null) return;

        ClearDialogueQueue();

        foreach (var entry in data.entries)
        {
            dialogueQueue.Enqueue(entry);
        }
    }

    public void NextDialogue()
    {
        if (dialogueQueue.Count > 0)
        {
            DialogueEntry entry = dialogueQueue.Dequeue();
            characterNameText.text = entry.characterName;
            characterMessageText.text = entry.message;
            if (!string.IsNullOrEmpty(entry.avatarPath))
                avatarImage.sprite = Resources.Load<Sprite>(entry.avatarPath);
                
            SBGDebug.LogInfo($"Displaying message by {entry.characterName}: {entry.message}", "DialogueBox");
        }
        else
        {
            SBGDebug.LogInfo("No more messages in the dialogue queue.", "DialogueBox");
            dialogueFinished.Invoke();
            CloseDialogueBox();
        }
    }

    public void ClearDialogueQueue()
    {
        dialogueQueue.Clear();
    }
    #endregion

    #region Open / Close
    //OPEN DIALOGUE BOX
    public void OpenDialogueBox()
    {
        StartCoroutine(OpenBoxRoutine());
    }

    private IEnumerator OpenBoxRoutine()
    {
        dialogueBoxUI.SetActive(true);
        //UI_Master.Instance.HideAllHUD();

        yield return new WaitForSeconds(0.1f);

        // Set initial position -- fail safe
        ((RectTransform)dialogueBoxUI.transform).DOAnchorPos(new Vector2(0, -70), 0f);

        
        NextDialogue(); // load first dialogue entry before showing the box

        // Animate to open position
        ((RectTransform)dialogueBoxUI.transform).DOAnchorPos(new Vector2(0, 70), 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                SBGDebug.LogInfo("Dialogue box opened with animation.", "DialogueBox");
                input.lint_InteractTriggered.AddListener(NextDialogue);
                dialogueBoxOpened.Invoke();
            });

        yield return new WaitForSeconds(0.5f);
        SBGDebug.LogInfo("Dialogue box opened.", "DialogueBox");
    }

    //CLOSE DIALOGUE BOX
    public void CloseDialogueBox()
    {
        StartCoroutine(CloseBoxRoutine());
        SBGDebug.LogInfo("Closing dialogue box...", "DialogueBox");
        ClearDialogueQueue();
    }
    
    private IEnumerator CloseBoxRoutine()
    {
        //UI_Master.Instance.ShowAllHUD(); //should probably create some checks to see if HUD actually needs to be shown

        ((RectTransform)dialogueBoxUI.transform).DOAnchorPos(new Vector2(0, -70), 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                input.lint_InteractTriggered.RemoveListener(NextDialogue);
                dialogueBoxUI.SetActive(false);
                characterNameText.text = string.Empty;
                characterMessageText.text = string.Empty;
                avatarImage.sprite = null;
                SBGDebug.LogInfo("Dialogue box closed with animation.", "DialogueBox");
                dialogueBoxClosed.Invoke();
            });

        yield return new WaitForSeconds(0.5f);
    }
    #endregion
}
