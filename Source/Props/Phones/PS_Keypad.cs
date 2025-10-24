using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;
using FMOD.Studio;
using InputSystem = BinaryBetrayal.InputManagement.InputSystem;

public class PS_Keypad : MonoBehaviour
{
    [SerializeField] private PS_Main c_PhoneMain;
    [SerializeField] private Collider phoneHook;
    [SerializeField] private Collider NUM0;
    [SerializeField] private Collider NUM1;
    [SerializeField] private Collider NUM2;
    [SerializeField] private Collider NUM3;
    [SerializeField] private Collider NUM4;
    [SerializeField] private Collider NUM5;
    [SerializeField] private Collider NUM6;
    [SerializeField] private Collider NUM7;
    [SerializeField] private Collider NUM8;
    [SerializeField] private Collider NUM9;

    [Header("FMOD")]

    [SerializeField] public EventReference phoneDialSound;
    [SerializeField] public EventReference phoneButtonSound;
    [SerializeField] private EventInstance dialSoundInstance;
    [SerializeField] private EventReference phoneHookSound;


    private bool dialTonePlaying = false;

    private string phoneNumber = "";

    void Start()
    {
        InputSystem.OnClickDown_focus += OnMouseClick;
    }



    private void SubscribeToNumpadEvents()
    {
        InputSystem.OnNum0Down_focus += () => AddDigit("0");
        InputSystem.OnNum1Down_focus += () => AddDigit("1");
        InputSystem.OnNum2Down_focus += () => AddDigit("2");
        InputSystem.OnNum3Down_focus += () => AddDigit("3");
        InputSystem.OnNum4Down_focus += () => AddDigit("4");
        InputSystem.OnNum5Down_focus += () => AddDigit("5");
        InputSystem.OnNum6Down_focus += () => AddDigit("6");
        InputSystem.OnNum7Down_focus += () => AddDigit("7");
        InputSystem.OnNum8Down_focus += () => AddDigit("8");
        InputSystem.OnNum9Down_focus += () => AddDigit("9");
    }

    private void UnsubscribeFromNumpadEvents()
    {
        InputSystem.OnNum0Down_focus -= () => AddDigit("0");
        InputSystem.OnNum1Down_focus -= () => AddDigit("1");
        InputSystem.OnNum2Down_focus -= () => AddDigit("2");
        InputSystem.OnNum3Down_focus -= () => AddDigit("3");
        InputSystem.OnNum4Down_focus -= () => AddDigit("4");
        InputSystem.OnNum5Down_focus -= () => AddDigit("5");
        InputSystem.OnNum6Down_focus -= () => AddDigit("6");
        InputSystem.OnNum7Down_focus -= () => AddDigit("7");
        InputSystem.OnNum8Down_focus -= () => AddDigit("8");
        InputSystem.OnNum9Down_focus -= () => AddDigit("9");
    }

    private void OnMouseClick()
    {
        Debug.Log("Click");

        if (dialTonePlaying && dialSoundInstance.isValid())
        {
            dialSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            dialSoundInstance.release();
            dialTonePlaying = false;
        }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider == phoneHook)
            {
                ResetInput();
                //RuntimeManager.PlayOneShot(phoneHookSound, gameObject.transform.position);
            }
            else if (hit.collider == NUM0)
            {
                AddDigit("0");
                RuntimeManager.PlayOneShot(phoneButtonSound, gameObject.transform.position);
            }
            else if (hit.collider == NUM1)
            {
                AddDigit("1");
                RuntimeManager.PlayOneShot(phoneButtonSound, gameObject.transform.position);
            }
            else if (hit.collider == NUM2)
            {
                AddDigit("2");
                RuntimeManager.PlayOneShot(phoneButtonSound, gameObject.transform.position);
            }
            else if (hit.collider == NUM3)
            {
                AddDigit("3");
                RuntimeManager.PlayOneShot(phoneButtonSound, gameObject.transform.position);
            }
            else if (hit.collider == NUM4)
            {
                AddDigit("4");
                RuntimeManager.PlayOneShot(phoneButtonSound, gameObject.transform.position);
            }
            else if (hit.collider == NUM5)
            {
                AddDigit("5");
                RuntimeManager.PlayOneShot(phoneButtonSound, gameObject.transform.position);
            }
            else if (hit.collider == NUM6)
            {
                AddDigit("6");
                RuntimeManager.PlayOneShot(phoneButtonSound, gameObject.transform.position);
            }
            else if (hit.collider == NUM7)
            {
                AddDigit("7");
                RuntimeManager.PlayOneShot(phoneButtonSound, gameObject.transform.position);
            }
            else if (hit.collider == NUM8)
            {
                AddDigit("8");
                RuntimeManager.PlayOneShot(phoneButtonSound, gameObject.transform.position);
            }
            else if (hit.collider == NUM9)
            {
                AddDigit("9");
                RuntimeManager.PlayOneShot(phoneButtonSound, gameObject.transform.position);
            }
        }
    }

    private void AddDigit(string digit)
    {
        if (phoneNumber.Length < 7)
        {
            phoneNumber += digit;
            if (phoneNumber.Length == 7)
            {
                c_PhoneMain.AttemptCall(phoneNumber);
            }
        }
    }

    private void ResetInput()
    {
        phoneNumber = "";

        RuntimeManager.StudioSystem.getBus("bus:/Pay Phone", out FMOD.Studio.Bus phoneBank);
        phoneBank.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);  

        EnableKeys();
        if (!dialTonePlaying)
        {
            dialSoundInstance.start();
            dialTonePlaying = true;
        }
    }

    public void DisableKeys()
    {
        NUM0.enabled = false;
        NUM1.enabled = false;
        NUM2.enabled = false;
        NUM3.enabled = false;
        NUM4.enabled = false;
        NUM5.enabled = false;
        NUM6.enabled = false;
        NUM7.enabled = false;
        NUM8.enabled = false;
        NUM9.enabled = false;

        if (dialTonePlaying)
        {
            dialSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            dialTonePlaying = false;
        }
    }

    public void EnableKeys()
    {
        NUM0.enabled = true;
        NUM1.enabled = true;
        NUM2.enabled = true;
        NUM3.enabled = true;
        NUM4.enabled = true;
        NUM5.enabled = true;
        NUM6.enabled = true;
        NUM7.enabled = true;
        NUM8.enabled = true;
        NUM9.enabled = true;
    }

    private void OnEnable()
    {
        ResetInput();
        // Make sure any existing instance is cleaned up
        if (dialSoundInstance.isValid())
        {
            dialSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            dialSoundInstance.release();
        }
        dialSoundInstance = RuntimeManager.CreateInstance(phoneDialSound);
        dialSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject.transform));
        dialSoundInstance.start();
        dialTonePlaying = true;
        
        SubscribeToNumpadEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromNumpadEvents();

        if (dialTonePlaying)
        {
            dialSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            dialSoundInstance.release();
            dialTonePlaying = false;
        }
    }
    
    private void OnDestroy()
    {
        InputSystem.OnClickDown_focus -= OnMouseClick;
        UnsubscribeFromNumpadEvents();

        // Clean up FMOD instance when object is destroyed
        if (dialSoundInstance.isValid())
        {
            dialSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            dialSoundInstance.release();
        }
    }
}
