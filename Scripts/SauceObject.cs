// BUILT AS REPLACEMENT FOR "Trackable.cs" & "Interactable.cs" ******
// This class combines properties and methods from both classes to streamline functionality.
// Potential to act as base class for most dynamic objects in the game.


using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class for objects that can be tracked and interacted with in the game.
/// </summary>

// Class has Custom Editor script ***
public class SauceObject : MonoBehaviour
{
    [SerializeField] public string objectID;
    [SerializeField] public string objectDisplayName;

    [Space(10)]
    [SerializeField] private bool isTrackable = false;

    //#region Tracking
    [Header("Trackable Properties")]
    [Space(10)]
    [SerializeField] public Sprite nav_CompassIcon;
    [HideInInspector] public Image compassImage;
    [SerializeField] public float nav_CompassDrawDistance;

    [SerializeField] public TrackableIconColorState iconColorState = TrackableIconColorState.Default;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color lockedColor = Color.grey;
    [SerializeField] private Color highlightedColor = Color.red;

    private bool isSauceObjectInGameMaster = false;
    //#endregion

    [HideInInspector]
    public Vector2 position
    {
        get
        {
            return new Vector2(transform.position.x, transform.position.z);
        }
    }

    void OnEnable()
    {
        if (GameMaster.Instance != null)
        {
            if (isTrackable)
            {
                GameMaster.Instance.allSauceObjects.Add(this);
                GameMaster.Instance.allTrackableSauceObjects.Add(this);
                isSauceObjectInGameMaster = true;
            }
            else
            {
                GameMaster.Instance.allSauceObjects.Add(this);
                isSauceObjectInGameMaster = true;
            }
        }
        else
        {
            isSauceObjectInGameMaster = false;
            SBGDebug.LogWarning("GameMaster instance not found. SauceObject will not be registered.", $"class: SauceObject | {gameObject.name}");
        }
    }

    void OnDestroy()
    {
        /* RemoveFromNavCompass(); */
        if (GameMaster.Instance != null && isSauceObjectInGameMaster)
        {
            if (isTrackable)
            {
                GameMaster.Instance.allTrackableSauceObjects.Remove(this);
            }
            GameMaster.Instance.allSauceObjects.Remove(this);
        }
    }

    #region Nav Tracking
    protected void UpdateNavIconColor(TrackableIconColorState colorState)
    {
        if (!isTrackable) return;

        iconColorState = colorState;
    }

    protected void RemoveFromNavCompass()
    {
        /* if (NavCompass.Instance != null)
        {
            NavCompass.Instance.RemoveCompassMarker(this);
        } */
    }
    #endregion

    #region Interactable
    public virtual void Interact()
    {
        //override this method in inherited classes 
    }

    public string GetObjectDisplayName()
    {
        return objectDisplayName;
    }
    #endregion
}

public enum TrackableIconColorState
{
    Default,
    Locked,
    Highlighted
}