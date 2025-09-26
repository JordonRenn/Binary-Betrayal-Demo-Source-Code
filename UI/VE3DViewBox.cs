using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ViewBox3D : VisualElement
{
    [UxmlAttribute]
    public float Width
    {
        get => _width;
        set
        {
            _width = value;
            style.width = _width;
            if (_image != null)
                _image.style.width = _width;
        }
    }
    [UxmlAttribute]
    public float Height
    {
        get => _height;
        set
        {
            _height = value;
            style.height = _height;
            if (_image != null)
                _image.style.height = _height;
        }
    }
    [UxmlAttribute]
    public bool EnableZoom
    {
        get => _enableZoom;
        set => _enableZoom = value;
    }

    private float _width = 256f;
    private float _height = 256f;
    private bool _enableZoom = true;

    private Vector2 _lastMousePos;
    private bool _dragging;
    private Image _image;

    public ViewBox3D()
    {
        style.width = _width;
        style.height = _height;

        _image = new Image();
        _image.style.width = _width;
        _image.style.height = _height;
        Add(_image);

        RegisterInputCallbacks();

        // Create and assign RenderTexture
        var renderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
        renderTexture.Create();
        _image.image = renderTexture;
        ItemViewerModelManager.RegisterSharedTexture(renderTexture);
    }

    private void RegisterInputCallbacks()
    {
        RegisterCallback<PointerDownEvent>(OnPointerDown);
        RegisterCallback<PointerUpEvent>(OnPointerUp);
        RegisterCallback<PointerMoveEvent>(OnPointerMove);
        RegisterCallback<WheelEvent>(OnWheel);

        RegisterCallback<DetachFromPanelEvent>(evt => 
            (_image?.image as RenderTexture)?.Release());
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.button == 0) // left mouse
        {
            _dragging = true;
            _lastMousePos = evt.position;
        }
        else if (evt.button == 1) // right mouse
        {
            ItemViewerModelManager.ResetView();
        }
        evt.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (_dragging)
        {
            Vector2 currentPos = new Vector2(evt.position.x, evt.position.y);
            Vector2 delta = currentPos - _lastMousePos;
            delta *= 0.5f;
            _lastMousePos = currentPos;
            ItemViewerModelManager.RotateModel(delta);
            evt.StopPropagation();
        }
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (evt.button == 0)
        {
            _dragging = false;
            evt.StopPropagation();
        }
    }

    private void OnWheel(WheelEvent evt)
    {
        if (!_enableZoom) return;
        ItemViewerModelManager.AdjustZoom(evt.delta.y * 0.1f);
        evt.StopPropagation();
    }
}