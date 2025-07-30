using UnityEngine;
using System.Collections;

public class FirstPersonCamController : MonoBehaviour
{
    public static FirstPersonCamController Instance { get; private set; }

    [SerializeField] public GameObject playerObject;
    [SerializeField] public static float sensX = 50f;
    [SerializeField] public static float sensY = 50f;
    [SerializeField] private float smoothTime = 0.05f; // Smoothing time for camera movement
    [SerializeField] private bool useSmoothing = true; // Toggle for enabling/disabling smoothing

    private Vector2 lookInput;
    private Vector2 currentLookVelocity; // For SmoothDamp calculation
    private Vector2 currentLook; // Smoothed look values
    float xRotation;
    float yRotation; 
    
    private float initDelay = 0.25f;    //used to pause execution between steps of initialization when needed
    private bool initialized = false;                   //flag used to stop Update() from running before initialization is complete

    private bool isOverridden = false;

    //private bool debugMode = false;            //Enable/Disable debug mode

    void Awake()
    {
        Debug.Log("FIRST PERSON CAM CONTROLLER | Instantiated");
        
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    void Start()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        yield return new WaitForSeconds(initDelay);

        initialized = true;
    }

    void Update()
    {
        if (!initialized || isOverridden) {return;}
        
        lookInput = FPS_InputHandler.Instance.LookInput;

        float mouseX = lookInput.x * sensX;
        float mouseY = lookInput.y * sensY;

        // Apply smoothing if enabled
        if (useSmoothing)
        {
            // Use SmoothDamp for interpolated movement
            currentLook.x = Mathf.SmoothDamp(currentLook.x, mouseX, ref currentLookVelocity.x, smoothTime);
            currentLook.y = Mathf.SmoothDamp(currentLook.y, mouseY, ref currentLookVelocity.y, smoothTime);
            
            yRotation += currentLook.x * Time.deltaTime;
            xRotation -= currentLook.y * Time.deltaTime;
        }
        else
        {
            // Original direct input method
            yRotation += mouseX * Time.deltaTime;
            xRotation -= mouseY * Time.deltaTime;
        }

        //clamp how far up and down you can look
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //rotate using input values
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        playerObject.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void ApplySpread(Vector2 spreadOffset)
    {
        xRotation -= spreadOffset.x;
        yRotation += spreadOffset.y;
    }

    public void SetRotation(Vector3 rotation)
    {
        xRotation = rotation.x;
        yRotation = rotation.y;
    }

    public void AllowOverride(bool allow)
    {
        isOverridden = allow;
    }

    // Method to adjust smoothing amount at runtime
    public void SetSmoothingAmount(float amount)
    {
        smoothTime = Mathf.Clamp(amount, 0.01f, 0.5f);
    }

    // Method to toggle smoothing on/off
    public void ToggleSmoothing(bool enabled)
    {
        useSmoothing = enabled;
    }
}
