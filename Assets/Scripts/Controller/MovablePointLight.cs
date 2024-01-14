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

    private bool onHandheldDevice;

    private void Awake()
    {
        _light = GetComponent<Light>();
        Events.OnChangeBottleMaterial += ChangeBottleMaterial;
    }

    private void Start()
    {
        onHandheldDevice = SystemInfo.deviceType == DeviceType.Handheld;

        if (onHandheldDevice)
        {
            Input.gyro.enabled = true;
            transform.position = new(0.0f, 0.0f, -lightDistance);
        }
    }

    void Update()
    {
        if (onHandheldDevice)
        {
            //TODO(henrik) move pointlight for metallic with gyro
            transform.position = new(Input.GetAxis("horizontal"), Input.GetAxis("vertical"), 0.0f);
        }
        else
        {
            Vector3 mousePosition = Input.mousePosition;
            // ScreenToWorldPoint will add input z to camera's z to get output z
            mousePosition.z = GameLogic.Instance.transform.position.z - Camera.main.transform.position.z - lightDistance;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            transform.position = worldPosition;
        }
    }

    void ChangeBottleMaterial(int matNr)
    {
        _light.intensity = matNr == 2 ? 2000 : 0;
    }

    private void OnDestroy()
    {
        Events.OnChangeBottleMaterial -= ChangeBottleMaterial;
    }
}
