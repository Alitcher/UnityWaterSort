using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class BottleController : MonoBehaviour
{
    public ColorSet bottleColors;
    public SpriteRenderer bottleMaskSR;

    private Material instanceMaterial;

    private bool selected = false;
    private int movingStep = 0;
    private float direction;
    private float targetRotation;

    private Vector3 startPosition;
    private Vector3 selectedPosition;
    private Vector3 targetPosition, smoothVector = Vector3.zero;

    void Start()
    {
        startPosition = transform.position;
        selectedPosition = new Vector3(startPosition.x, startPosition.y + 10f, startPosition.z);

        instanceMaterial = bottleMaskSR.material;
        //instanceMaterial.SetFloat("_ScaleFactor", 0.5f);
        bottleMaskSR.enabled = false;
        bottleMaskSR.enabled = true;

        ChangeColorsOnSgader();
    }

    void Update()
    {
        //if (Input.GetKeyUp(KeyCode.P))
        //{
        //    StartCoroutine(RotateBottle());
        //}

        if (selected & transform.position != selectedPosition & movingStep == 0) // moving up when selected
        {
            goTo(selectedPosition);
        }
        if (!selected & transform.position != startPosition & movingStep == 0) // moving down when unselected
        {
            goTo(startPosition);
        }

        if (movingStep == 1 & transform.position != targetPosition) // moving to other bottle
        {
            goTo(targetPosition);
            Quaternion endROtation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 90.0f * direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, endROtation, 100.0f * Time.deltaTime);
            if (transform.position == targetPosition & transform.eulerAngles.z == targetRotation)
            {
                movingStep = 2;
            }
        }
        if (movingStep == 2 & transform.position != startPosition) // moving back to original position
        {
            goTo(startPosition);
            Quaternion endROtation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0.0f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, endROtation, 100.0f * Time.deltaTime);
            if (transform.position == startPosition & transform.eulerAngles.z == 0.0f)
            {
                movingStep = 0;
                SetSelected(false);
            }
        }
    }

    Vector3 goTo(Vector3 targetPosition)
    {
        return transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref smoothVector, 0.1f);
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;
    }

    public void pourTo(Vector3 target)
    {
        direction = transform.position.x - target.x > 0 ? direction = 1.0f : direction = -1.0f;
        targetPosition = target + (transform.right * (direction * 40.0f)) + (transform.up * (bottleMaskSR.bounds.size.y / 2.0f + bottleMaskSR.bounds.size.x));
        targetRotation = direction < 0 ? 270.0f : 90.0f;
        movingStep = 1;
    }

    void ChangeColorsOnSgader()
    {
        instanceMaterial.SetColor("_C1", bottleColors.colors[0]);
        instanceMaterial.SetColor("_C2", bottleColors.colors[1]);
        instanceMaterial.SetColor("_C3", bottleColors.colors[2]);
        instanceMaterial.SetColor("_C4", bottleColors.colors[3]);
    }

    //IEnumerator RotateBottle()
    //{
    //    float t = 0;
    //    float lerpValue;
    //    float angleValue;

    //    while (t < rotationTime)
    //    {
    //        lerpValue = t / rotationTime;
    //        angleValue = Mathf.Lerp(0.0f, 75.0f, lerpValue);

    //        transform.eulerAngles = new Vector3(0.0f, 0.0f, angleValue);
    //        t += Time.deltaTime;

    //        yield return new WaitForEndOfFrame();
    //    }
    //    angleValue = 75.0f;
    //    transform.eulerAngles = new Vector3(0.0f, 0.0f, angleValue);
    //}
}
