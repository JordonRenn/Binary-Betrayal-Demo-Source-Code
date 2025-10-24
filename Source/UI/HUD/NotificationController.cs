using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using System.Linq;
using BinaryBetrayal.GlobalEvents;
using SBG.InventorySystem;

namespace BinaryBetrayal.UI.HUD
{
    public class NotificationController : MonoBehaviour
    {
        private VisualElement root;
        private VisualElement notificationPanel;
        private Label overflowLabel; // hidden by default, shown when too many notifications "+{n} more"

        private const int maxVisibleNotifications = 6;
        private const string ELEMENT_NAME_NOTIFICATION = "NotificationContainer";
        private const string ELEMENT_NAME_OVERFLOW = "NotificationOverFlowLabel";

        private const string ELEMENT_CLASS_NOTIFICATION_LABEL = "notification__label";

        void Start()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            notificationPanel = root.Q<VisualElement>(ELEMENT_NAME_NOTIFICATION);
            overflowLabel = root.Q<Label>(ELEMENT_NAME_OVERFLOW);

            InventoryEvents.ItemAdded += OnItemAdded;
            InventoryEvents.ItemRemoved += OnItemRemoved;
        }

        void OnDestroy()
        {
            InventoryEvents.ItemAdded -= OnItemAdded;
            InventoryEvents.ItemRemoved -= OnItemRemoved;
        }

        private void OnItemAdded(InventoryType inventoryType, string itemId, string itemName)
        {
            if (inventoryType != InventoryType.Player) return;

            var notification = new NotificationLabel($"Added {itemName} to Inventory");

            AddNotification(notification);
        }

        private void OnItemRemoved(InventoryType inventoryType, string itemId, string itemName)
        {
            if (inventoryType != InventoryType.Player) return;

            var notification = new NotificationLabel($"Removed {itemName} from Inventory");

            AddNotification(notification);
        }

        private void AddNotification(NotificationLabel message)
        {
            message.AddToClassList(ELEMENT_CLASS_NOTIFICATION_LABEL);
            notificationPanel.Add(message);

            // Handle overflow
            if (notificationPanel.childCount > maxVisibleNotifications)
            {
                overflowLabel.text = $"+{notificationPanel.childCount - maxVisibleNotifications} more";
                overflowLabel.style.display = DisplayStyle.Flex;

                // Remove oldest notification
                if (notificationPanel.childCount > 0)
                {
                    var oldestNotification = notificationPanel.Children().First();
                    oldestNotification?.RemoveFromHierarchy();
                }
            }
            else
            {
                overflowLabel.style.display = DisplayStyle.None;
            }
        }
    }

    public class NotificationLabel : Label
    {
        public NotificationLabel(string text) : base(text)
        {
            this.text = text;
            FadeOutAndRemove();
        }

        private void FadeOutAndRemove()
        {
            DOVirtual.DelayedCall(3f, () =>
            {
                var initialColor = this.style.color.value;
                DOTween.To(() => initialColor, x => this.style.color = new StyleColor(x), new Color(initialColor.r, initialColor.g, initialColor.b, 0f), 3f)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() => RemoveFromHierarchy());
            });

        }
    }
}