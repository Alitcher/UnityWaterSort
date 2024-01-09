using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovablePointLight : MonoBehaviour
{
    private Light _light;
    /// <summary>
    /// Depth diff between point light and bottles. Needs to be positive.
    /// </summary>
    [SerializeField] private float lightDistance;

    private void Awake()
    {
        _light = GetComponent<Light>();
        Events.OnChangeBottleMaterial += ChangeBottleMaterial;
    }

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        // ScreenToWorldPoint will add input z to camera's z to get output z
        mousePosition.z = GameLogic.Instance.transform.position.z - Camera.main.transform.position.z - lightDistance;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        transform.position = worldPosition;
    }

    void ChangeBottleMaterial(int matNr)
    {
        _light.intensity = matNr == 2 ? 1000 : 0;
    }

    private void OnDestroy()
    {
        Events.OnChangeBottleMaterial -= ChangeBottleMaterial;
    }
}
