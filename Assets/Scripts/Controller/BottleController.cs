using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.ShaderGraph.Internal;
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
    private bool rotationStarted = false;
    private int movingStep = 0;
    private float direction;
    private float targetRotation;
    private float rotationSpeed = 120.0f;
    private float rotationStart;
    private float rotationEnd;
    private float fillAmount;
    private float fillDelta;

    public Vector3 startPosition;
    private Vector3 selectedPosition;
    private Vector3 targetPosition;
    private Vector3 rotationPivot;

    public float[] waterPivots = new float[4];

    private ObjectPool<BottleController> _pool;

    private int currentWater; // goes between 0 - 4
    private int layersToPour = 1;

    [SerializeField] private int[] colorIndices = { 0, 0, 0, 0 };

    private float layerHeight = 6.25f;
    /*
     check ColorSet scriptableObject the number is equivalent to the index of the color. For example:
        0 = empty, // always be on top most of the bottle. shouldn't be the case where the empty color is at the bottom and on top of it is some other colors.
        1 = red,
        ...
        6 = purple
     ColorSet
     */

    public int GetCurrentWater => currentWater;

    public int GetLayersToPour()
    {
        int counts = 1;
        int topColorIndex = colorIndices.Length - 1;

        while (topColorIndex >= 0 && colorIndices[topColorIndex] == 0)
        {
            topColorIndex--; // Move down if we find an empty layer.
        }

        // If the bottle is empty, we return 0 because there's nothing to pour.
        if (topColorIndex < 0)
        {
            return 0;
        }

        int topColor = colorIndices[topColorIndex];

        for (int i = topColorIndex - 1; i >= 0; i--)
        {
            if (colorIndices[i] == topColor)
            {
                counts++; // Increment count if the color is the same.
            }
            else
            {
                break; // Stop counting if we hit a different color.
            }
        }
        return counts;
    }

    private void Awake()
    {
        instanceMaterial = bottleMaskSR.material;
        fillAmount = instanceMaterial.GetFloat("_FillAmount");
    }

    void Start()
    {
        startPosition = transform.position * 1.0f;
        selectedPosition = new Vector3(startPosition.x, startPosition.y + 10f, startPosition.z);

        for (int i = 0; i < waterPivots.Length; i++)
        {
            waterPivots[i] = -8.75f + i * layerHeight;

        }
    }

    public void GenerateColor(int fillAmount)
    {
        currentWater = fillAmount;
        ResetShaderLayers();
    }

    public void SetFillIn(int fillColor, int layerCount)
    {
        for (int i = 0; i < layerCount; i++)
        {
            colorIndices[currentWater + i] = fillColor;
        }
        currentWater += layerCount;

        float additionalFillAmount = layerCount * layerHeight;
        float newFillAmount = instanceMaterial.GetFloat("_FillAmount") + additionalFillAmount;

        UpdateShaderLayers(newFillAmount);


    }

    public void SetPourOut(int layerCount)
    {
        for (int i = 1; i <= layerCount; i++)
        {
            colorIndices[currentWater - i] = 0;
        }
        currentWater -= layerCount;

        float additionalFillAmount = layerCount * layerHeight;
        float newFillAmount = instanceMaterial.GetFloat("_FillAmount") - additionalFillAmount;

        UpdateShaderLayers(newFillAmount);

    }

    void ResetShaderLayers()
    {
        if (currentWater == 0)
        {
            UpdateShaderLayers(-15.0f);
            return;
        }


        UpdateShaderLayers(10.0f);

    }

    private void UpdateShaderLayers(float newFillAmount)
    {
        instanceMaterial.SetFloat("_FillAmount", newFillAmount);

        for (int i = 0; i < GameLogic.BottleCapacity; i++)
        {
            instanceMaterial.SetColor("_C" + (i + 1), bottleColors.colors[colorIndices[i]]);
        }
    }

    public bool CheckEmpty()
    {
        return colorIndices[0] == 0;
    }

    public bool CheckFull()
    {
        return colorIndices[colorIndices.Length - 1] != 0;
    }

    void Update()
    {
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
        if (movingStep == 2) // rotating to pour
        {
            RotateToPour();
        }
        if (movingStep == 3) // rotating to upright position
        {
            RotateBack();
        }
        if (movingStep == 4 & transform.position != startPosition) // moving back to original position
        {
            transform.position = goTo(startPosition, 3.0f);
            if (transform.position == startPosition)
            {
                instanceMaterial.SetFloat("_SARM", 1.0f);
                movingStep = 0;
                SetSelected(false);
            }
        }

        if (movingStep != 0)
        {
            instanceMaterial.SetFloat("_SARM", 1.0f - (0.7f * (direction == -1.0f ? (360.0f - transform.eulerAngles.z) / 90.0f : transform.eulerAngles.z / 90.0f)));
        }
    }

    Vector3 goTo(Vector3 targetPosition, float movementSpeed) // increment bottle position towards target position
    {
        return Vector3.MoveTowards(transform.position, targetPosition, movementSpeed);
    }

    void RotateToPour()
    {
        transform.RotateAround(rotationPivot, Vector3.forward, direction * rotationSpeed * Time.deltaTime); //rotate around above other bottle center point

        rotationStart = waterPivots[currentWater - 1];
        rotationEnd = currentWater - layersToPour == 0 ? waterPivots[1] - (bottleMaskSR.bounds.size.y / 2.0f) : waterPivots[currentWater - layersToPour - 1];

        float fillChange = fillAmount - (layersToPour * layerHeight * Math.Clamp((fillDelta - (rotationPivot.y - rotationEnd)) / fillDelta, 0.0f, 1.0f));
        instanceMaterial.SetFloat("_FillAmount", fillChange);

        if (rotationStart > rotationPivot.y && !rotationStarted)
        {
            rotationStarted = true;

            fillDelta = rotationPivot.y - rotationEnd;

            rotationSpeed = 30.0f;
        }
        if (rotationEnd > rotationPivot.y)
        {
            rotationStarted = false;

            fillAmount = fillAmount - layersToPour * layerHeight;
            instanceMaterial.SetFloat("_FillAmount", fillAmount);

            rotationSpeed = 120.0f;

            movingStep = 3;
        }
    }

    void RotateBack()
    {
        transform.RotateAround(rotationPivot, Vector3.forward, -1.0f * direction * rotationSpeed * Time.deltaTime);

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

    public void PourTo(Vector3 target, int layersToPour) // initiate pouring animation
    {
        direction = transform.position.x - target.x > 0 ? 1.0f : -1.0f;
        targetPosition = target + (transform.right * (direction * (bottleMaskSR.bounds.size.x / 2.0f)) + transform.up * bottleMaskSR.bounds.size.y / 2.0f);
        rotationPivot = target + (transform.up * (bottleMaskSR.bounds.size.y));
        this.layersToPour = layersToPour;
        targetRotation = (direction < 0 ? (360.0f - 90.0f) : 90.0f);
        movingStep = 1;
    }





    public void SetColorAt(int index, int color)
    {
        colorIndices[index] = color;
    }

    public void SetPool(ObjectPool<BottleController> pool)
    {
        _pool = pool;
    }

    public int GetTopColor()
    {
        // Find the topmost color that isn't zero and return it.
        for (int i = colorIndices.Length - 1; i >= 0; i--)
        {
            if (colorIndices[i] != 0)
                return colorIndices[i];
        }
        return 0; // If no color is found, return 0.
    }

}
