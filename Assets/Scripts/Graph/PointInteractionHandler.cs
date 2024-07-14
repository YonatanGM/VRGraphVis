using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class PointInteractionHandler : MonoBehaviour
{
    public TextMeshProUGUI uiText;
    public float textOffset = 0.1f;  // Adjustable offset above the object
    private Camera mainCamera;
    private XRBaseInteractable interactable;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (uiText == null)
        {
            Debug.LogError("UI Text is not assigned.");
        }

        interactable = GetComponent<XRBaseInteractable>();
        if (interactable == null)
        {
            Debug.LogError("XRBaseInteractable component is missing.");
        }
    }

    private void OnEnable()
    {
        if (interactable != null)
        {
            interactable.hoverEntered.AddListener(OnHoverEnter);
            interactable.hoverExited.AddListener(OnHoverExit);
        }
    }

    private void OnDisable()
    {
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEnter);
            interactable.hoverExited.RemoveListener(OnHoverExit);
        }
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (uiText == null || mainCamera == null)
        {
            return;
        }

        if (args.interactorObject is XRRayInteractor)
        {
            Vector3 objectCenter = GetObjectCenter();
            float objectHeight = GetObjectHeight();
            uiText.text = "Object Pointed At!";
            uiText.transform.position = objectCenter + new Vector3(0, objectHeight / 2 + textOffset, 0);  // Adjust the offset based on object height
            uiText.transform.LookAt(mainCamera.transform);
            uiText.transform.Rotate(0, 180, 0);  // Correct the rotation to face the camera
            uiText.gameObject.SetActive(true);
        }
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        if (uiText == null)
        {
            return;
        }

        uiText.gameObject.SetActive(false);
    }

    private Vector3 GetObjectCenter()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogError("Collider component is missing.");
            return Vector3.zero;
        }
        return collider.bounds.center;
    }

    private float GetObjectHeight()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogError("Collider component is missing.");
            return 0;
        }
        return collider.bounds.size.y;
    }
}
