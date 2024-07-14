using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GraphManager
{
    private Dictionary<GraphEdge, List<GraphEdge>> edgeCompatibilityMap;

    public IEnumerator BundleEdges()
    {
        PrecomputeEdgeCompatibilities();

        foreach (GraphEdge edge in edges)
        {
            edge.Subdivide(SubdivisionCount);
            if (CurrentLayout == LayoutType.Spherical && ConstrainToSphere)
            {
                ConstrainEdgeToSphere(edge);
            }
        }

        for (int iteration = 0; iteration < MaxIterations; iteration++)
        {
            float currentStepSize = StepSizeInitial * Mathf.Pow(StepDecrementFactor, iteration / (float)MaxIterations);
            ApplyBundlingForces(currentStepSize);
            // Resample and smooth after bundling
            foreach (GraphEdge edge in edges)
            {
                edge.Smooth(WindowSize, Sigma);
                if (CurrentLayout == LayoutType.Spherical && ConstrainToSphere)
                {
                    ConstrainEdgeToSphere(edge);
                }
            }
            yield return new WaitForSeconds(0.02f); // Small delay for each iteration
        }

        
        foreach (GraphEdge edge in edges) {
            edge.StoreOriginalPositions();
        }
        didFinishEB = true;
        
    }

    private void PrecomputeEdgeCompatibilities()
    {
        edgeCompatibilityMap = new Dictionary<GraphEdge, List<GraphEdge>>();
        foreach (GraphEdge edge in edges)
        {
            edgeCompatibilityMap[edge] = GetCompatibleEdges(edge);
        }
    }

    private List<GraphEdge> GetCompatibleEdges(GraphEdge edge)
    {
        List<GraphEdge> compatibleEdges = new List<GraphEdge>();

        foreach (GraphEdge otherEdge in edges)
        {
            if (otherEdge != edge)
            {
                float compatibility = CalculateEdgeCompatibility(edge, otherEdge);
                if (compatibility > CompatibilityThreshold)
                {
                    compatibleEdges.Add(otherEdge);
                }
            }
        }

        return compatibleEdges;
    }

    private void ApplyBundlingForces(float stepSize)
    {
        foreach (GraphEdge edge in edges)
        {
            for (int i = 1; i < edge.lineRenderer.positionCount - 1; i++)
            {
                Vector3 force = Vector3.zero;
                foreach (GraphEdge otherEdge in edgeCompatibilityMap[edge])
                {
                    force += CalculateBundlingForce(edge, otherEdge, i);
                }

                Vector3 newPosition = edge.lineRenderer.GetPosition(i) + stepSize * force;
                if (CurrentLayout == LayoutType.Spherical  && ConstrainToSphere)
                {
                    newPosition = newPosition.normalized * MaxDistanceFromCenter;
                }
                edge.lineRenderer.SetPosition(i, newPosition);
            }
        }
    }

    private Vector3 CalculateBundlingForce(GraphEdge edge, GraphEdge otherEdge, int index)
    {
        Vector3 direction = otherEdge.lineRenderer.GetPosition(index) - edge.lineRenderer.GetPosition(index);
        float distance = direction.magnitude;
        if (distance == 0) return Vector3.zero;

        return K * direction / distance;
    }

    private void ConstrainEdgeToSphere(GraphEdge edge)
    {
        for (int i = 0; i < edge.lineRenderer.positionCount; i++)
        {
            Vector3 position = edge.lineRenderer.GetPosition(i);
            edge.lineRenderer.SetPosition(i, position.normalized * MaxDistanceFromCenter);
        }
    }

    private float CalculateEdgeCompatibility(GraphEdge edge1, GraphEdge edge2)
    {
        return (CalculateAngleCompatibility(edge1, edge2) *
                CalculateScaleCompatibility(edge1, edge2) *
                CalculatePositionCompatibility(edge1, edge2) *
                CalculateVisibilityCompatibility(edge1, edge2));
    }

    private float CalculateAngleCompatibility(GraphEdge edge1, GraphEdge edge2)
    {
        Vector3 dir1 = (edge1.lineRenderer.GetPosition(edge1.lineRenderer.positionCount - 1) - edge1.lineRenderer.GetPosition(0)).normalized;
        Vector3 dir2 = (edge2.lineRenderer.GetPosition(edge2.lineRenderer.positionCount - 1) - edge2.lineRenderer.GetPosition(0)).normalized;
        return Mathf.Abs(Vector3.Dot(dir1, dir2));
    }

    private float CalculateScaleCompatibility(GraphEdge edge1, GraphEdge edge2)
    {
        float len1 = Vector3.Distance(edge1.lineRenderer.GetPosition(0), edge1.lineRenderer.GetPosition(edge1.lineRenderer.positionCount - 1));
        float len2 = Vector3.Distance(edge2.lineRenderer.GetPosition(0), edge2.lineRenderer.GetPosition(edge2.lineRenderer.positionCount - 1));
        return 2.0f / (len1 / len2 + len2 / len1);
    }

    private float CalculatePositionCompatibility(GraphEdge edge1, GraphEdge edge2)
    {
        Vector3 mid1 = (edge1.lineRenderer.GetPosition(0) + edge1.lineRenderer.GetPosition(edge1.lineRenderer.positionCount - 1)) / 2.0f;
        Vector3 mid2 = (edge2.lineRenderer.GetPosition(0) + edge2.lineRenderer.GetPosition(edge2.lineRenderer.positionCount - 1)) / 2.0f;

        float len1 = Vector3.Distance(edge1.lineRenderer.GetPosition(0), edge1.lineRenderer.GetPosition(edge1.lineRenderer.positionCount - 1));
        float len2 = Vector3.Distance(edge2.lineRenderer.GetPosition(0), edge2.lineRenderer.GetPosition(edge2.lineRenderer.positionCount - 1));

        float lavg = (len1 + len2) * 0.5f;

        return lavg / (lavg + Vector3.Distance(mid1, mid2));
    }

    private float CalculateVisibilityCompatibility(GraphEdge edge1, GraphEdge edge2)
    {
        Vector3 mid1 = (edge1.lineRenderer.GetPosition(0) + edge1.lineRenderer.GetPosition(edge1.lineRenderer.positionCount - 1)) / 2.0f;
        Vector3 mid2 = (edge2.lineRenderer.GetPosition(0) + edge2.lineRenderer.GetPosition(edge2.lineRenderer.positionCount - 1)) / 2.0f;

        Vector3 dir1 = (edge1.lineRenderer.GetPosition(edge1.lineRenderer.positionCount - 1) - edge1.lineRenderer.GetPosition(0)).normalized;
        Vector3 dir2 = (edge2.lineRenderer.GetPosition(edge2.lineRenderer.positionCount - 1) - edge2.lineRenderer.GetPosition(0)).normalized;

        float visibility = Mathf.Min(CalculateVisibility(mid1, edge1, edge2), CalculateVisibility(mid2, edge2, edge1));

        return visibility;
    }

    private float CalculateVisibility(Vector3 point, GraphEdge edge1, GraphEdge edge2)
    {
        Vector3 dir1 = (edge1.lineRenderer.GetPosition(edge1.lineRenderer.positionCount - 1) - edge1.lineRenderer.GetPosition(0)).normalized;
        Vector3 dir2 = (edge2.lineRenderer.GetPosition(edge2.lineRenderer.positionCount - 1) - edge2.lineRenderer.GetPosition(0)).normalized;

        Vector3 inter1 = point + dir1 * Vector3.Distance(point, edge1.lineRenderer.GetPosition(0));
        Vector3 inter2 = point + dir2 * Vector3.Distance(point, edge2.lineRenderer.GetPosition(0));

        float visibility = Mathf.Max(1 - (2 * Vector3.Distance(inter1, inter2) / Vector3.Distance(edge1.lineRenderer.GetPosition(0), edge1.lineRenderer.GetPosition(edge1.lineRenderer.positionCount - 1))), 0);

        return visibility;
    }
}



    //    private Vector3 CalculateEdgeAttractionForce(GraphEdge edge, int index)
    // {
    //     // Calculate edge attraction force between corresponding subdivision points of different edges
    //     Vector3 force = Vector3.zero;

    //     foreach (GraphEdge otherEdge in edges)
    //     {
    //         if (otherEdge != edge)
    //         {
    //             Vector3 direction = otherEdge.lineRenderer.GetPosition(index) - edge.lineRenderer.GetPosition(index);
    //             float distance = direction.magnitude;
    //             if (distance > 0)
    //             {
    //                 float compatibility = CalculateEdgeCompatibility(edge, otherEdge);
    //                 force += compatibility * direction.normalized / (distance * distance); // Compatibility * (p_i - q_i) / |p_i - q_i|^2 from Holten's paper
    //             }
    //         }
    //     }

    //     return force;
    // }


    // compatibility calculations 