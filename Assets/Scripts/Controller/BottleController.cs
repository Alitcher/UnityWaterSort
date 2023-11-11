using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
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

    public Vector3 startPosition;
    private Vector3 selectedPosition;
    private Vector3 targetPosition;
    private Vector3 rotationPivot;


    private ObjectPool<BottleController> _pool;

    [NonSerialized]
    public int[] colorIndices = { 0, 0, 0, 0 };
    /*
     check ColorSet scriptableObject the number is equivalent to the index of the color. For example:
        0 = empty, // always be on top most of the bottle. shouldn't be the case where the empty color is at the bottom and on top of it is some other colors.
        1 = red,
        ...
        6 = purple
     ColorSet
     */

    private int currentWater; // goes between 0 - 4

    private void Awake()
    {
        instanceMaterial = bottleMaskSR.material;
    }

    void Start()
    {
        startPosition = transform.position * 1.0f;
        selectedPosition = new Vector3(startPosition.x, startPosition.y + 10f, startPosition.z);

        //instanceMaterial.SetFloat("_ScaleFactor", 0.5f);
        //bottleMaskSR.enabled = false;
        //bottleMaskSR.enabled = true;
    }
    public void GenerateColor(int fillAmount)
    {
        /*
         generate color logic at start
         */
        currentWater = fillAmount;
        ChangeColorsOnShader();
    }

    public void SetFillIn()
    {
        /*
         currentWater++;
         update color
        ***UPDATE SHADER TOO***
         */
    }

    public void SetPourOut()
    {
        /*
         currentWater--;
         update color

        ***UPDATE SHADER TOO***
         */
    }

    void UpdateColor(int top, int color) 
    {
        // TODO: UPDATE SHADER TOO
        colorIndices[top] = color;
    }

    public bool CheckEmpty()
    {
        return colorIndices[0] == 0;
    }

    bool CheckFull() 
    {
        for (int i = 0; i < colorIndices.Length; i++)
        {
            if(i == 0)
                return false;
        }
        return true;
    }

    void Update()
    {
        /*
         * Henrik
         TODO: Play with bottleMaskSR.material.SetFloat("_SARM", <adjust value here>); to smooth out water scailing during the rotation
         The rotation scale, aka _SARM inside shader graph, must be 0.3 when the bottle is at 90 degrees,
                                                        and must be 1.0 when the bottle is at 0 degree(original position).       
         */
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
            rotateAround(1.0f);

            bottleMaskSR.material.SetFloat("_SARM", 0.3f);

            bool firstCall = transform.eulerAngles.z == 0.0f;
            if (direction == -1.0f)
            {
                if (transform.eulerAngles.z < targetRotation && !firstCall)
                {
                    movingStep = 3;
                }
            } else
            {
                if (transform.eulerAngles.z > targetRotation && !firstCall)
                {
                    movingStep = 3;
                }
            }
        }
        if (movingStep == 3 & transform.eulerAngles.z != 0.0f) // rotating to upright position
        {
            rotateAround(-1.0f);

            bottleMaskSR.material.SetFloat("_SARM", 0.3f);

            if (direction == -1.0f)
            {
                if (transform.eulerAngles.z < 90.0f)
                {
                    movingStep = 4;
                    transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                }
            }
            else
            {
                if (transform.eulerAngles.z > 270.0f)
                {
                    movingStep = 4;
                    transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                }
            }
        }
        if (movingStep == 4 & transform.position != startPosition) // moving back to original position
        {
            transform.position = goTo(startPosition, 3.0f);
            bottleMaskSR.material.SetFloat("_SARM", 1.0f);
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

    void rotateAround(float back)
    {
        transform.RotateAround(rotationPivot, Vector3.forward, back * direction * 90 * Time.deltaTime);
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
        targetPosition = target + (transform.right * (direction * (bottleMaskSR.bounds.size.x / 2.0f)) + transform.up * bottleMaskSR.bounds.size.y / 2.0f);
        rotationPivot = target + (transform.up * (bottleMaskSR.bounds.size.y));
        targetRotation = direction < 0 ? 270.0f : 90.0f;
        movingStep = 1;
    }

    void ChangeColorsOnShader()
    {
        // TODO: color index should not be fixed. It should randomize within colorset;
        // Make sure that the color set to randomize is not over ColorCount inside the scriptableObject
        //
        if (currentWater == 0)
        {
            instanceMaterial.SetFloat("_FillAmount", -15.0f);
        } else
        {
            instanceMaterial.SetColor("_C4", bottleColors.colors[colorIndices[3] - 1]);
            instanceMaterial.SetColor("_C3", bottleColors.colors[colorIndices[2] - 1]);
            instanceMaterial.SetColor("_C2", bottleColors.colors[colorIndices[1] - 1]);
            instanceMaterial.SetColor("_C1", bottleColors.colors[colorIndices[0] - 1]);
        }
    }

    public void SetPool(ObjectPool<BottleController> pool)
    {
        _pool = pool;
    }



}
