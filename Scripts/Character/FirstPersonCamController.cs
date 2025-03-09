using UnityEngine;
using System.Collections;

public class FirstPersonCamController : MonoBehaviour
{
    public static FirstPersonCamController Instance { get; private set; }

    [SerializeField] public GameObject playerObject;
    [SerializeField] public static float sensX = 50f;
    [SerializeField] public static float sensY = 50f;

    private Vector2 lookInput;
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

        float mouseX = lookInput.x * Time.deltaTime * sensX;
        float mouseY = lookInput.y * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;

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
}
