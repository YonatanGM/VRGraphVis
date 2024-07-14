using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GraphNode : MonoBehaviour
{
    public int ID { get; private set; }
    public string Title { get; private set; }
    public int Year { get; private set; }
    public int NCitation { get; private set; }
    public string DocType { get; private set; }
    public string Publisher { get; private set; }
    public string Doi { get; private set; }

    public float textOffset = 0.5f; // Adjustable offset above the object
    private Camera mainCamera;
    public TextMeshProUGUI uiText { get; private set; }

    public float FontSize = 10;

    // List to store outgoing edges
    private List<GraphEdge> outgoingEdges = new List<GraphEdge>();

    public void Initialize(int id, Vector3 position, string title = "", int year = 0, int nCitation = 0, string docType = "", string publisher = "", string doi = "")
    {
        ID = id;
        Title = title;
        Year = year;
        NCitation = nCitation;
        DocType = docType;
        Publisher = publisher;
        Doi = doi;
        transform.position = position;
        CreateUIText();
        UpdateUIText();
    }

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void CreateUIText()
    {
        // Find existing Canvas by name in the scene
        GameObject existingCanvasObject = GameObject.Find("Canvas");
        if (existingCanvasObject == null)
        {
            Debug.LogError("No Canvas found in the scene.");
            return;
        }

        // Instantiate new Canvas as a child of the current GameObject
        GameObject newCanvasObject = Instantiate(existingCanvasObject, transform);
        newCanvasObject.name = "NodeCanvas";
        Canvas newCanvas = newCanvasObject.GetComponent<Canvas>();
        newCanvas.renderMode = RenderMode.WorldSpace;

        // Adjust the size and scale of the canvas to fit the text
        RectTransform canvasRectTransform = newCanvas.GetComponent<RectTransform>();
        canvasRectTransform.sizeDelta = new Vector2(2, 1); // Adjust the size of the canvas
        canvasRectTransform.localScale = Vector3.one * 0.1f; // Adjust the scale of the canvas

        // Create TextMeshProUGUI
        GameObject uiTextObject = new GameObject("UIText");
        uiTextObject.transform.SetParent(newCanvasObject.transform);
        uiTextObject.transform.localPosition = Vector3.zero; // Position it at the center of the canvas

        uiText = uiTextObject.AddComponent<TextMeshProUGUI>();

        // Set default properties for the TextMeshProUGUI
        uiText.fontSize = FontSize; // Adjust the font size to fit the canvas
        uiText.color = Color.white;
        uiText.alignment = TextAlignmentOptions.Center;
        uiText.enableWordWrapping = false; // Disable word wrapping to keep text horizontal
        uiText.overflowMode = TextOverflowModes.Overflow; // Allow text to overflow the bounds

        // Set RectTransform properties
        RectTransform rectTransform = uiText.GetComponent<RectTransform>();
        rectTransform.sizeDelta = canvasRectTransform.sizeDelta; // Match the size of the canvas
        rectTransform.localScale = Vector3.one; // Set the scale to one
    }

    private void Update()
    {
        if (uiText != null && mainCamera != null)
        {
            UpdateUIText();
        }
    }

    private void UpdateUIText()
    {
        if (uiText == null)
        {
            Debug.LogError("UI Text is not assigned in UpdateUIText.");
            return;
        }

        uiText.text = Title ?? "No Title"; // Display the title of the paper, or "No Title" if Title is null
        uiText.transform.LookAt(mainCamera.transform);
        uiText.transform.Rotate(0, 180, 0); // Correct the rotation to face the camera
    }

    public void SetUITextSize(float fontSize)
    {
        if (uiText != null)
        {
            uiText.fontSize = fontSize;
        }
    }

    public void SetUITextColor(Color color)
    {
        if (uiText != null)
        {
            uiText.color = color;
        }
    }

    public void SetNodeColor(Color color)
    {
        // Assuming the node has a renderer to change its color
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    public void AddOutgoingEdges(IEnumerable<GameObject> edges)
    {
        foreach (var edge in edges)
        {
            GraphEdge graphEdge = edge.GetComponent<GraphEdge>();
            if (graphEdge != null)
            {
                outgoingEdges.Add(graphEdge);
            }
        }
    }

    public List<GraphEdge> GetOutgoingEdges(int depth)
    {
        List<GraphEdge> result = new List<GraphEdge>();
        GetOutgoingEdgesRecursive(this, depth, result);
        return result;
    }

    private void GetOutgoingEdgesRecursive(GraphNode node, int depth, List<GraphEdge> result)
    {
        if (depth <= 0)
        {
            return;
        }

        foreach (var edge in node.outgoingEdges)
        {
            result.Add(edge);
            GetOutgoingEdgesRecursive(edge.Target, depth - 1, result);
        }
    }
}
