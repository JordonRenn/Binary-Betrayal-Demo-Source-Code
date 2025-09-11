using System;
using UnityEngine.UIElements;

/* 
- Simplified version for testing
- shortcutKey text displays within a visual element with a border to imitate a key/button
- Intended to be a menu shortcut with icon and label, adapting to input type (PC/gamepad)
 */

[UxmlElement]
public partial class MenuShortcut : VisualElement
{
    [UxmlAttribute] public string menuName 
    { 
        get => _menuName;
        set
        {
            _menuName = value;
            if (_label != null)
            {
                _label.text = value;
            }
        }
    }
    [UxmlAttribute] public string shortcutKey
    { 
        get => _shortcutKey;
        set
        {
            _shortcutKey = value;
            if (_shortcutIcon != null)
            {
                _shortcutIcon.iconKey = value;
            }
        }
    }

    private string _shortcutKey = "";
    private string _menuName = "";
    private Label _label;
    private ShortcutIcon _shortcutIcon;

    private enum InputType
    {
        PC,
        Gamepad
    }

    public MenuShortcut()
    {
        this.AddToClassList("menu-shortcut");
        this.name = "MenuShortcut";

        _label = new Label();
        _label.text = menuName;
        _label.name = "MenuShortcut-Label";
        _label.AddToClassList("menu-shortcut__label");
        Add(_label);

        _shortcutIcon = new ShortcutIcon();
        _shortcutIcon.iconKey = shortcutKey;
        _shortcutIcon.name = "MenuShortcut-Icon";
        _shortcutIcon.AddToClassList("menu-shortcut__icon");
        Add(_shortcutIcon);

        SetVisualState(InputType.PC);

        // TODO: subscribe to input type change event to trigger SetVisualState
        // InputHandler.OnInputTypeChanged += SetVisualState; // example
    }

    private void SetVisualState(InputType inputType)
    {
        if (inputType == InputType.PC)
        {
            _shortcutIcon.SetVisualState(true);
        }
        else if (inputType == InputType.Gamepad)
        {
            _shortcutIcon.SetVisualState(false);
        }
    }
}

[UxmlElement]
public partial class ShortcutIcon : VisualElement
{
    [UxmlAttribute] public string iconKey
    { 
        get => _iconKey;
        set
        {
            _iconKey = value;
            if (_iconKeyLabel != null)
            {
                _iconKeyLabel.text = value;
            }
        }
    }

    private string _iconKey = "";
    private Label _iconKeyLabel;

    public ShortcutIcon()
    {
        _iconKeyLabel = new Label();
        _iconKeyLabel.text = iconKey;
        _iconKeyLabel.name = "ShortcutIcon-KeyLabel";
        _iconKeyLabel.AddToClassList("shortcut-icon__key-label");
        Add(_iconKeyLabel);
    }

    public void SetIconKey(string newKey)
    {
        iconKey = newKey;
        _iconKeyLabel.text = iconKey;
    }

    public void SetVisualState(bool isPC)
    {
        if (isPC)
        {
            _iconKeyLabel.style.display = DisplayStyle.Flex;
        }
        else if (!isPC)
        {
            _iconKeyLabel.style.display = DisplayStyle.None;
            // TODO -> set gamepad icon instead
        }
    }
}