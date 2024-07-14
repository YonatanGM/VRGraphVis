using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.XR.Interaction.Toolkit;

public partial class GraphManager : MonoBehaviour
{

    public enum LayoutType { ForceDirected, Spherical }
     
    [Header("File Paths")]
    public string NodesFilePath;
    public string RelationshipsFilePath;


    [Header("Layout")]
    public LayoutType CurrentLayout = LayoutType.Spherical;

    [Header("Position offset")]
    public int distanceInFront = 200;


    [Header("Prefabs")]
    public GameObject NodePrefab;
    public GameObject EdgePrefab;
    public GameObject BoundingBoxPrefab; // Reference to the bounding box prefab
    private GameObject boundingBox; // Private reference to the instantiated bounding box

    [Header("Force-Directed Graph Settings")]
    public float MaxDistanceFromCenter = 5f; 
    public float RepulsiveForceConstant = 50f; // Reduced from 100
    public float SpringLength = 2f; // Adjusted from 1f
    public float SpringConstant = 0.05f; // Adjusted from 0.1f
    public float Damping = 0.9f; // Increased damping
    public int MaxIterationsFDG = 250;
    [SerializeField]
    [Tooltip("Load the Force-Directed Graph layout from disk if a saved layout exists.")]
     [Space(10)]
    private bool loadFDGFromDisk = false;

    [Header("Spherical Layout Settings")]
    public bool ConstrainToSphere = true;
    public float MaxDistFromCenterMultiplier = 30;
    [Header("Edge Bundling Settings")]
    public float K = 0.12f; // Base spring constant
    public int InitialIterations = 50;
    public float IDecrementFactor = 2f / 3f;
    public int Cycles = 6;
     [Space(10)]
    public float StepSizeInitial = 0.126f;
    public float StepDecrementFactor = 0.8f;
    public float CompatibilityThreshold = 0.44f;
    public int SubdivisionCount = 10;
    public int MaxIterations = 50;

    public float straightenAmount = 1;
    [Space(10)]
    [Range(3, 21)]
    public int WindowSize = 5;
    [Range(0.1f, 5f)]
    public float Sigma = 2f;

   

    private Dictionary<int, GraphNode> nodeMap = new Dictionary<int, GraphNode>();
    private List<GraphEdge> edges= new List<GraphEdge>(); // List to hold edge references


    public List<GraphEdge> selectedEdges { get; private set; } = new List<GraphEdge>();
    public GraphNode selectedNode { get; private set; } = null;


    private BoxCollider boxCollider;
    private LineRenderer lineRenderer;
    private Coroutine forceDirectedLayoutCoroutine;
    private Coroutine edgeBundlingCoroutine;

    private bool didInitializeGraph = false; 
    public bool didFinishEB = false;
    private bool didFinishFDG = false; 
    private string savePath = "";

    public GameObject CameraOffset; 

    private void Start()
    {
        // Camera mainCamera = CameraOffset;
        if (CameraOffset != null)
        {
            Vector3 newPos = Vector3.zero;
            if (CurrentLayout == LayoutType.Spherical) {
                newPos.z = 0;
                CameraOffset.transform.position = newPos;
                
            } else {
                newPos.z = -100;
                CameraOffset.transform.position = newPos;
            }
        }
        
        Initialize();
     
    }

    private void OnEnable()
    {
        if (CameraOffset != null)
        {
            Vector3 newPos = Vector3.zero;
            if (CurrentLayout == LayoutType.Spherical) {
                newPos.z = 0;
                CameraOffset.transform.position = newPos;
                
            } else {
                newPos.z = -100;
                CameraOffset.transform.position = newPos;
            }
        }
        if (didInitializeGraph) {
            if (File.Exists(savePath) && loadFDGFromDisk && !didFinishEB) 
            {
                edgeBundlingCoroutine = StartCoroutine(BundleEdges());
            }
            else
            {
                if (!didFinishEB) {

                    forceDirectedLayoutCoroutine = StartCoroutine(RunForceDirectedLayout(savePath));
                }
             
            }
           

        }

    }

