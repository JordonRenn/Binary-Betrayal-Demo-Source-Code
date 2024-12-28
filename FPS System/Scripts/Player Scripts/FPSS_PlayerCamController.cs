using System.Collections;
using UnityEngine;

public class FPSS_PlayerCamController : MonoBehaviour
{
    [SerializeField] Transform orientation;
    [SerializeField] public static float sensX = 50f;
    [SerializeField] public static float sensY = 50f;

    private Vector2 lookInput;
    float xRotation;
    float yRotation; 

    private bool cursorLocked = false;

    //

    [Header("DEV OPTIONS")]
    [Space(10)]
    
    [SerializeField] private bool debugMode;            //Enable/Disable debug mode
    [SerializeField] private float initDelay = 0.2f;    //used to pause execution between steps of initialization when needed
    [SerializeField] private float initTimeout = 10f;   //initialization timeout
    private bool initialized = false;                   //flag used to stop Update() from running before initialization is complete

    void Start()
    {
        StartCoroutine(Init());
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    IEnumerator Init()
    {
        yield return new WaitForSeconds(initDelay);

        yield return orientation = GameObject.FindWithTag("cam_Orientation").transform;

        initialized = true;
    }

    void Update()
    {
        if (!initialized) {return;}
        
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
        
        //xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    }

    public void ToggleCursorLock()
    {
        cursorLocked = !cursorLocked;
        Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !cursorLocked;
    }
}
