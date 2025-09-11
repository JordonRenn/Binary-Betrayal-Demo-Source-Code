using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;

public class CompassController : MonoBehaviour
{
    private GradientMask _compassMask;
    private GameObject playerObject;

    private void Awake()
    {
        _compassMask = GetComponent<UIDocument>().rootVisualElement.Q<GradientMask>("compass-texture");

        _compassMask.texture.wrapMode = TextureWrapMode.Repeat;

        GameMaster.Instance.gm_PlayerSpawned.AddListener(GetPlayer);
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
        GameMaster.Instance?.gm_PlayerSpawned?.RemoveListener(GetPlayer);
    }
}