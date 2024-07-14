using UnityEngine;
using System.Collections.Generic;

public class BezierLineRenderer : MonoBehaviour
{
    public LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer component is missing on BezierLineRenderer object.");
        }
    }

    // Call this function to smooth the line
    public void Smooth(int interpolationSteps)
    {
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer component is not initialized.");
            return;
        }

        Vector3[] originalPoints = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(originalPoints);

        List<Vector3> smoothedPoints = new List<Vector3>();

        for (int i = 0; i < originalPoints.Length - 1; i++)
        {
            Vector3 p0 = originalPoints[i];
            Vector3 p1 = originalPoints[i + 1];

            // Interpolate points between p0 and p1
            for (int j = 0; j <= interpolationSteps; j++)
            {
                float t = j / (float)interpolationSteps;
                Vector3 point = CalculateBezierPoint(t, p0, (p0 + p1) / 2, p1);
                smoothedPoints.Add(point);
            }
        }

        lineRenderer.positionCount = smoothedPoints.Count;
        lineRenderer.SetPositions(smoothedPoints.ToArray());
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0; // (1-t)^2 * P0
        p += 2 * u * t * p1; // 2 * (1-t) * t * P1
        p += tt * p2;        // t^2 * P2

        return p;
    }
}
