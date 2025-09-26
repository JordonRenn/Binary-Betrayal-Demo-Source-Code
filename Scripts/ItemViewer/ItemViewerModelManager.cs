using UnityEngine;
using UnityEngine.SceneManagement;

public static class ItemViewerModelManager
{
    public static RenderTexture SharedRenderTexture;

    private static ItemViewerController _controller;

    static ItemViewerModelManager()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    #region Public API
    public static void RegisterController(ItemViewerController controller)
    {
        _controller = controller;
        _controller?.RegisterTextureWithPreviewCamera(SharedRenderTexture);
    }

    public static void RegisterSharedTexture(RenderTexture texture)
    {
        SharedRenderTexture = texture;
    }

    public static void RotateModel(Vector2 rotation)
    {
        _controller?.RotateModel(rotation);
    }

    public static void ResetView()
    {
        _controller?.ResetView();
    }

    public static void AdjustZoom(float zoomDelta)
    {
        _controller?.AdjustZoom(zoomDelta);
    }

    public static void ShowItem(string itemId)
    {
        _controller?.ShowItemById(itemId);
    }

    private static void OnSceneUnloaded(Scene previousScene)
    {
        _controller = null;
    }
    #endregion
}

