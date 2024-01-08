using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManipulation : MonoBehaviour
{
    [SerializeField] private float bottlesZPos;
    /// <summary>
    /// Depth diff between point light and bottles. Needs to be positive.
    /// </summary>
    [SerializeField] private float lightDistance;

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        // ScreenToWorldPoint will add input z to camera's z to get output z
        mousePosition.z = bottlesZPos - Camera.main.transform.position.z - lightDistance;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        transform.position = worldPosition;
    }
}
