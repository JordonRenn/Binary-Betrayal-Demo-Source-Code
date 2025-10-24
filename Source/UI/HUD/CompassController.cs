using UnityEngine;
using UnityEngine.UIElements;
using BinaryBetrayal.GlobalEvents;

namespace BinaryBetrayal.UI.HUD
{
    public class CompassController : MonoBehaviour
    {
        private GradientMask _compassMask;
        private GameObject playerObject;

        private void Awake()
        {
            _compassMask = GetComponent<UIDocument>().rootVisualElement.Q<GradientMask>("compass-texture");

            _compassMask.texture.wrapMode = TextureWrapMode.Repeat;

            LevelEvents.PlayerControllerInstantiated += GetPlayer;
        }

        private void Update()
        {
            if (playerObject == null || _compassMask == null) return;

            float uvOffset = playerObject.transform.eulerAngles.y / 360f;
            _compassMask.SetScrollOffset(uvOffset);
        }

        private void GetPlayer()
        {
            playerObject = GameObject.FindWithTag("Player");
            if (playerObject == null) Debug.LogWarning("CompassController: No player object found with Player tag!");
        }

        private void OnDestroy()
        {
            LevelEvents.PlayerControllerInstantiated -= GetPlayer;
        }
    }
}