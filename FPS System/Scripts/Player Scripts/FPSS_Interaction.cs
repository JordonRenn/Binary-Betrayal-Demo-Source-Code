using UnityEngine;

public class FPSS_Interaction : MonoBehaviour
{
    public static FPSS_Interaction Instance {get ; private set;}

    [SerializeField] float interactionCooldown = 0.25f;
    [SerializeField] float reachDistance = 3f;
    private FPS_InputHandler input;
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
    }

    void Update() //why tf am i using the Update method??
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
            else
            {
                interactable = hit.collider.GetComponentInParent<Interactable>(); //Check parent object... might break something? idk maybe...
                if (interactable != null)
                {
                    interactable.Interact();
                }
                else
                {
                    //notify.Notify("Nothing to interact with");
                    Debug.Log("Nothing to interact with");
                }
            }
        }
    }
}
