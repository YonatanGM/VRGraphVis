using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionCanvas : MonoBehaviour
{
    // Start is called before the first frame update
    public Canvas canvas;

    void Start()
    {
        RectTransform rectTransform = canvas.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0); // Bottom-left corner
        rectTransform.anchorMax = new Vector2(0, 0); // Bottom-left corner
        rectTransform.pivot = new Vector2(0, 0); // Bottom-left corner
        rectTransform.anchoredPosition = new Vector2(0, 0); // Position at (0, 0)
    }
}
