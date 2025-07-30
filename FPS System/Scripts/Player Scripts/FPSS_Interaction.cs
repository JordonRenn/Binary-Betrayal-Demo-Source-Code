using UnityEngine;
using TMPro;

/// <summary>
/// Handles player interaction with objects in the game world through raycasting.
/// </summary>
public class FPSS_Interaction : MonoBehaviour
{
    public static FPSS_Interaction Instance { get; private set; }

    [SerializeField] float interactionCooldown = 0.25f;
    [SerializeField] float reachDistance = 3f;
    private FPS_InputHandler input;
    private TMP_Text objctInfoText; 
    private float lastInteractionTime = 0f;

    private Ray ray;
    private RaycastHit hitInfo;

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
        input = FPS_InputHandler.Instance;
        objctInfoText = FPSS_ReticleSystem.Instance.objectInfoText;
    }

    /// <summary>
    /// Updates the interaction system each frame, handling input and hover states.
    /// </summary>
    void Update() 
    {
        ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (input.InteractInput && Time.time >= lastInteractionTime + interactionCooldown)
        {
            AttemptInteraction();
            lastInteractionTime = Time.time;
        }
        else if (objctInfoText == null)
        {
            objctInfoText = FPSS_ReticleSystem.Instance.objectInfoText;
        }
        else
        {
            InteractHover();
        }
    }

    /// <summary>
    /// Performs a raycast to detect interactable objects in front of the player.
    /// </summary>
    /// <returns>The Interactable component if found, null otherwise.</returns>
    private Interactable GetInteractableFromRaycast()
    {
        if (Physics.Raycast(ray, out hitInfo, reachDistance))
        {
            #if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * reachDistance, Color.red, 0.1f);
            #endif

            Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
            return interactable ?? hitInfo.collider.GetComponentInParent<Interactable>();
        }
        return null;
    }

    /// <summary>
    /// Attempts to interact with an object in front of the player.
    /// Called when the interaction input is triggered.
    /// </summary>
    public void AttemptInteraction()
    {
        Interactable interactable = GetInteractableFromRaycast();
        if (interactable != null)
        {
            interactable.Interact();
        }
        else
        {
            Debug.Log("Nothing to interact with");
        }
    }

    /// <summary>
    /// Handles the hover state when looking at interactable objects.
    /// Updates the UI with object information.
    /// </summary>
    private void InteractHover()
    {
        Interactable interactable = GetInteractableFromRaycast();
        if (interactable != null)
        {
            ShowObjectInfo(interactable);
        }
        else
        {
            objctInfoText.text = "";
            objctInfoText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Updates the UI with information about the currently hovered interactable object.
    /// </summary>
    /// <param name="interactable">The interactable object to show information for.</param>
    private void ShowObjectInfo(Interactable interactable)
    {
        if (objctInfoText != null)
        {
            objctInfoText.text = interactable.ShowObjectInfo();
            objctInfoText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Object Info Text is not assigned in FPSS_Interaction.");
            objctInfoText.text = "Interact";
        }
    }
}
