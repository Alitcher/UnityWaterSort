using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManipulation : MonoBehaviour
{
    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        transform.position = new(worldPosition.x, worldPosition.y, -11.0f);
    }
}