    private void Initialize()
    {

        boundingBox = Instantiate(BoundingBoxPrefab, transform);
        boundingBox.AddComponent<ColliderOutline>();
        boundingBox.SetActive(false);
        // Add a BoxCollider
        boxCollider = gameObject.AddComponent<BoxCollider>();
        MeshRenderer boundingBoxRenderer = boundingBox.GetComponent<MeshRenderer>();
        boundingBoxRenderer.enabled = false;
        Vector3 newPosition = transform.position;
        // newPosition.z = 700;
        transform.position = newPosition;
        if (boundingBoxRenderer != null)
        {
            boxCollider.size = boundingBoxRenderer.bounds.size;
            boxCollider.center = boundingBoxRenderer.bounds.center - transform.position;
        }
        forces = new Dictionary<int, Vector3>();

        string layoutType = CurrentLayout.ToString();
        string constrainToSphereFlag = ConstrainToSphere ? "Sphere" : "Flat";
        savePath = Path.Combine(Application.persistentDataPath, $"nodePositionssss_{layoutType}_{constrainToSphereFlag}.dat");

        List<Dictionary<string, string>> nodesData = CSVParser.Read(NodesFilePath);
        List<Dictionary<string, string>> relationshipsData = CSVParser.Read(RelationshipsFilePath);
        InitializeGraph(nodesData, relationshipsData);
        PositionNodes();
        if (File.Exists(savePath) && loadFDGFromDisk)
        {
            LoadGraphFromDisk(savePath);
            didInitializeGraph = true;
            edgeBundlingCoroutine = StartCoroutine(BundleEdges());



        }
        else
        {
            didInitializeGraph = true;
            forceDirectedLayoutCoroutine = StartCoroutine(RunForceDirectedLayout(savePath));
        }
    }


    void PositionNodes()
    {
        if (CurrentLayout == LayoutType.Spherical)
        {
            MaxDistanceFromCenter *= MaxDistFromCenterMultiplier;
            Sigma = 1;
            PositionNodesOnSphere();

        }
        else
        {
            PositionNodesRandomly();
        }
    }
    void PositionNodesOnSphere()
    {
        float phi = (Mathf.Sqrt(5) + 1) / 2 - 1;
        float angleIncrement = phi * 2 * Mathf.PI;

        int i = 0;
        foreach (var node in nodeMap.Values)
        {
            float y = 1 - (i / (float)(nodeMap.Count - 1)) * 2;
            float radiusAtY = Mathf.Sqrt(1 - y * y);
            float theta = angleIncrement * i;
            float x = Mathf.Cos(theta) * radiusAtY;
            float z = Mathf.Sin(theta) * radiusAtY;
            Vector3 position = new Vector3(x, y, z) * MaxDistanceFromCenter;
            node.transform.localPosition = position;
            i++;
        }
    }


    void PositionNodesRandomly()
    {
        foreach (var node in nodeMap.Values)
        {
            Vector3 position = new Vector3(
                Random.Range(-MaxDistanceFromCenter, MaxDistanceFromCenter),
                Random.Range(-MaxDistanceFromCenter, MaxDistanceFromCenter),
                Random.Range(-MaxDistanceFromCenter, MaxDistanceFromCenter)
            );
            node.transform.localPosition = position;
        }
    }

