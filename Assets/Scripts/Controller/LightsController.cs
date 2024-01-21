using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LightsController : MonoBehaviour
{
    private Light _light;
    private float distanceToLightTarget;

    [SerializeField] private Light _bottlesLight;

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
            Input.gyro.enabled = true; // If you want to use the gyroscope
        }
    }

    void Update()
    {
        if (onHandheldDevice)
        {
            // Get the device acceleration
            Vector3 acceleration = Input.acceleration;

            // Optionally, you can smooth the movement or apply some sensitivity factor
            float sensitivity = 50.0f; // Adjust this value to your liking
            Vector3 smoothPosition = new Vector3(acceleration.x * sensitivity, acceleration.y * sensitivity, 190.0f);

            // Assign the position to the light
            transform.position = smoothPosition;
        }
        else
        {
            Vector3 mousePosition = Input.mousePosition;
            // ScreenToWorldPoint will add input z to camera's z to get output z
            //mousePosition.z = GameLogic.Instance.transform.position.z - Camera.main.transform.position.z + lightDistance;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) distanceToLightTarget = hit.distance - 10.0f;
            Vector3 worldPosition = ray.GetPoint(distanceToLightTarget);
            transform.position = new(worldPosition.x, worldPosition.y, 190.0f);
        }
    }

    void ChangeBottleMaterial(int matNr)
    {
        _light.intensity = matNr == 2 ? 1000 : 0;
        _bottlesLight.gameObject.SetActive(matNr != 2);
    }

    private void OnDestroy()
    {
        Events.OnChangeBottleMaterial -= ChangeBottleMaterial;
    }
}
