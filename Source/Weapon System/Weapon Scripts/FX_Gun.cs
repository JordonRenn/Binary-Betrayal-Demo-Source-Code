using UnityEngine;
using FMODUnity;

public class SFX_Gun : MonoBehaviour
{
    private WPO_Gun gun;
    private Transform pos;

    private EventReference sfx_ClipOut;
    private EventReference sfx_ClipIn;
    private EventReference sfx_Slide;
    private EventReference sfx_Grab;
    private EventReference sfx_Handling_1;
    private EventReference sfx_Handling_2;

    private void Awake()
    {
        gun = GetComponentInParent<WPO_Gun>();
        if (gun == null)
        {
            Debug.LogError("SFX_Gun: No WPO_Gun found in parent.");
        }
        
        // Initialize the position transform
        pos = transform;
    }

    private void Start()
    {
        if (gun != null)
        {
            sfx_ClipOut = gun.sfx_ClipOut;
            sfx_ClipIn = gun.sfx_ClipIn;
            sfx_Slide = gun.sfx_Slide;
            sfx_Grab = gun.sfx_Grab;
            sfx_Handling_1 = gun.sfx_Handling_1;
            sfx_Handling_2 = gun.sfx_Handling_2;
            
            // Preload all events to cache them
            PreloadEvents();
        }
        else
        {
            Debug.LogError("SFX_Gun: Cannot assign audio events because gun is null.");
        }
    }
    
    // Preload audio events to minimize lag when playing
    private void PreloadEvents()
    {
        // Force FMOD to load these events into memory before they're played
        if (!sfx_ClipIn.IsNull) RuntimeManager.CreateInstance(sfx_ClipIn);
        if (!sfx_ClipOut.IsNull) RuntimeManager.CreateInstance(sfx_ClipOut);
        if (!sfx_Slide.IsNull) RuntimeManager.CreateInstance(sfx_Slide);
        if (!sfx_Grab.IsNull) RuntimeManager.CreateInstance(sfx_Grab);
        if (!sfx_Handling_1.IsNull) RuntimeManager.CreateInstance(sfx_Handling_1);
        if (!sfx_Handling_2.IsNull) RuntimeManager.CreateInstance(sfx_Handling_2);
    }

    public void PlayClipInSFX()
    {
        if (!sfx_ClipIn.IsNull && pos != null)
        {
            RuntimeManager.PlayOneShotAttached(sfx_ClipIn, pos.gameObject);
        }
    }

    public void PlayClipOutSFX()
    {
        if (!sfx_ClipOut.IsNull && pos != null)
        {
            RuntimeManager.PlayOneShotAttached(sfx_ClipOut, pos.gameObject);
        }
    }

    public void PlaySlideSFX()
    {
        if (!sfx_Slide.IsNull && pos != null)
        {
            RuntimeManager.PlayOneShotAttached(sfx_Slide, pos.gameObject);
        }
    }

    public void PlayGrabSFX()
    {
        if (!sfx_Grab.IsNull && pos != null)
        {
            RuntimeManager.PlayOneShotAttached(sfx_Grab, pos.gameObject);
        }
    }

    public void PlayHandling1SFX()
    {
        if (!sfx_Handling_1.IsNull && pos != null)
        {
            RuntimeManager.PlayOneShotAttached(sfx_Handling_1, pos.gameObject);
        }
    }
    
    public void PlayHandling2SFX()
    {
        if (!sfx_Handling_2.IsNull && pos != null)
        {
            RuntimeManager.PlayOneShotAttached(sfx_Handling_2, pos.gameObject);
        }
    }
}
