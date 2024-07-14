using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LeftControllerVerticalAxisInput : MonoBehaviour
{
    public InputActionReference thumbstickVerticalAction; // Reference to the thumbstick vertical action

    private GraphManager graphManager; // Reference to the GraphManager
    private float bundlingAmount = 0.0f;
    private bool increasing = true;
    private void OnEnable()
    {
        // Enable the thumbstick vertical action
        thumbstickVerticalAction.action.Enable();
    }

    private void OnDisable()
    {
        // Disable the thumbstick vertical action
        thumbstickVerticalAction.action.Disable();
    }

    private void Start()
    {
        // Get the GraphManager component from the same GameObject
        graphManager = GetComponent<GraphManager>();
    }

    // void Update()
    // {

    // }

    void Update()
    {
        // Apply bundling from 0 to 1 with a step of 0.1 repeatedly
        // Read the value of the vertical axis from the thumbstick
        Vector2 thumbstickValue = thumbstickVerticalAction.action.ReadValue<Vector2>();
        float verticalValue = thumbstickValue.y;

        // Map the input value from [-1, 1] to [0, 1]
        float bundlingAmount = (verticalValue + 1) / 2;

        // Debug statement to check the mapped bundling amount
        Debug.Log($"Vertical Value: {verticalValue}, Bundling Amount: {bundlingAmount}");

        // Adjust bundling with the mapped value
        graphManager.AdjustBundling(1- bundlingAmount);

        // Wait for a short interval before the next update
        StartCoroutine(WaitAndUpdate());
    }

    private IEnumerator WaitAndUpdate()
    {
        yield return new WaitForSeconds(0.5f); // Wait for half a second
    }
}
