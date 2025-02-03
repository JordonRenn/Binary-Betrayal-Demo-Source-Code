using System.Collections;
using UnityEngine;

public class FPSS_PlayerCamController : MonoBehaviour
{
    public static FPSS_PlayerCamController Instance { get; private set; }
    
    [SerializeField] public Transform orientation;
    [SerializeField] public static float sensX = 50f;
    [SerializeField] public static float sensY = 50f;

    private Vector2 lookInput;
    float xRotation;
    float yRotation; 
    
    private float initDelay = 0.2f;    //used to pause execution between steps of initialization when needed
    private float initTimeout = 10f;   //initialization timeout
    private bool initialized = false;                   //flag used to stop Update() from running before initialization is complete

    private bool cursorLocked = true;
    private bool isOverridden = false;

    //private bool debugMode = false;            //Enable/Disable debug mode

    void Start()
    {
        StartCoroutine(Init());
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Instance = this;
    }

    IEnumerator Init()
    {
        yield return new WaitForSeconds(initDelay);

        yield return orientation = GameObject.FindWithTag("cam_Orientation").transform;

        initialized = true;
    }

    void Update()
    {
        if (!initialized || isOverridden) {return;}
        
        lookInput = FPS_InputHandler.Instance.LookInput;

        float mouseX = lookInput.x * Time.deltaTime * sensX;
        float mouseY = lookInput.y * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;

        //clamp how far up and down you can look
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //rotate using input values
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void ApplySpread(Vector2 spreadOffset)
    {
        xRotation -= spreadOffset.y;
        yRotation += spreadOffset.x;
    }

    public void ToggleCursorLock()
    {
        cursorLocked = !cursorLocked;
        Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !cursorLocked;
    }

    public void SetRotation(Vector3 rotation)
    {
        xRotation = rotation.x;
        yRotation = rotation.y;
    }

    public void AllowOverride(bool toggle)
    {
        isOverridden = toggle;
    }
}
