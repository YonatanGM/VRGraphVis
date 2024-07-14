using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GraphManager
{
    private Dictionary<int, Vector3> forces;

    public IEnumerator RunForceDirectedLayout(string savePath)
    {
        forces = new Dictionary<int, Vector3>();
        foreach (var node in nodeMap.Values)
        {
            forces[node.ID] = Vector3.zero;
        }

        bool isStable = false;
        int iteration = 0;

        while (!isStable && iteration < MaxIterationsFDG)
        {
            isStable = ApplyForceDirectedLayout();
            iteration++;
            yield return new WaitForSeconds(0.1f); // Adding a small delay for stability
        }

        SaveNodePositions(savePath);
        didFinishFDG = true;
        StartCoroutine(BundleEdges());
    }

    private bool ApplyForceDirectedLayout()
    {
        // Reset forces
        foreach (var node in nodeMap.Values)
        {
            forces[node.ID] = Vector3.zero;
        }

        // Calculate repulsive forces
        foreach (var nodeA in nodeMap.Values)
        {
            foreach (var nodeB in nodeMap.Values)
            {
                if (nodeA != nodeB)
                {
                    Vector3 direction = nodeA.transform.position - nodeB.transform.position;
                    float distance = direction.magnitude;
                    if (distance == 0) distance = 0.1f;
                    Vector3 repulsiveForce = RepulsiveForceConstant * direction.normalized / (distance * distance);
                    forces[nodeA.ID] += repulsiveForce;
                    forces[nodeB.ID] -= repulsiveForce;
                }
            }
        }

        // Calculate attractive forces using the stored edges
        foreach (var edge in edges)
        {
            GraphNode sourceNode = edge.Source;
            GraphNode targetNode = edge.Target;
            Vector3 direction = targetNode.transform.position - sourceNode.transform.position;
            float distance = direction.magnitude;
            Vector3 attractiveForce = SpringConstant * (distance - SpringLength) * direction.normalized;
            forces[sourceNode.ID] += attractiveForce;
            forces[targetNode.ID] -= attractiveForce;
        }

        // Apply forces and check if the system is stable
        bool isSystemStable = true;
        foreach (var node in nodeMap.Values)
        {
            Vector3 velocity = Damping * forces[node.ID];
            if (velocity.magnitude > 0.01f) // Threshold for stability
            {
                isSystemStable = false;
                velocity = Vector3.ClampMagnitude(velocity, 0.5f); // Cap the maximum velocity
                node.transform.position += velocity;
            }

            // Constrain node to the surface of the sphere if in spherical layout
            if (CurrentLayout == LayoutType.Spherical)
            {
                ConstrainToSphereMethod(node, MaxDistanceFromCenter);
            }
            else
            {
                // Clamp the node position within the bounding box
                if (boundingBox != null)
                {
                    Vector3 minBounds = boundingBox.GetComponent<Renderer>().bounds.min;
                    Vector3 maxBounds = boundingBox.GetComponent<Renderer>().bounds.max;
                    Vector3 position = node.transform.position;

                    position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
                    position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
                    position.z = Mathf.Clamp(position.z, minBounds.z, maxBounds.z);

                    node.transform.position = position;
                }
            }
        }

        // Update edges
        foreach (var edge in edges)
        {
            edge.UpdatePosition();
        }

        return isSystemStable;
    }

    private void ConstrainToSphereMethod(GraphNode node, float radius)
    {
        Vector3 position = node.transform.position;
        node.transform.position = position.normalized * radius;
    }
}
