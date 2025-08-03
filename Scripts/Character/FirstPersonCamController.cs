using UnityEngine;
using System.Collections;

public class FirstPersonCamController : MonoBehaviour
{
    public static FirstPersonCamController Instance { get; private set; }

    [SerializeField] public GameObject playerObject;

    [Header("Look Settings")]
    private float baseSensitivity = 50f;  // Base sensitivity value
    private float sensitivityX;           // Will be calculated from settings
    private float sensitivityY;           // Will be calculated from settings
    private bool invertY;                 // Will be set from settings

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
        
        // Apply initial settings
        UpdateFromSettings();
        
        // Subscribe to settings change event
        GameMaster.Instance.gm_SettingsChanged.AddListener(UpdateFromSettings);
        
        initialized = true;
    }

    private void UpdateFromSettings()
    {
        if (GameMaster.Instance != null)
        {
            var settings = GameMaster.Instance.GetSettings();
            sensitivityX = baseSensitivity * settings.GetHorizontalSensitivityMultiplier();
            sensitivityY = baseSensitivity * settings.GetVerticalSensitivityMultiplier();
            invertY = settings.invertYAxis;
        }
    }

    void Update()
    {
        if (!initialized || isOverridden) {return;}
        
        lookInput = FPS_InputHandler.Instance.LookInput;
        
        // Direct mouse-to-view conversion, scaled by sensitivity and deltaTime
        yRotation += lookInput.x * sensitivityX * Time.deltaTime;
        float verticalInput = invertY ? lookInput.y : -lookInput.y;
        xRotation += verticalInput * sensitivityY * Time.deltaTime;

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

    void OnDestroy()
    {
        if (GameMaster.Instance != null)
        {
            GameMaster.Instance.gm_SettingsChanged.RemoveListener(UpdateFromSettings);
        }
    }
}
