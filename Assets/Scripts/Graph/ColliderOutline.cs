using UnityEngine;


public class ColliderOutline : MonoBehaviour
{
    private LineRenderer lineRenderer;

    void Start()
    {
        // Add a Line Renderer component if not already attached
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        // Set the Line Renderer material and width
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.2f;

        // Set the number of points to 16, as the last point will connect to the first to close the box
        lineRenderer.positionCount = 16;

        // Set the Line Renderer to use World Space
        lineRenderer.useWorldSpace = true;

        // Set the color of the Line Renderer
        Color lineColor = Color.red; // Change this to your desired color
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        // Call the method to update the line positions
        UpdateLinePositions();
    }

    void UpdateLinePositions()
    {
        // Get the Box Collider component at runtime
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider component not found on the GameObject.");
            return;
        }

        // Calculate the local space corners of the Box Collider
        Vector3 colliderBounds = boxCollider.size;
        Vector3[] corners = new Vector3[8];

        // Bottom square
        corners[0] = boxCollider.center + new Vector3(-colliderBounds.x, -colliderBounds.y, -colliderBounds.z) * 0.5f;
        corners[1] = boxCollider.center + new Vector3(colliderBounds.x, -colliderBounds.y, -colliderBounds.z) * 0.5f;
        corners[2] = boxCollider.center + new Vector3(colliderBounds.x, -colliderBounds.y, colliderBounds.z) * 0.5f;
        corners[3] = boxCollider.center + new Vector3(-colliderBounds.x, -colliderBounds.y, colliderBounds.z) * 0.5f;

        // Top square
        corners[4] = boxCollider.center + new Vector3(-colliderBounds.x, colliderBounds.y, -colliderBounds.z) * 0.5f;
        corners[5] = boxCollider.center + new Vector3(colliderBounds.x, colliderBounds.y, -colliderBounds.z) * 0.5f;
        corners[6] = boxCollider.center + new Vector3(colliderBounds.x, colliderBounds.y, colliderBounds.z) * 0.5f;
        corners[7] = boxCollider.center + new Vector3(-colliderBounds.x, colliderBounds.y, colliderBounds.z) * 0.5f;

        // Convert local space to world space
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = transform.TransformPoint(corners[i]);
        }

        // Set the positions for the Line Renderer
        // Bottom square
        lineRenderer.SetPosition(0, corners[0]);
        lineRenderer.SetPosition(1, corners[1]);
        lineRenderer.SetPosition(2, corners[2]);
        lineRenderer.SetPosition(3, corners[3]);
        lineRenderer.SetPosition(4, corners[0]); // Close the bottom square

        // Sides
        lineRenderer.SetPosition(5, corners[4]);
        lineRenderer.SetPosition(6, corners[5]);
        lineRenderer.SetPosition(7, corners[1]); // Connect top and bottom
        lineRenderer.SetPosition(8, corners[5]);
        lineRenderer.SetPosition(9, corners[6]);
        lineRenderer.SetPosition(10, corners[2]); // Connect top and bottom
        lineRenderer.SetPosition(11, corners[6]);
        lineRenderer.SetPosition(12, corners[7]);
        lineRenderer.SetPosition(13, corners[3]); // Connect top and bottom
        lineRenderer.SetPosition(14, corners[7]);
        lineRenderer.SetPosition(15, corners[4]); // Close the top square
    }

    void Update()
    {
        UpdateLinePositions();
    }
}
