using System.Collections.Generic;
using UnityEngine;

public class GraphEdge : MonoBehaviour
{
    public GraphNode Source;
    public GraphNode Target;
    public LineRenderer lineRenderer;
    private List<Vector3> originalPositions;
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.positionCount = 2;
    }

    public void Initialize(GraphNode source, GraphNode target)
    {
        this.Source = source;
        this.Target = target;

        Vector3[] positions = new Vector3[2] { source.transform.position, target.transform.position };
        lineRenderer.SetPositions(positions);
    }


    // public void UpdatePosition()
    // {
    //     if (lineRenderer != null && Source != null && Target != null)
    //     {
    //         // Update the initial two points (start and end) in the line renderer
    //         lineRenderer.SetPosition(0, Source.transform.position);
    //         lineRenderer.SetPosition(1, Target.transform.position);
    //     }
    // }

    public void Subdivide(int subdivisionCount)
    {
        Vector3 startPoint = lineRenderer.GetPosition(0);
        Vector3 endPoint = lineRenderer.GetPosition(lineRenderer.positionCount - 1);

        List<Vector3> points = new List<Vector3> { startPoint };

        for (int i = 1; i <= subdivisionCount; i++)
        {
            Vector3 point = Vector3.Lerp(startPoint, endPoint, (float)i / (subdivisionCount + 1));
            points.Add(point);
        }

        points.Add(endPoint);
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    public void UpdatePointPosition(int index, Vector3 position)
    {
        if (index > 0 && index < lineRenderer.positionCount - 1)
        {
            lineRenderer.SetPosition(index, position);
        }
    }

    // Member function to generate Gaussian kernel
    private float[] GaussianKernel(int size, float sigma)
    {
        float[] kernel = new float[size];
        float sum = 0;
        int halfSize = size / 2;
        for (int i = 0; i < size; i++)
        {
            kernel[i] = Mathf.Exp(-0.5f * Mathf.Pow((i - halfSize) / sigma, 2));
            sum += kernel[i];
        }
        // Normalize the kernel
        for (int i = 0; i < size; i++)
        {
            kernel[i] /= sum;
        }
        return kernel;
    }


    public Vector3[] Resample(int newPointCount)
    {
        Vector3[] points = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(points);

        // Calculate the cumulative distances
        float[] cumulativeDistances = new float[points.Length];
        cumulativeDistances[0] = 0f;
        for (int i = 1; i < points.Length; i++)
        {
            cumulativeDistances[i] = cumulativeDistances[i - 1] + Vector3.Distance(points[i - 1], points[i]);
        }

        float totalLength = cumulativeDistances[cumulativeDistances.Length - 1];
        float interval = totalLength / (newPointCount - 1);

        Vector3[] resampledPoints = new Vector3[newPointCount];
        resampledPoints[0] = points[0];
        resampledPoints[resampledPoints.Length - 1] = points[points.Length - 1];

        int currentIndex = 1;
        for (int i = 1; i < resampledPoints.Length - 1; i++)
        {
            float targetDistance = i * interval;
            while (cumulativeDistances[currentIndex] < targetDistance && currentIndex < cumulativeDistances.Length - 1)
            {
                currentIndex++;
            }

            float t = (targetDistance - cumulativeDistances[currentIndex - 1]) / (cumulativeDistances[currentIndex] - cumulativeDistances[currentIndex - 1]);
            resampledPoints[i] = Vector3.Lerp(points[currentIndex - 1], points[currentIndex], t);
        }
        lineRenderer.SetPositions(resampledPoints);
        return resampledPoints;
    }
    // Member function to smooth the edge using Gaussian convolution
    public void Smooth(int windowSize, float sigma)
    {
        Vector3[] points = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(points);

        Vector3[] smoothedPoints = new Vector3[points.Length];
        float[] kernel =GaussianKernel(windowSize, sigma);
        int halfSize = windowSize / 2;

        for (int i = 1; i < points.Length - 1; i++) // Exclude first and last points
        {
            Vector3 sum = Vector3.zero;
            float weightSum = 0;

            for (int j = -halfSize; j <= halfSize; j++)
            {
                int index = i + j;
                if (index >= 0 && index < points.Length)
                {
                    sum += points[index] * kernel[j + halfSize];
                    weightSum += kernel[j + halfSize];
                }
            }

            smoothedPoints[i] = sum / weightSum;
        }

        // Preserve the first and last points to keep endpoints fixed
        smoothedPoints[0] = points[0];
        smoothedPoints[smoothedPoints.Length - 1] = points[points.Length - 1];

        lineRenderer.SetPositions(smoothedPoints);
    }


    private void OnDrawGizmos()
    {
        if (lineRenderer == null)
            return;

        Gizmos.color = Color.red;

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            Gizmos.DrawSphere(lineRenderer.GetPosition(i), 0.1f);
        }
    }

    public void UpdatePosition()
    {

        if (transform.parent.GetComponent<GraphManager>().CurrentLayout == GraphManager.LayoutType.Spherical && transform.parent.GetComponent<GraphManager>().ConstrainToSphere)
        {
            DrawEdgeOnSphere(Source.transform.position, Target.transform.position, transform.parent.GetComponent<GraphManager>().SubdivisionCount, transform.parent.GetComponent<GraphManager>().MaxDistanceFromCenter);
        }
        else
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, Source.transform.position);
            lineRenderer.SetPosition(1, Target.transform.position);
        }
    }

    void DrawEdgeOnSphere(Vector3 start, Vector3 end, int segments, float radius)
    {
        lineRenderer.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 pointOnSphere = Vector3.Slerp(start.normalized, end.normalized, t) * radius;
            lineRenderer.SetPosition(i, pointOnSphere);
        }
    }

public void StoreOriginalPositions()
{
    originalPositions = new List<Vector3>();
    for (int i = 0; i < lineRenderer.positionCount; i++)
    {
        originalPositions.Add(lineRenderer.GetPosition(i));
    }
}

public void StraightenEdge(float bundlingAmount)
{
    if (originalPositions == null || originalPositions.Count == 0)
    {
        Debug.LogWarning("Original positions not stored.");
        return;
    }

    Vector3 P0 = originalPositions[0];
    Vector3 P1 = originalPositions[originalPositions.Count - 1];
    int N = originalPositions.Count - 2; // Exclude the first and last points

    for (int i = 1; i <= N; i++)
    {
        Vector3 pi = originalPositions[i];
        float t = (float)i / (N + 1);
        Vector3 newPos = (1 - bundlingAmount) * pi + bundlingAmount * (P0 + t * (P1 - P0));
        lineRenderer.SetPosition(i, newPos);
    }
}

public void ApplySphericalConstraint(float maxDistanceFromCenter)
{
    for (int i = 0; i < lineRenderer.positionCount; i++)
    {
        Vector3 pos = lineRenderer.GetPosition(i);
        pos = pos.normalized * maxDistanceFromCenter;
        lineRenderer.SetPosition(i, pos);
    }
}


}

