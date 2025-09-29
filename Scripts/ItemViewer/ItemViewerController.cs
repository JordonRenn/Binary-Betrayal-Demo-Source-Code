using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ItemViewerController : MonoBehaviour
{
    [SerializeField] private Camera _previewCamera;
    [SerializeField] private Transform _previewPivot;
    [SerializeField] private GameObject _spotLight;

    private static Dictionary<string, GameObject> _itemModelPrefabCache = new Dictionary<string, GameObject>();
    private const string PATH_ITEM_MODELS = "Assets/Prefabs/ItemViewModels/";
    private const string NAME_ERROR_MODEL = "error";
    private const float ZOOM_LIMIT = 10f;
    private const float ZOOM_SPEED = 6f;

    void Awake()
    {
        ItemViewerModelManager.RegisterController(this);
    }

    void Start()
    {
        if (_previewCamera == null)
        {
            Debug.LogError("Preview Camera not assigned in ItemViewerController");
            return;
        }
        LoadItemModels();
    }

    private void LoadItemModels()
    {
        var itemModelGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PATH_ITEM_MODELS });
        foreach (var guid in itemModelGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                _itemModelPrefabCache[prefab.name.ToLower()] = prefab;
            }
        }

        if (!_itemModelPrefabCache.ContainsKey(NAME_ERROR_MODEL))
        {
            Debug.LogError($"Default item model ({NAME_ERROR_MODEL}) not found in {PATH_ITEM_MODELS}");
        }
    }

    private void HideCurrentObject()
    {
        foreach (Transform child in _previewPivot)
        {
            Destroy(child.gameObject);
        }
    }

    #region Public API
    public void RegisterTextureWithPreviewCamera(RenderTexture texture)
    {
        if (_previewCamera != null)
        {
            _previewCamera.targetTexture = texture;
        }
        else
        {
            Debug.LogError("Preview Camera not assigned in ItemViewerController.");
        }
    }

    public void ResetView()
    {
        _previewPivot.rotation = Quaternion.identity;
        _previewPivot.localPosition = Vector3.zero;
        _spotLight.transform.localPosition = Vector3.zero;
    }

    public void RotateModel(Vector2 rotation)
    {
        _previewPivot.Rotate(Vector3.up, -rotation.x, Space.World);
        _previewPivot.Rotate(Vector3.right, rotation.y, Space.World);
    }

    public void AdjustZoom(float zoomDelta)
    {
        Vector3 position = _previewPivot.localPosition;
        float scaledDelta = zoomDelta * ZOOM_SPEED;
        position.z = Mathf.Clamp(position.z + scaledDelta, -ZOOM_LIMIT, ZOOM_LIMIT);
        _previewPivot.localPosition = position;
        _spotLight.transform.localPosition = position;
    }

    public void ShowItemById(string itemId)
    {
        string normalizedId = itemId.ToLower();
        if (_itemModelPrefabCache.TryGetValue(normalizedId, out GameObject prefab))
        {
            HideCurrentObject();
            var instance = Instantiate(prefab, _previewPivot);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * 1.0f; // Adjust scale as needed
            ResetView();
        }
        else if (normalizedId != NAME_ERROR_MODEL)
        {   
            Debug.LogWarning($"Item with ID '{itemId}' not found in prefab cache. Showing default item view model.");
            ShowItemById(NAME_ERROR_MODEL);
        }
        else
        {
            Debug.LogError($"Default item model not found in prefab cache! Please ensure a prefab named 'error' exists in {PATH_ITEM_MODELS}");
        }
    }
    #endregion
}