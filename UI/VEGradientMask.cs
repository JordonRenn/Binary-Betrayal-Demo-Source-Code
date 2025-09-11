using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class GradientMask : VisualElement
{
    [UxmlAttribute]
    public Texture2D texture { get; set; }

    // scroll offset property
    private float _scrollOffset = 0;
    // Number of segments to subdivide the gradient - more segments = smoother gradient
    private const int GradientSegments = 5;
    
    // Static arrays to hold vertices and indices for the mesh
    readonly Vertex[] m_Vertices = new Vertex[(GradientSegments + 1) * 2];
    readonly ushort[] m_Indices = new ushort[GradientSegments * 6];

    public GradientMask()
    {
        // Initialize indices for triangle strips
        for (int i = 0; i < GradientSegments; i++)
        {
            int baseIndex = i * 6;
            int vertexIndex = i * 2;

            // Two triangles per segment (quad)
            // Triangle 1
            m_Indices[baseIndex] = (ushort)(vertexIndex);
            m_Indices[baseIndex + 1] = (ushort)(vertexIndex + 2);
            m_Indices[baseIndex + 2] = (ushort)(vertexIndex + 3);

            // Triangle 2
            m_Indices[baseIndex + 3] = (ushort)(vertexIndex);
            m_Indices[baseIndex + 4] = (ushort)(vertexIndex + 3);
            m_Indices[baseIndex + 5] = (ushort)(vertexIndex + 1);
        }

        // Subscribe to the visual content generation
        generateVisualContent += OnGenerateVisualContent;

    }
    
    // public method to update scroll
    public void SetScrollOffset(float offset)
    {
        _scrollOffset = offset;
        MarkDirtyRepaint(); // Request redraw when scroll changes
    }

    private void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        Rect r = contentRect;
        if (r.width < 0.01f || r.height < 0.01f)
            return; // Skip rendering when too small

        float width = r.width;
        float height = r.height;

        // Generate vertices for the gradient mesh
        for (int i = 0; i <= GradientSegments; i++)
        {
            float t = (float)i / GradientSegments;
            float x = width * t;
            float alpha = CalculateGradientAlpha(t);
            
            // Modify UV calculation to include scroll offset
            float u = t + _scrollOffset; // Add scroll offset to U coordinate
            
            // Top vertex
            int topVertexIndex = i * 2;
            m_Vertices[topVertexIndex].position = new Vector3(x, 0, Vertex.nearZ);
            m_Vertices[topVertexIndex].tint = new Color(1, 1, 1, alpha);
            m_Vertices[topVertexIndex].uv = new Vector2(u, 1);
            
            // Bottom vertex
            int bottomVertexIndex = i * 2 + 1;
            m_Vertices[bottomVertexIndex].position = new Vector3(x, height, Vertex.nearZ);
            m_Vertices[bottomVertexIndex].tint = new Color(1, 1, 1, alpha);
            m_Vertices[bottomVertexIndex].uv = new Vector2(u, 0);
        }

        // Allocate mesh data and set vertices/indices
        MeshWriteData mwd = mgc.Allocate(m_Vertices.Length, m_Indices.Length, texture);
        mwd.SetAllVertices(m_Vertices);
        mwd.SetAllIndices(m_Indices);
    }

    private float CalculateGradientAlpha(float t)
    {
        if (t <= 0.0f)
            return 0.0f; // Fully transparent at start
        else if (t <= 0.3f)
        {
            // Linear interpolation from 0 to 1 between 0% and 30%
            return Mathf.Lerp(0.0f, 1.0f, t / 0.3f);
        }
        else if (t <= 0.7f)
        {
            // Fully opaque between 30% and 70%
            return 1.0f;
        }
        else if (t <= 1.0f)
        {
            // Linear interpolation from 1 to 0 between 70% and 100%
            return Mathf.Lerp(1.0f, 0.0f, (t - 0.7f) / 0.3f);
        }
        else
            return 0.0f; // Fully transparent at end
    }
}