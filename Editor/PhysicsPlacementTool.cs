using UnityEngine;
using UnityEditor;

public class PhysicsPlacementTool : EditorWindow
{
    private static GameObject selectedObject;
    private static bool isSimulating = false;
    private static Rigidbody rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private ConfigurableJoint dragJoint;
    private Vector3 grabPoint;
    private GameObject anchorObject;

    private enum SimulationMode { Fall, Move }
    private static SimulationMode currentMode = SimulationMode.Fall;
    private Vector3 mouseStartPosition;
    private Vector3 objectStartPosition;
    private bool isDragging = false;

    [MenuItem("Tools/Physics Placement Tool")]
    public static void ShowWindow()
    {
        GetWindow<PhysicsPlacementTool>("Physics Placement");
    }

    private void OnGUI()
    {
        GUILayout.Label("Physics-Based Object Placement", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(isSimulating && currentMode == SimulationMode.Fall ? "Stop Simulation" : "Physics Fall"))
        {
            currentMode = SimulationMode.Fall;
            if (isSimulating)
                StopPhysicsSimulation();
            else
                ApplyPhysicsToSelected();
        }
        if (GUILayout.Button(isSimulating && currentMode == SimulationMode.Move ? "Stop Moving" : "Physics Move"))
        {
            currentMode = SimulationMode.Move;
            if (isSimulating)
                StopPhysicsSimulation();
            else
                ApplyPhysicsToSelected();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void OnSelectionChange()
    {
        selectedObject = Selection.activeGameObject;
        Repaint();
    }

    private void ApplyPhysicsToSelected()
    {
        if (selectedObject == null)
        {
            Debug.LogWarning("No object selected!");
            return;
        }

        // Store initial transform state
        initialPosition = selectedObject.transform.position;
        initialRotation = selectedObject.transform.rotation;

        if (!selectedObject.GetComponent<Collider>())
        {
            Debug.LogWarning("Selected object must have a Collider!");
            return;
        }

        // Ensure the object has a Rigidbody
        rb = selectedObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = selectedObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = currentMode == SimulationMode.Fall;
        rb.mass = 10f;
        rb.linearDamping = 3f; // Increased damping for slower movement
        rb.angularDamping = 2f; // Reduced for more natural rotation
        rb.maxAngularVelocity = 7f; // Limit rotation speed
        
        Time.timeScale = 0.5f; // Slow down physics simulation
        Physics.simulationMode = UnityEngine.SimulationMode.Script;

        isSimulating = true;
        EditorApplication.update += SimulatePhysics;
    }

    private void StopPhysicsSimulation()
    {
        if (selectedObject != null)
        {
            if (dragJoint != null)
            {
                DestroyImmediate(dragJoint);
                dragJoint = null;
            }
            if (anchorObject != null)
            {
                DestroyImmediate(anchorObject);
                anchorObject = null;
            }
            if (rb != null)
            {
                // Register the movement with Unity's Undo system before destroying the component
                Undo.RegisterCompleteObjectUndo(selectedObject.transform, "Physics Placement");
                Vector3 finalPosition = selectedObject.transform.position;
                Quaternion finalRotation = selectedObject.transform.rotation;
                
                rb.isKinematic = true;
                DestroyImmediate(rb);
                
                // Ensure transform is properly set after component removal
                selectedObject.transform.position = finalPosition;
                selectedObject.transform.rotation = finalRotation;
            }
        }

        Time.timeScale = 1f; // Reset time scale
        Physics.simulationMode = UnityEngine.SimulationMode.FixedUpdate;
        isSimulating = false;
        rb = null;
        EditorApplication.update -= SimulatePhysics;
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        DrawSimulationOverlay(sceneView);

        if (!isSimulating || currentMode != SimulationMode.Move)
            return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        
        Event e = Event.current;
        if (e == null) return;

        Camera sceneCamera = sceneView.camera;
        
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    RaycastHit hit;
                    
                    if (Physics.Raycast(ray, out hit) && hit.rigidbody == rb)
                    {
                        mouseStartPosition = e.mousePosition;
                        objectStartPosition = selectedObject.transform.position;
                        grabPoint = hit.point;
                        isDragging = true;
                        
                        // Create anchor object
                        anchorObject = new GameObject("Physics Drag Anchor");
                        anchorObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                        anchorObject.transform.position = hit.point;
                        var anchorRb = anchorObject.AddComponent<Rigidbody>();
                        anchorRb.isKinematic = true;
                        
                        // Create joint at hit point
                        dragJoint = selectedObject.AddComponent<ConfigurableJoint>();
                        dragJoint.connectedBody = anchorRb;
                        dragJoint.configuredInWorldSpace = true;
                        dragJoint.xMotion = ConfigurableJointMotion.Limited;
                        dragJoint.yMotion = ConfigurableJointMotion.Limited;
                        dragJoint.zMotion = ConfigurableJointMotion.Limited;
                        dragJoint.angularXMotion = ConfigurableJointMotion.Free;
                        dragJoint.angularYMotion = ConfigurableJointMotion.Free;
                        dragJoint.angularZMotion = ConfigurableJointMotion.Free;
                        
                        var jointDrive = new JointDrive 
                        { 
                            positionSpring = 1000f,
                            positionDamper = 50f,
                            maximumForce = 1000f 
                        };
                        dragJoint.xDrive = jointDrive;
                        dragJoint.yDrive = jointDrive;
                        dragJoint.zDrive = jointDrive;
                        dragJoint.targetPosition = Vector3.zero;
                        dragJoint.anchor = selectedObject.transform.InverseTransformPoint(hit.point);
                    }
                    e.Use();
                }
                break;

            case EventType.MouseDrag:
                if (isDragging && rb != null && dragJoint != null && anchorObject != null)
                {
                    rb.useGravity = false;
                    
                    Plane dragPlane = new Plane(-sceneCamera.transform.forward, grabPoint);
                    Ray currentRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    
                    if (dragPlane.Raycast(currentRay, out float enter))
                    {
                        Vector3 targetPoint = currentRay.GetPoint(enter);
                        anchorObject.transform.position = targetPoint;
                    }
                    
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                if (isDragging && rb != null)
                {
                    isDragging = false;
                    rb.useGravity = true;
                    if (dragJoint != null)
                    {
                        DestroyImmediate(dragJoint);
                        dragJoint = null;
                    }
                    if (anchorObject != null)
                    {
                        DestroyImmediate(anchorObject);
                        anchorObject = null;
                    }
                    e.Use();
                }
                break;
        }
    }

    private void DrawSimulationOverlay(SceneView sceneView)
    {
        if (!isSimulating) return;

        Handles.BeginGUI();
        
        // Draw overlay text in center-top
        var view = sceneView.position;
        var overlayRect = new Rect((view.width - 190) / 2, 10, 190, 45);
        var backgroundColor = currentMode == SimulationMode.Fall ? 
            new Color(0.8f, 0.3f, 0.3f, 0.8f) : 
            new Color(0.3f, 0.5f, 0.8f, 0.8f);

        EditorGUI.DrawRect(overlayRect, backgroundColor);
        GUI.Label(overlayRect, 
            $"\n  PHYSICS SIMULATION\n  Mode: {currentMode}", 
            new GUIStyle(EditorStyles.boldLabel) { 
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            });

        Handles.EndGUI();
    }

    private void SimulatePhysics()
    {
        if (!isSimulating || selectedObject == null)
        {
            StopPhysicsSimulation();
            return;
        }

        rb = selectedObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            StopPhysicsSimulation();
            return;
        }

        // Record object state before simulation step
        Undo.RegisterCompleteObjectUndo(selectedObject.transform, "Physics Simulation Step");
        
        float deltaTime = 0.01f; // Halved from 0.02f
        Physics.Simulate(deltaTime);
        SceneView.RepaintAll();
    }

    private void OnDestroy()
    {
        // Cleanup when the window is closed
        if (isSimulating)
        {
            StopPhysicsSimulation();
        }
    }
}
