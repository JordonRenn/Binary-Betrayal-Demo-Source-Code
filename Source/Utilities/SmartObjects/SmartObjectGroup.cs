// place on the parent object of a group of smart objects
// runs in editor, not runtime
// uses CombineMesh to combine meshes of all child smart objects
// can exlude certain smart objects from the combine
// can flatten the group, removing this component but leaving instantiated objects intact

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


#if UNITY_EDITOR
namespace SBG.SmartObjects
{
    /// <summary>
    /// Place on the parent object of a group of smart objects. Editor-only: combines meshes, flattens group, manages smart objects.
    /// </summary>
    public class SmartObjectGroup : MonoBehaviour
    {
        /// <summary>
        /// List of child smart objects in this group.
        /// </summary>
        public List<ISmartObject> SmartObjects = new List<ISmartObject>();

        /// <summary>
        /// Collects all child ISmartObject components.
        /// </summary>
        public void CollectSmartObjects()
        {
            SmartObjects.Clear();
            var foundObjects = GetComponentsInChildren<ISmartObject>();
            foreach (var obj in foundObjects)
            {
                if (obj != null && !SmartObjects.Contains(obj))
                    SmartObjects.Add(obj);
            }
        }

        private void Reset()
        {
            CollectSmartObjects();
        }

        private void OnTransformChildrenChanged()
        {
            CollectSmartObjects();
        }
        /// <summary>
        /// Flattens all smart objects in the group (editor only).
        /// </summary>
        public void Flatten()
        {
            foreach (var smartObject in SmartObjects)
            {
                if (smartObject != null)
                {
                    try
                    {
                        smartObject.Flatten();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"Failed to flatten smart object: {smartObject}. Exception: {ex.Message}", this);
                    }
                }
            }
        }

        /// <summary>
        /// Extracts objects marked with SmartObjectExclude and their children from the group.
        /// Creates a child GameObject to hold the extracted objects.
        /// </summary>
        public void ExtractExcludedObjects()
        {
            // Find all objects marked for exclusion
            var excludedObjects = GetComponentsInChildren<SmartObjectExclude>(true);
            if (excludedObjects == null || excludedObjects.Length == 0)
                return;

            // Create a container for excluded objects if needed
            string containerName = "Excluded_Objects";
            Transform container = transform.Find(containerName);
            if (container == null)
            {
                container = new GameObject(containerName).transform;
                container.SetParent(transform);
                container.localPosition = Vector3.zero;
                container.localRotation = Quaternion.identity;
                container.localScale = Vector3.one;
            }

            // Extract each excluded object
            foreach (var excludedObj in excludedObjects)
            {
                if (excludedObj == null || excludedObj.transform == null)
                    continue;

                try
                {
                    Transform objTransform = excludedObj.transform;

                    // Preserve world position and rotation
                    Vector3 worldPos = objTransform.position;
                    Quaternion worldRot = objTransform.rotation;
                    Vector3 worldScale = objTransform.lossyScale;

                    // Move to new parent
                    objTransform.SetParent(container, false);

                    // Restore world position and rotation
                    objTransform.position = worldPos;
                    objTransform.rotation = worldRot;

                    // Try to maintain scale
                    if (container.lossyScale != Vector3.one)
                    {
                        Vector3 newLocalScale = objTransform.localScale;
                        newLocalScale.x *= worldScale.x / container.lossyScale.x;
                        newLocalScale.y *= worldScale.y / container.lossyScale.y;
                        newLocalScale.z *= worldScale.z / container.lossyScale.z;
                        objTransform.localScale = newLocalScale;
                    }

                    Debug.Log($"Extracted '{objTransform.name}' to '{containerName}'", container.gameObject);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to extract excluded object '{excludedObj.name}': {ex.Message}", this);
                }
            }

            // Update smart objects list after extraction
            CollectSmartObjects();

            // If the container is empty, remove it
            if (container.childCount == 0)
                DestroyImmediate(container.gameObject);
            else
                EditorUtility.SetDirty(container.gameObject);
        }