    public void InitializeGraph(List<Dictionary<string, string>> nodesData, List<Dictionary<string, string>> relationshipsData)
    {
        foreach (var row in nodesData)
        {
            if (row.ContainsKey("id"))
            {
                int id = int.Parse(row["id"]);
                string title = row.ContainsKey("title") ? row["title"] : "";
                int year = row.ContainsKey("year") ? int.Parse(row["year"]) : 0;
                int nCitation = row.ContainsKey("n_citation") ? int.Parse(row["n_citation"]) : 0;
                string docType = row.ContainsKey("doc_type") ? row["doc_type"] : "";
                string publisher = row.ContainsKey("publisher") ? row["publisher"] : "";
                string doi = row.ContainsKey("doi") ? row["doi"] : "";

                Vector3 pos = Vector3.zero + Random.insideUnitSphere * MaxDistanceFromCenter;
                Debug.Log(gameObject.transform.position);
                GameObject nodeObject = Instantiate(NodePrefab, gameObject.transform);
                nodeObject.name = title;
                nodeObject.transform.localPosition = pos;
                nodeObject.transform.parent = gameObject.transform;
                Rigidbody rb = nodeObject.GetComponent<Rigidbody>();
                Collider col = nodeObject.GetComponent<Collider>();
                XRGrabInteractable grabbable = nodeObject.GetComponent<XRGrabInteractable>();
                Debug.Log("XRGrabInteractable active: " + grabbable.isActiveAndEnabled);
                Debug.Log("Object Layer: " + LayerMask.LayerToName(nodeObject.layer));
                Debug.Log("Attach Point Set: " + (grabbable.attachTransform != null));
                Debug.Log("Prefab Setup Correct: " + (NodePrefab.GetComponent<XRGrabInteractable>() != null));
                Debug.Log("Interaction Manager Assigned: " + (grabbable.interactionManager != null));
                // Check if all components are present
                if (rb == null || col == null || grabbable == null)
                {
                    Debug.LogError("Missing component on instantiated object: " + nodeObject.name);
                }
                else
                {
                    // Additional checks here
                }
                // Add the necessary components
                //var interactable = nodeObject.AddComponent<XRSimpleInteractable>();
                // nodeObject.AddComponent<PointInteractionHandler>();  // Ensure the PointInteractionHandler script is attached

                // Initialize the node
                // Ensure the necessary components are attached and initialized

                GraphNode graphNode = nodeObject.GetComponent<GraphNode>();
                // GraphNode graphNode = nodeObject.AddComponent<GraphNode>();
                graphNode.Initialize(id, pos, title, year, nCitation, docType, publisher, doi);
                nodeMap[id] = graphNode;
            }
            else
            {
                Debug.LogError("Key 'id' not found in node data.");
            }
        }

        foreach (var row in relationshipsData)
        {
           if (row.ContainsKey("start") && row.ContainsKey("end"))
           {
               int sourceID = int.Parse(row["start"]);
               int targetID = int.Parse(row["end"]);

               if (nodeMap.ContainsKey(sourceID) && nodeMap.ContainsKey(targetID))
               {
                   GraphNode sourceNode = nodeMap[sourceID];
                   GraphNode targetNode = nodeMap[targetID];

                   GameObject edgeObject = Instantiate(EdgePrefab);
                   edgeObject.transform.parent = gameObject.transform;

                   GraphEdge graphEdge = edgeObject.AddComponent<GraphEdge>();
                   graphEdge.Initialize(sourceNode, targetNode);
                   graphEdge.transform.parent = gameObject.transform;

                   edges.Add(graphEdge); // Store the reference
                               // Add the edge to the source node's outgoing edges
                   sourceNode.AddOutgoingEdges(new List<GameObject> { edgeObject });
               }
               else
               {
                   Debug.LogWarning($"Source node with ID {sourceID} or target node with ID {targetID} does not exist in node map.");
               }
           }
           else
           {
               Debug.LogError("Keys 'start' or 'end' not found in relationship data.");
           }
        

        }
    }

    public void SaveNodePositions(string filePath)
    {
        GraphData graphData = new GraphData();
        foreach (var node in nodeMap)
        {
            graphData.Nodes.Add(new NodeData(node.Key, node.Value.transform.position));
        }

        BinaryFormatter bf = new BinaryFormatter();
        using (FileStream file = File.Create(filePath))
        {
            bf.Serialize(file, graphData);
        }

        Debug.Log("Node positions saved.");
    }

    public void LoadGraphFromDisk(string filePath)
    {
        if (File.Exists(filePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream file = File.Open(filePath, FileMode.Open))
            {
                GraphData graphData = (GraphData)bf.Deserialize(file);

                foreach (var nodeData in graphData.Nodes)
                {
                    Debug.Log(nodeData.ID);
                    if (nodeMap.ContainsKey(nodeData.ID))
                    {


                       nodeMap[nodeData.ID].gameObject.transform.position = nodeData.Position.ToVector3();
                    }
                }

                // Update edge positions after loading nodes
                foreach (var edge in edges)
                {
                    edge.UpdatePosition();
                }
            }

            Debug.Log("Graph loaded from disk.");
        }
        else
        {
            Debug.LogWarning("Save file not found.");
        }


    }

    void PositionGraph()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Get the forward direction of the camera
            Vector3 cameraForward = mainCamera.transform.forward;
            // Ignore the vertical component
            cameraForward.y = 0;
            // Normalize the vector
            cameraForward.Normalize();

