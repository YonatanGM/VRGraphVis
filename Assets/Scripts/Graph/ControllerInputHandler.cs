// using UnityEngine;
// using UnityEngine.InputSystem;

// public class ControllerInputHandler : MonoBehaviour
// {
//     // References to the Input Actions
//     public InputActionReference gripAction;
//     public InputActionReference translateAnchorAction;

//     private void OnEnable()
//     {
//         // Enable the input actions
//         gripAction.action.Enable();
//         translateAnchorAction.action.Enable();
//     }

//     private void OnDisable()
//     {
//         // Disable the input actions
//         gripAction.action.Disable();
//         translateAnchorAction.action.Disable();
//     }

//     void Update()
//     {
//         // Read the grip value
//         float gripValue = gripAction.action.ReadValue<float>();

//         // Read the translate anchor input (2D vector from the thumbstick)
//         Vector2 translateInput = translateAnchorAction.action.ReadValue<Vector2>();

//         // Log the values to the console
//         Debug.Log($"Grip Value: {gripValue}");
//         Debug.Log($"Translate Anchor Value: {translateInput}");
        
//         // Use translateInput.x and translateInput.y to control movement or other logic
//     }
// }

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ControllerInputHandler : MonoBehaviour
{
    public InputActionReference aButtonAction;
    public InputActionReference bButtonAction;

    private GraphManager graphManager;
    private int edgeCounter = 1;

    private void Awake()
    {
        // Get the GraphManager component attached to the same GameObject
        graphManager = GetComponent<GraphManager>();
        if (graphManager == null)
        {
            Debug.LogError("GraphManager component not found on the same GameObject.");
        }
    }

    private void OnEnable()
    {
        aButtonAction.action.Enable();
        bButtonAction.action.Enable();
    }

    private void OnDisable()
    {
        aButtonAction.action.Disable();
        bButtonAction.action.Disable();
    }

    void Update()
    {
        if (aButtonAction.action.triggered)
        {
            Debug.Log("A Button Pressed");
            HandleAButtonPress();
        }

        if (bButtonAction.action.triggered)
        {
            Debug.Log("B Button Pressed");
            HandleBButtonPress();
        }
    }

    private void HandleAButtonPress()
    {
        if (graphManager.selectedNode != null)
        {
            edgeCounter = Mathf.Min(edgeCounter + 1, 5);
            List<GraphEdge> outgoingEdges = graphManager.selectedNode.GetOutgoingEdges(edgeCounter);
            
            graphManager.UpdateSelectedEdgesWithHue(outgoingEdges);
        }
        else
        {
            Debug.LogWarning("No node is currently selected.");
        }
    }

    private void HandleBButtonPress()
    {
        if (graphManager.selectedNode != null)
        {
            edgeCounter = Mathf.Max(0, edgeCounter - 1);  // Ensure counter does not go below 0
            List<GraphEdge> outgoingEdges = graphManager.selectedNode.GetOutgoingEdges(edgeCounter);
            graphManager.UpdateSelectedEdgesWithHue(outgoingEdges);
        }
        else
        {
            Debug.LogWarning("No node is currently selected.");
        }
    }
}
