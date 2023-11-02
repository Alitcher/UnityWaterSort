using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BottleController : MonoBehaviour
{
    public ColorSet bottleColors;
    public SpriteRenderer bottleMaskSR;
    private Material instanceMaterial;
    // Start is called before the first frame update
    void Start()
    {
        instanceMaterial = bottleMaskSR.material;
        //instanceMaterial.SetFloat("_ScaleFactor", 0.5f);
        bottleMaskSR.enabled = false;
        bottleMaskSR.enabled = true;

        ChangeColorsOnSgader();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.P)) 
        {
            StartCoroutine(RotateBottle());
        }
    }


    void ChangeColorsOnSgader() 
    {
        instanceMaterial.SetColor("_C1", bottleColors.colors[0]);
        instanceMaterial.SetColor("_C2", bottleColors.colors[1]);
        instanceMaterial.SetColor("_C3", bottleColors.colors[2]);
        instanceMaterial.SetColor("_C4", bottleColors.colors[3]);
    }

    public float rotTime = 1.0f;
    IEnumerator RotateBottle() 
    {
        float t = 0;
        float lerpValue;
        float angleValue;

        while (t < rotTime) 
        {
            lerpValue = t / rotTime;
            angleValue = Mathf.Lerp(0.0f, 75.0f, lerpValue);

            transform.eulerAngles = new Vector3 (0.0f, 0.0f, angleValue);
            t += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
        angleValue = 75.0f;
        transform.eulerAngles = new Vector3(0.0f, 0.0f, angleValue);
    }
}
