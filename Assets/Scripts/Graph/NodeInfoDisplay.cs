using UnityEngine;
using TMPro;

public class NodeInfoDisplay : MonoBehaviour
{
    public TextMeshProUGUI infoText;
    private Camera mainCamera;
    private GraphManager graphManager;

    private void Start()
    {
        mainCamera = Camera.main;
        graphManager = GetComponent<GraphManager>();

        if (graphManager == null)
        {
            Debug.LogError("GraphManager component not found on the GameObject.");
        }

        if (infoText == null)
        {
            Debug.LogError("TextMeshProUGUI component is not assigned.");
        }
    }

    private void Update()
    {
        if (graphManager != null && graphManager.selectedNode != null)
        {
            GraphNode selectedNode = graphManager.selectedNode;
            infoText.text = $"ID: {selectedNode.ID}\nTitle: {selectedNode.Title}\nYear: {selectedNode.Year}\nCitations: {selectedNode.NCitation}";
        }
        else
        {
            infoText.text = "No node selected.";
        }

        // Make the canvas face the camera
        if (mainCamera != null)
        {
            transform.LookAt(mainCamera.transform);
            transform.Rotate(0, 180, 0); // Correct the rotation to face the camera
        }
    }
}
