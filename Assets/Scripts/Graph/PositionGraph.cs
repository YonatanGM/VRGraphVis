using UnityEngine;

public class PositionGraph : MonoBehaviour
{

    public Camera mainCamera; // Public Camera variable to be set from the Inspector
    // public Camera mainCamera = Camera.main;
    public float distanceInFront = 100;


    void Start()
    {


    }
    //void Update()
    //{
    //    if (mainCamera != null)
    //    {
    //        // Get the forward direction of the camera
    //        Vector3 cameraForward = mainCamera.transform.forward;
    //        // Ignore the vertical component
    //        cameraForward.y = 0;
    //        // Normalize the vector
    //        cameraForward.Normalize();
    //        // Set the GameObject's rotation to match the camera's Y rotation
    //        Quaternion newRotation = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0);
    //        this.transform.rotation = newRotation;
    //        // Calculate the new position
    //        Vector3 cameraPosition = mainCamera.transform.position;
    //        Vector3 newPosition = cameraPosition + cameraForward * distanceInFront;

    //        // Set the GameObject's position
    //        gameObject.transform.position = newPosition;


    //    }
    //    else
    //    {
    //        Debug.LogError("Main camera not assigned.");
    //    }
    //}

}