        /// <summary>
        /// Combines meshes of all flattened child objects into a single optimized mesh. Editor only.
        /// </summary>
        /// <param name="excludeObjects">Optional list of GameObjects to exclude from combining.</param>
        public void CombineMeshes(List<GameObject> excludeObjects = null)
        {
            // Remove previous combined mesh if present
            var oldCombined = transform.Find("CombinedMesh");
            if (oldCombined != null)
            {
                DestroyImmediate(oldCombined.gameObject);
            }

            var meshFilters = GetComponentsInChildren<MeshFilter>();
            var combinesByMaterial = new Dictionary<Material, List<CombineInstance>>();
            var allMaterials = new List<Material>();

            foreach (var mf in meshFilters)
            {
                if (mf == null || mf.sharedMesh == null) continue;
                if (mf.gameObject == this.gameObject) continue;
                if (excludeObjects != null && excludeObjects.Contains(mf.gameObject)) continue;

                // Only include meshes from flattened objects
                Transform parent = mf.transform.parent;
                while (parent != null && parent != transform)
                {
                    if (parent.name.EndsWith("_Flattened"))
                    {
                        var mr = mf.GetComponent<MeshRenderer>();
                        if (mr == null || mr.sharedMaterials == null) continue;

                        // Calculate transform relative to the SmartObjectGroup
                        Matrix4x4 localToGroup = transform.worldToLocalMatrix * mf.transform.localToWorldMatrix;

                        // Process each submesh with its material
                        for (int i = 0; i < mf.sharedMesh.subMeshCount; i++)
                        {
                            // Get material for this submesh
                            Material mat = i < mr.sharedMaterials.Length ? mr.sharedMaterials[i] : mr.sharedMaterial;
                            if (mat == null) continue;

                            // Create combine instance for this submesh
                            var ci = new CombineInstance
                            {
                                mesh = mf.sharedMesh,
                                transform = localToGroup,
                                subMeshIndex = i
                            };

                            // Add to the appropriate material group
                            if (!combinesByMaterial.ContainsKey(mat))
                            {
                                combinesByMaterial[mat] = new List<CombineInstance>();
                                allMaterials.Add(mat);
                            }
                            combinesByMaterial[mat].Add(ci);
                        }

                        mr.enabled = false;
                        break;
                    }
                    parent = parent.parent;
                }
            }

            if (combinesByMaterial.Count > 0)
            {
                // Create the combined mesh object
                var combinedObject = new GameObject("CombinedMesh");
                combinedObject.transform.SetParent(transform, false);
                combinedObject.transform.localPosition = Vector3.zero;
                combinedObject.transform.localRotation = Quaternion.identity;
                combinedObject.transform.localScale = Vector3.one;

                // Create and configure the combined mesh
                var combinedMesh = new Mesh();
                combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Support larger meshes

                // Create a submesh for each material
                int submeshCount = allMaterials.Count;
                int vertexOffset = 0;
                var vertices = new List<Vector3>();
                var normals = new List<Vector3>();
                var uvs = new List<Vector2>();
                var submeshTriangles = new List<int[]>();

                // Process each material's meshes separately
                foreach (var material in allMaterials)
                {
                    if (!combinesByMaterial.TryGetValue(material, out var combines))
                        continue;

                    var submeshTris = new List<int>();
                    foreach (var combine in combines)
                    {
                        var mesh = combine.mesh;
                        var matrix = combine.transform;
                        int subMeshIndex = combine.subMeshIndex;

                        // Add vertices, normals, and UVs for this submesh
                        for (int i = 0; i < mesh.vertices.Length; i++)
                        {
                            vertices.Add(matrix.MultiplyPoint3x4(mesh.vertices[i]));
                            if (mesh.normals != null && mesh.normals.Length > i)
                                normals.Add(matrix.MultiplyVector(mesh.normals[i]).normalized);
                            if (mesh.uv != null && mesh.uv.Length > i)
                                uvs.Add(mesh.uv[i]);
                        }

                        // Add triangles for this submesh
                        int[] meshTriangles = mesh.GetTriangles(subMeshIndex);
                        for (int i = 0; i < meshTriangles.Length; i++)
                        {
                            submeshTris.Add(meshTriangles[i] + vertexOffset);
                        }

                        vertexOffset += mesh.vertices.Length;
                    }

                    submeshTriangles.Add(submeshTris.ToArray());
                }

                // Apply the combined data to the mesh
                combinedMesh.vertices = vertices.ToArray();
                if (normals.Count == vertices.Count)
                    combinedMesh.normals = normals.ToArray();
                if (uvs.Count == vertices.Count)
                    combinedMesh.uv = uvs.ToArray();

                // Set submesh count and assign triangles
                combinedMesh.subMeshCount = submeshTriangles.Count;
                for (int i = 0; i < submeshTriangles.Count; i++)
                {
                    combinedMesh.SetTriangles(submeshTriangles[i], i);
                }

                combinedMesh.name = "CombinedMesh";

                UnityEditor.Unwrapping.GenerateSecondaryUVSet(combinedMesh);

                // Optimize the mesh
                combinedMesh.Optimize();
                MeshUtility.Optimize(combinedMesh);

                // Add and configure components
                var combinedFilter = combinedObject.AddComponent<MeshFilter>();
                combinedFilter.sharedMesh = combinedMesh;

                var combinedRenderer = combinedObject.AddComponent<MeshRenderer>();
                combinedRenderer.sharedMaterials = allMaterials.ToArray();

                // Add mesh collider
                var collider = combinedObject.AddComponent<MeshCollider>();
                collider.sharedMesh = combinedMesh;
                collider.convex = false;

                // Mark the object as static
                var flags = StaticEditorFlags.ContributeGI | 
                          StaticEditorFlags.OccluderStatic | 
                          StaticEditorFlags.BatchingStatic | 
                          StaticEditorFlags.OccludeeStatic |
                          StaticEditorFlags.ReflectionProbeStatic;
                
                UnityEditor.GameObjectUtility.SetStaticEditorFlags(combinedObject, flags);

                // Save the mesh asset in editor
                SaveCombinedMesh(combinedMesh);

                // Clean up the flattened objects that were used in the combination
                var flattenedObjects = new List<GameObject>();
                foreach (Transform child in transform)
                {
                    if (child.name.EndsWith("_Flattened") && child.gameObject != combinedObject)
                    {
                        // Only destroy if all child mesh renderers were used (disabled)
                        var renderers = child.GetComponentsInChildren<MeshRenderer>();
                        bool allDisabled = renderers.Length > 0;
                        foreach (var renderer in renderers)
                        {
                            if (renderer.enabled)
                            {
                                allDisabled = false;
                                break;
                            }
                        }
                        
                        if (allDisabled)
                        {
                            flattenedObjects.Add(child.gameObject);
                        }
                    }
                }

                // Destroy the flattened objects
                foreach (var obj in flattenedObjects)
                {
                    DestroyImmediate(obj);
                }

                if (flattenedObjects.Count > 0)
                {
                    Debug.Log($"Cleaned up {flattenedObjects.Count} flattened object(s)", this);
                }
            }
            else
            {
                Debug.LogWarning("No meshes found to combine.", this);
            }
        }

        /// <summary>
        /// Saves the combined mesh as an asset in the project.
        /// </summary>
        /// <param name="mesh">The mesh to save</param>
        private void SaveCombinedMesh(Mesh mesh)
        {
            if (Application.isPlaying) return;

            string meshFolder = "Assets/Meshes";
            if (!System.IO.Directory.Exists(meshFolder))
            {
                System.IO.Directory.CreateDirectory(meshFolder);
                AssetDatabase.Refresh();
            }

            string meshPath = $"{meshFolder}/{gameObject.name}_CombinedMesh_{System.Guid.NewGuid().ToString().Substring(0, 8)}.asset";
            AssetDatabase.CreateAsset(mesh, meshPath);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif