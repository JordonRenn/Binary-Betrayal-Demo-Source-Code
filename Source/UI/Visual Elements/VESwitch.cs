using System;
using UnityEngine.UIElements;

[UxmlElement]
public partial class Switch : VisualElement
{
    [UxmlAttribute]
    public bool value { get => _value; set => Set(value); }
    public Action<bool> onValueChanged { get; set; }

    private VisualElement _switchBorder;
    private VisualElement _switchControl;

    private bool _value;

    public Switch()
    {
        _switchBorder = new VisualElement();
        _switchBorder.name = "Switch-Border";
        _switchBorder.AddToClassList("switch-border");

        _switchControl = new VisualElement();
        _switchControl.name = "Switch-Control";
        _switchControl.AddToClassList("switch-control");

        //Add(_label);
        Add(_switchBorder);
        _switchBorder.Add(_switchControl);

        RegisterCallback<ClickEvent>(evt => Set(!_value));
    }

    public void Set(bool newValue)
    {
        _value = newValue;
        onValueChanged?.Invoke(_value);
        SetVisualState(newValue);
    }

    private void SetVisualState(bool newValue)
    {
        _switchBorder.EnableInClassList("switch-border_active", newValue);
        _switchControl.EnableInClassList("switch-control_active", newValue);
    }
}