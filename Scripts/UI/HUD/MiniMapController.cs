using UnityEngine;
using UnityEngine.UIElements;

public class MiniMapController : MonoBehaviour
{
    private UIDocument _document;
    private VisualElement _root;
    void Start()
    {
        _document = GetComponent<UIDocument>();
        _root = _document.rootVisualElement;
    }

    void Update()
    {
        
    }
}
