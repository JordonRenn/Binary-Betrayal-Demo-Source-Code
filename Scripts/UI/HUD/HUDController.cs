using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;

public class HUDController : MonoBehaviour
{
    private UIDocument hudDocument;
    private NotificationController notificationController;
    private MiniMapController miniMapController;
    private CompassController compassController;
    private PlayerStatDisplayController playerStatDisplayController;
    private WeaponDisplayController weaponDisplayController;

    private void Awake()
    {
        // Initialize references to child controllers
        notificationController = GetComponent<NotificationController>();
        miniMapController = GetComponent<MiniMapController>();
        compassController = GetComponent<CompassController>();
        playerStatDisplayController = GetComponent<PlayerStatDisplayController>();
        weaponDisplayController = GetComponent<WeaponDisplayController>();

        // Get the UIDocument component
        hudDocument = GetComponent<UIDocument>();
    }

    public void HideAllHUD(bool hide)
    {
        hudDocument.rootVisualElement.style.display = hide ? DisplayStyle.None : DisplayStyle.Flex;
    }
}
