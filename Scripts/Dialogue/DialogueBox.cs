using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;
using FMODUnity;

/* 
 * Place dialogue JSON files in the StreamingAssets/Dialogues/{language} folder
 * File names should match the dialogue ID

 * Avatar Placement:
 * 1. Place avatar sprites in Assets/Resources/Textures/avatars
 * 2. Reference avatars in dialogue files using just the character name in lowercase
 *    Example: "john" for Assets/Resources/Textures/avatars/john.png
 */

public class DialogueBox : MonoBehaviour
{
    public static DialogueBox Instance { get; private set; }
    private InputHandler input;

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

    private const string AVATAR_PATH = "Textures/avatars/";

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
        input = InputHandler.Instance;

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
            {
                Sprite avatarSprite = Resources.Load<Sprite>(AVATAR_PATH + entry.avatarPath);
                if (avatarSprite != null)
                {
                    avatarImage.sprite = avatarSprite;
                }
                else
                {
                    SBGDebug.LogWarning($"Could not load avatar sprite at path: {AVATAR_PATH + entry.avatarPath}", "DialogueBox");
                }
            }
            
            SBGDebug.LogInfo($"Displaying message by {entry.characterName}: {entry.message}", "DialogueBox");
        }
        else
        {
            //SBGDebug.LogInfo("No more messages in the dialogue queue.", "DialogueBox");
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
                //SBGDebug.LogInfo("Dialogue box opened with animation.", "DialogueBox");
                input.OnFocus_InteractInput.AddListener(NextDialogue);
                dialogueBoxOpened.Invoke();
                if (GameMaster.Instance != null)
                {
                    GameMaster.Instance.gm_DialogueStarted.Invoke();
                }
            });

        yield return new WaitForSeconds(0.5f);
        //SBGDebug.LogInfo("Dialogue box opened.", "DialogueBox");
    }

    //CLOSE DIALOGUE BOX
    public void CloseDialogueBox()
    {
        StartCoroutine(CloseBoxRoutine());
        //SBGDebug.LogInfo("Closing dialogue box...", "DialogueBox");
        ClearDialogueQueue();
    }
    
    private IEnumerator CloseBoxRoutine()
    {
        //UI_Master.Instance.ShowAllHUD(); //should probably create some checks to see if HUD actually needs to be shown

        ((RectTransform)dialogueBoxUI.transform).DOAnchorPos(new Vector2(0, -70), 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                input.OnFocus_InteractInput.RemoveListener(NextDialogue);
                dialogueBoxUI.SetActive(false);
                characterNameText.text = string.Empty;
                characterMessageText.text = string.Empty;
                avatarImage.sprite = null;
                //SBGDebug.LogInfo("Dialogue box closed with animation.", "DialogueBox");
                dialogueBoxClosed.Invoke();
                GameMaster.Instance?.gm_DialogueEnded?.Invoke();
            });

        yield return new WaitForSeconds(0.5f);
    }
    #endregion
}