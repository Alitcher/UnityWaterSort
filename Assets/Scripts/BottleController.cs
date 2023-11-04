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
    private Vector3 targetPosition;

    void Start()
    {
        startPosition = transform.position * 1.0f;
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


        //TODO change targetposition and rotation to pivot around target bottle and selected bottle corner

        if (selected & transform.position != selectedPosition & movingStep == 0) // moving up when selected
        {
            transform.position = goTo(selectedPosition, 1.0f);
        }
        if (!selected & transform.position != startPosition & movingStep == 0) // moving down when unselected
        {
            transform.position = goTo(startPosition, 1.0f);
        }

        if (movingStep == 1 & transform.eulerAngles.z != targetRotation) // moving to other bottle
        {
            transform.position = goTo(targetPosition, 3.0f);
            if (transform.position == targetPosition)
            {
                movingStep = 2;
            }
        }
        if (movingStep == 2 & transform.eulerAngles.z != targetRotation) // rotating to pour
        {
            transform.rotation = rotate(90.0f * direction, 75.0f);
            if (transform.eulerAngles.z == targetRotation)
            {
                movingStep = 3;
            }
        }
        if (movingStep == 3 & transform.eulerAngles.z != 0.0f) // rotating to upright position
        {
            transform.rotation = rotate(0.0f, 75.0f);
            if (transform.eulerAngles.z == 0.0f)
            {
                movingStep = 4;
            }
        }
        if (movingStep == 4 & transform.position != startPosition) // moving back to original position
        {
            transform.position = goTo(startPosition, 3.0f);
            if (transform.position == startPosition)
            {
                movingStep = 0;
                SetSelected(false);
            }
        }
    }

    Vector3 goTo(Vector3 targetPosition, float movementSpeed) // increment bottle position towards target position
    {
        return Vector3.MoveTowards(transform.position, targetPosition, movementSpeed);
    }

    Quaternion rotate(float targetRotation, float rotationSpeed) // increment bottle rotation towards target rotation around z-axis
    {
        Quaternion endROtation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, targetRotation);
        return Quaternion.RotateTowards(transform.rotation, endROtation, rotationSpeed * Time.deltaTime);
    }

    public void SetSelected(bool selected) // set this bottle as seleced or not selected and change spriterenderers orders in layers
    {
        if (selected)
        {
            GetComponent<SpriteRenderer>().sortingOrder = 2;
            bottleMaskSR.sortingOrder = 2;
        }
        else
        {
            GetComponent<SpriteRenderer>().sortingOrder = 1;
            bottleMaskSR.sortingOrder = 0;
        }
        this.selected = selected;
    }

    public void PourTo(Vector3 target) // initiate pouring animation
    {
        direction = transform.position.x - target.x > 0 ? 1.0f : -1.0f;
        targetPosition = target + (transform.right * (direction * 20.0f)) + (transform.up * (bottleMaskSR.bounds.size.y / 2.0f + bottleMaskSR.bounds.size.x * 1.0f / 2f));
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
