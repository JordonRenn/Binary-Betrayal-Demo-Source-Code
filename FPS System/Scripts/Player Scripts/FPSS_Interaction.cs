using UnityEngine;

public class FPSS_Interaction : MonoBehaviour
{
    public static FPSS_Interaction Instance {get ; private set;}

    [SerializeField] Camera cam;
    private FPS_InputHandler input;
    //private FMOD_FPSS_PlayerAudio audio;
    private NotificationSystem notify;
    
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

    public void AttemptInteraction()
    {
        //Raycast from the center of the camera
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        //debug ray
        Debug.DrawRay(ray.origin, ray.direction * 3f, Color.red, 1f);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 3f))
        {
            //Check if the object has an interactable component
            FPSS_InteractableObject interactable = hit.collider.GetComponent<FPSS_InteractableObject>();

            if (interactable != null)
            {
                //Interact with the object
                interactable.AttemptInteract();
            }
        }
    }
}
