using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace SBG.ItemViewer
{
    public class ItemViewerController : MonoBehaviour
    {
        [SerializeField] private Camera _previewCamera;
        [SerializeField] private Transform _previewPivot;
        [SerializeField] private GameObject _spotLight;

        private static Dictionary<string, GameObject> _itemModelPrefabCache = new Dictionary<string, GameObject>();
        private const string NAME_ERROR_MODEL = "error";
        private const string PATH_ITEM_MODELS = "ItemModels"; // Path in Resources folder
        private const float ZOOM_LIMIT = 10f;
        private const float ZOOM_SPEED = 6f;

        void OnDestroy()
        {
            // Clear the cache when this controller is destroyed
            _itemModelPrefabCache.Clear();
        }

        void Awake()
        {
            // Register this controller with the model manager
            ItemViewerModelManager.RegisterController(this);
        }

        void Start()
        {
            if (_previewCamera == null)
            {
                Debug.LogError("Preview Camera not assigned in ItemViewerController");
                return;
            }
            if (_previewPivot == null)
            {
                Debug.LogError("Preview Pivot not assigned in ItemViewerController");
                return;
            }
            // Item models will be loaded on-demand from Resources folder
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
            StartCoroutine(ShowItemByIdAsync(normalizedId));
        }

        private IEnumerator ShowItemByIdAsync(string normalizedId)
        {
            GameObject prefab = null;

            // Check if already cached
            if (_itemModelPrefabCache.TryGetValue(normalizedId, out prefab))
            {
                InstantiateItem(prefab);
                yield break;
            }

            // Load on-demand from Resources folder
            string resourcePath = $"{PATH_ITEM_MODELS}/{normalizedId}";
            prefab = Resources.Load<GameObject>(resourcePath);

            if (prefab != null)
            {
                _itemModelPrefabCache[normalizedId] = prefab;
                InstantiateItem(prefab);
                Debug.Log($"Successfully loaded and instantiated item model '{normalizedId}' from Resources");
            }
            else
            {
                Debug.LogWarning($"Item with ID '{normalizedId}' not found in Resources at path '{resourcePath}'. Showing default item view model.");

                if (normalizedId != NAME_ERROR_MODEL)
                {
                    Debug.Log($"Attempting to load error model '{NAME_ERROR_MODEL}'");
                    StartCoroutine(ShowItemByIdAsync(NAME_ERROR_MODEL));
                }
                else
                {
                    HideCurrentObject(); // Clear view if even error model fails
                }
            }

            yield return null;
        }

        private void InstantiateItem(GameObject prefab)
        {
            HideCurrentObject();
            var instance = Instantiate(prefab, _previewPivot);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * 1.0f; // Adjust scale as needed
            ResetView();
            Debug.Log($"Instantiated item model at {_previewPivot}, instance name: {instance.name}");
        }
        #endregion
    }
}