using UnityEngine;
using MixedReality.Toolkit;

public class GraphToggleHandler : MonoBehaviour
{
    public GameObject defaultFDGGraph;
    public GameObject sphericalGraph;
    public StatefulInteractable edgeBundlingToggle;

    private GraphManager currentGraphManager;

    private void Update() {
        if (currentGraphManager != null && !currentGraphManager.didFinishEB)
        {
            edgeBundlingToggle.ForceSetToggled(false);
            edgeBundlingToggle.enabled = false;
        }
        else
        {
            edgeBundlingToggle.enabled = true;
        }
        bool isEdgeBundlingToggled = edgeBundlingToggle.IsToggled;
        if (isEdgeBundlingToggled)
        {
            currentGraphManager.GetComponent<LeftControllerVerticalAxisInput>().enabled = false;
        }
        else
        {
            currentGraphManager.GetComponent<LeftControllerVerticalAxisInput>().enabled = true;
        }
    }
    private void Start()
    {
        if (defaultFDGGraph == null || sphericalGraph == null || edgeBundlingToggle == null)
        {
            Debug.LogError("One or more public GameObjects or interactable references are not set in the inspector.");
            return;
        }

        // Ensure only the default graph is active at the start
        ShowDefaultGraph();
        ApplyEdgeBundlingState(); // Apply edge bundling state on start
    }

    // This method will be called when the default button is toggled
    public void OnDefaultButtonToggled()
    {
        ShowDefaultGraph();
        FindAndUntoggle("SphericalButton");
        ApplyEdgeBundlingState();
    }

    // This method will be called when the spherical button is toggled
    public void OnSphericalButtonToggled()
    {
        ShowSphericalGraph();
        FindAndUntoggle("DefaultButton");
        ApplyEdgeBundlingState();
    }

    // This method will be called when the edge bundling button is toggled
    public void OnEdgeBundlingToggled()
    {
        if (edgeBundlingToggle == null)
        {
            Debug.LogError("EdgeBundlingToggle reference is not set.");
            return;
        }

        bool isToggled = edgeBundlingToggle.IsToggled;
        ToggleEdgeBundling(isToggled);
    }

    private void ShowDefaultGraph()
    {
        if (defaultFDGGraph == null || sphericalGraph == null)
        {
            Debug.LogError("Default or Spherical graph GameObject references are not set.");
            return;
        }

        defaultFDGGraph.SetActive(true);
        sphericalGraph.SetActive(false);

        currentGraphManager = defaultFDGGraph.GetComponent<GraphManager>();

        if (currentGraphManager == null)
        {
            Debug.LogError("GraphManager component not found on defaultFDGGraph.");
        }
    }

    private void ShowSphericalGraph()
    {
        if (defaultFDGGraph == null || sphericalGraph == null)
        {
            Debug.LogError("Default or Spherical graph GameObject references are not set.");
            return;
        }

        defaultFDGGraph.SetActive(false);
        sphericalGraph.SetActive(true);

        currentGraphManager = sphericalGraph.GetComponent<GraphManager>();

        if (currentGraphManager == null)
        {
            Debug.LogError("GraphManager component not found on sphericalGraph.");
        }
    }

    private void ToggleEdgeBundling(bool isEnabled)
    {
        if (currentGraphManager == null)
        {
            Debug.LogError("Current GraphManager is null.");
            return;
        }

        Debug.Log($"Edge bundling toggled: {isEnabled}");

        currentGraphManager.StraightenAllEdgesGlobally(isEnabled ? 1 : 0);
    }

    private void ApplyEdgeBundlingState()
    {
        if (edgeBundlingToggle == null)
        {
            Debug.LogError("EdgeBundlingToggle reference is not set.");
            return;
        }

        if (currentGraphManager != null && !currentGraphManager.didFinishEB)
        {
            edgeBundlingToggle.ForceSetToggled(false);
            edgeBundlingToggle.enabled = false;
        }
        else
        {
            edgeBundlingToggle.enabled = true;
        }

        bool isToggled = edgeBundlingToggle.IsToggled;
        ToggleEdgeBundling(isToggled);
    }

    private void FindAndUntoggle(string buttonName)
    {
        GameObject buttonObject = GameObject.Find(buttonName);
        if (buttonObject != null)
        {
            StatefulInteractable interactable = buttonObject.GetComponent<StatefulInteractable>();
            if (interactable != null)
            {
                interactable.ForceSetToggled(false);
            }
        }
    }
}
