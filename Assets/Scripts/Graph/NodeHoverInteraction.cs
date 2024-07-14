using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Collections.Generic;

public class NodeEventHandler : MonoBehaviour
{
    private TextMeshProUGUI uiText;
    private float originalFontSize;
    private GraphManager graphManager;

    private void InitializeUI()
    {
        if (uiText == null)
        {
            Transform canvasTransform = transform.Find("NodeCanvas");
            if (canvasTransform != null)
            {
                Transform uiTextTransform = canvasTransform.Find("UIText");
                if (uiTextTransform != null)
                {
                    uiText = uiTextTransform.GetComponent<TextMeshProUGUI>();
                    if (uiText != null)
                    {
                        originalFontSize = uiText.fontSize;
                    }
                }
            }

            if (uiText == null)
            {
                Debug.LogError("uiText is not set and could not be found dynamically.");
            }
        }
    }

    private void Awake()
    {
        graphManager = GetComponentInParent<GraphManager>();
        if (graphManager == null)
        {
            Debug.LogError("GraphManager not found in parent.");
        }
    }

    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        Debug.Log($"{args.interactorObject} hovered over {args.interactableObject}", this);

        InitializeUI();

        if (uiText != null)
        {
            uiText.fontSize = originalFontSize * 2f; // Increase text size
            // uiText.color = Color.red; // Change text color to red

            Debug.Log("Hover entered: text size increased and color changed to red.");
        }
    }

    public void OnHoverExited(HoverExitEventArgs args)
    {
        Debug.Log($"{args.interactorObject} stopped hovering over {args.interactableObject}", this);

        InitializeUI();

        if (uiText != null)
        {
            uiText.fontSize = originalFontSize; // Reset text size
            uiText.color = Color.white; // Reset text color to white

            Debug.Log("Hover exited: text size reset and color changed to white.");
        }
    }

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log($"{args.interactorObject} selected {args.interactableObject}", this);

        InitializeUI();
               
        if (uiText != null)
        { 
            uiText.fontSize = originalFontSize * 4f;
            uiText.color = Color.green; // Change text color to green to indicate selection

            Debug.Log("Select entered: text color changed to green.");
        }

        if (graphManager != null)
        {
            // Get the GraphNode component and update outgoing edges
            GraphNode graphNode = GetComponent<GraphNode>();
            if (graphNode != null)
            {
                List<GraphEdge> newOutgoingEdges = graphNode.GetOutgoingEdges(1); // Change the depth as needed
                foreach (var edge in newOutgoingEdges)
                {
                    Debug.Log($"{graphNode.ID}  --- Outgoing edge from {graphNode.ID} to {edge.Target.ID}");
                }

                // Update the selected node and edges in the GraphManager
                graphManager.UpdateSelectedNode(graphNode);
                graphManager.UpdateSelectedEdges(newOutgoingEdges);
            }
        }
    }

    public void OnSelectExited(SelectExitEventArgs args)
    {
        Debug.Log($"{args.interactorObject} deselected {args.interactableObject}", this);

        InitializeUI();

        if (uiText != null)
        {
            uiText.fontSize = originalFontSize;;  
            uiText.color = Color.white; // Reset text color to white

            Debug.Log("Select exited: text color reset to white.");
        }

        if (graphManager != null)
        {
            // Clear the selected edges in the GraphManager
            // graphManager.ClearSelectedEdges();
        }
    }

    public void OnActivate(ActivateEventArgs args)
    {
        Debug.Log($"{args.interactorObject} activated {args.interactableObject}", this);

        InitializeUI();

        if (uiText != null)
        {
            uiText.color = Color.green; // Change text color to blue to indicate activation

            Debug.Log("Activated: text color changed to blue.");
        }
    }

    public void OnDeactivate(DeactivateEventArgs args)
    {
        Debug.Log($"{args.interactorObject} deactivated {args.interactableObject}", this);

        InitializeUI();

        if (uiText != null)
        {
            uiText.color = Color.white; // Reset text color to white

            Debug.Log("Deactivated: text color reset to white.");
        }
    }
}
