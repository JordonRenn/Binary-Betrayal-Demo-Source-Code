using UnityEngine;

public class FPSS_Interaction : MonoBehaviour
{
    public static FPSS_Interaction Instance {get ; private set;}

    //[SerializeField] Camera cam;
    [SerializeField] float interactionCooldown = 0.25f;
    [SerializeField] float reachDistance = 3f;
    private FPS_InputHandler input;
    //private FMOD_FPSS_PlayerAudio audio;
    private NotificationSystem notify;
    private float lastInteractionTime = 0f;
    
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
        //audio = FMOD_FPSS_PlayerAudio.Instance;
        notify = NotificationSystem.Instance;
    }

    void Update()
    {
        if (input.InteractInput && Time.time >= lastInteractionTime + interactionCooldown)
        {
            AttemptInteraction();
            lastInteractionTime = Time.time;
        }
    }

    public void AttemptInteraction()
    {
        //Raycast from the center of the camera
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        //debug ray
        Debug.DrawRay(ray.origin, ray.direction * 3f, Color.red, 1f);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, reachDistance))
        {
            //Check if the object has an interactable component
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null)
            {
                //Interact with the object
                interactable.Interact();
            }
        }
    }
}
