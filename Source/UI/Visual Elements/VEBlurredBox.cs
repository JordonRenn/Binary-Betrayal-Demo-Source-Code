using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class BlurredBox : VisualElement
{
    [UxmlAttribute]
    public Color backgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            MarkDirtyRepaint(); // Request redraw when color changes
        }
    }


    [UxmlAttribute]
    public float blurSize
    {
        get => _blurSize;
        set
        {
            _blurSize = Mathf.Clamp(value, 0f, 100f); // Clamp to reasonable range
            MarkDirtyRepaint(); // Request redraw when blur size changes
        }
    }

    private Color _backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    private float _blurSize = 15f;
    
    // Number of segments for the blur gradient - more segments = smoother blur
    private const int BlurSegments = 16;

    public BlurredBox()
    {
        // Subscribe to visual content generation
        generateVisualContent += OnGenerateVisualContent;
        
        this.AddToClassList("blurred-box");
    }

    private void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        Rect r = contentRect;
        if (r.width < 0.01f || r.height < 0.01f)
            return; // Skip rendering when too small

        float width = r.width;
        float height = r.height;
        float blur = Mathf.Min(_blurSize, Mathf.Min(width, height) * 0.4f);

        // Simple test: just create a single rectangle with gradient from center to edges
        CreateSimpleBlurRect(mgc, width, height, blur);
    }
    
    private void CreateSimpleBlurRect(MeshGenerationContext mgc, float width, float height, float blur)
    {
        // Create a simple 9-slice style blur effect
        Vertex[] vertices = new Vertex[16]; // 4x4 grid of vertices
        ushort[] indices = new ushort[54]; // 9 quads * 6 indices per quad
        
        // Define the grid positions
        float[] xPositions = { 0, blur, width - blur, width };
        float[] yPositions = { 0, blur, height - blur, height };
        
        // Define alpha values for the grid (center is solid, edges are transparent)
        float[,] alphaValues = new float[4, 4] {
            { 0f, 0f, 0f, 0f },      // Top row (transparent)
            { 0f, 1f, 1f, 0f },      // Upper middle (solid in center)
            { 0f, 1f, 1f, 0f },      // Lower middle (solid in center)
            { 0f, 0f, 0f, 0f }       // Bottom row (transparent)
        };
        
        // Create vertices
        int vertexIndex = 0;
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                float alpha = alphaValues[y, x];
                Color vertexColor = new Color(_backgroundColor.r, _backgroundColor.g, _backgroundColor.b, _backgroundColor.a * alpha);
                
                vertices[vertexIndex].position = new Vector3(xPositions[x], yPositions[y], Vertex.nearZ);
                vertices[vertexIndex].tint = vertexColor;
                vertices[vertexIndex].uv = new Vector2(0.5f, 0.5f);
                vertexIndex++;
            }
        }
        
        // Create indices for 9 quads
        int indexPos = 0;
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int topLeft = row * 4 + col;
                int topRight = topLeft + 1;
                int bottomLeft = topLeft + 4;
                int bottomRight = bottomLeft + 1;
                
                // First triangle
                indices[indexPos++] = (ushort)topLeft;
                indices[indexPos++] = (ushort)topRight;
                indices[indexPos++] = (ushort)bottomLeft;
                
                // Second triangle
                indices[indexPos++] = (ushort)topRight;
                indices[indexPos++] = (ushort)bottomRight;
                indices[indexPos++] = (ushort)bottomLeft;
            }
        }
        
        MeshWriteData mwd = mgc.Allocate(vertices.Length, indices.Length);
        mwd.SetAllVertices(vertices);
        mwd.SetAllIndices(indices);
    }
}