            // Calculate the new position
            Vector3 cameraPosition = mainCamera.transform.position;
            Vector3 newPosition = cameraPosition + cameraForward * distanceInFront;

            // Set the GameObject's position
            this.transform.position = newPosition;

            // Set the GameObject's rotation to match the camera's Y rotation
            Quaternion newRotation = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0);
            this.transform.rotation = newRotation;

            Debug.Log("Graph position updated to: " + newPosition);
        }
        else
        {
            Debug.LogError("Main camera not assigned.");
        }
    }

    public void UpdateSelectedEdges(List<GraphEdge> newEdges)
    {
        // Reset the color of the previously selected edges
        foreach (var edge in selectedEdges)
        {
            if (edge.lineRenderer != null)
            {
                edge.lineRenderer.startColor = Color.white;
                edge.lineRenderer.endColor = Color.white;
            }
        }

        // Update the selected edges
        selectedEdges = newEdges;

        // Set the color of the new selected edges
        foreach (var edge in selectedEdges)
        {
            if (edge.lineRenderer != null)
            {
                edge.lineRenderer.startColor = Color.green;
                edge.lineRenderer.endColor = Color.green;
            }
        }
    }

    public void ClearSelectedEdges()
    {
        // Reset the color of the selected edges
        foreach (var edge in selectedEdges)
        {
            if (edge.lineRenderer != null)
            {
                edge.lineRenderer.startColor = Color.white;
                edge.lineRenderer.endColor = Color.white;
            }
        }

        selectedEdges.Clear();
    }

    public void UpdateSelectedNode(GraphNode newNode)
    {
        // Reset the color of the previously selected node
        if (selectedNode != null)
        {
            selectedNode.SetNodeColor(Color.white);
        }

        // Update the selected node
        selectedNode = newNode;

        // Set the color of the new selected node
        if (selectedNode != null)
        {
            selectedNode.SetNodeColor(Color.green);
        }
    }
    public void UpdateSelectedEdgesWithHue(List<GraphEdge> newEdges)
    {
        if (selectedNode == null)
        {
            Debug.LogWarning("Selected node is null.");
            return;
        }

        // Reset the color of the previously selected edges
        foreach (var edge in selectedEdges)
        {
            if (edge.lineRenderer != null)
            {
                edge.lineRenderer.startColor = Color.white;
                edge.lineRenderer.endColor = Color.white;
            }
        }

        // Update the selected edges
        selectedEdges = newEdges;

        // Calculate maximum distance for normalization
        float maxDistance = 100f;
        
        // Set the color of the new selected edges based on the distance
        foreach (var edge in selectedEdges)
        {
            if (edge.lineRenderer != null)
            {
                // Calculate the hue based on the distance
                float distance = Vector3.Distance(selectedNode.transform.position, edge.Source.transform.position);
                Debug.Log("dist: " + distance);
                float normalizedDistance = distance / maxDistance;
                float hue = Mathf.Lerp(1.0f / 3.0f, 1.0f / 3.0f + 0.2f, normalizedDistance); // Linear interpolation from green to blue
                Color edgeColor = Color.HSVToRGB(hue, 1.0f, 1.0f);
                edge.lineRenderer.startColor = edgeColor;
                edge.lineRenderer.endColor = edgeColor;
            }
        }
    }
    public void AdjustBundling(float bundlingAmount)
    {
        StraightenSelectedEdges(bundlingAmount);
    }

    public void StraightenSelectedEdges(float bundlingAmount)
    {
        if (selectedEdges == null || selectedEdges.Count == 0)
        {
            Debug.LogWarning("No selected edges to straighten.");
            return;
        }

        foreach (var edge in selectedEdges)
        {
            Debug.LogWarning("No =.");
            edge.StraightenEdge(bundlingAmount);
            if (CurrentLayout == LayoutType.Spherical && ConstrainToSphere)
            {
                edge.ApplySphericalConstraint(MaxDistanceFromCenter);
            }
        }
    }

    public void StraightenAllEdgesGlobally(float bundlingAmount)
    {
        if (didFinishEB) {
            foreach (var edge in edges)
            {
                edge.StraightenEdge(bundlingAmount);
                if (CurrentLayout == LayoutType.Spherical && ConstrainToSphere)
                {
                    

                    edge.ApplySphericalConstraint(MaxDistanceFromCenter);
                }
            }


        }

    }

 

}

