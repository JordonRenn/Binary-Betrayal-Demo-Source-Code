using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class for objects that can be tracked and interacted with in the game.
/// </summary>

namespace SBG
{
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
            if (isTrackable)
            {
                GameMaster.allSauceObjects.Add(this);
                GameMaster.allTrackableSauceObjects.Add(this);
                isSauceObjectInGameMaster = true;
            }
            else
            {
                GameMaster.allSauceObjects.Add(this);
                isSauceObjectInGameMaster = true;
            }
        }

        void OnDestroy()
        {
            if (isTrackable)
            {
                GameMaster.allTrackableSauceObjects.Remove(this);
            }
            GameMaster.allSauceObjects.Remove(this);
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
}