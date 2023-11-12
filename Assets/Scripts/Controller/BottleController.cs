using System;
using UnityEngine;
using UnityEngine.Pool;


public class BottleController : MonoBehaviour
{
    public ColorSet bottleColors;
    public SpriteRenderer bottleMaskSR;

    private Material instanceMaterial;

    [NonSerialized] public bool pouring = false;
    private bool selected = false;
    private bool rotationStarted = false;
    private int movingStep = 0;
    private float direction;
    private float targetRotation;
    private float rotationSpeed = 90.0f;
    private float rotationStart;
    private float rotationEnd;
    private float fillAmount;
    private float fillDelta;

    public Vector3 startPosition;
    private Vector3 selectedPosition;
    private Vector3 targetPosition;
    private Vector3 rotationPivot;

    public GameObject[] waterPivots = new GameObject[4];

    private BottleController pourToBottle; 

    private ObjectPool<BottleController> _pool;

    private int currentWater; // goes between 0 - 4
    private int layersToPour = 1;

    [SerializeField] private int[] colorIndices = { 0, 0, 0, 0 }; //check ColorSet scriptableObject the number is equivalent to the index of the color.

    private float layerHeight = 6.25f;

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
    }

    public void GenerateColor(int layers)
    {
        currentWater = layers;

        //ResetShaderLayers();
        instanceMaterial.SetFloat("_FillAmount", currentWater == 0 ? -15.0f : 10.0f); // replacement for resetShaderLayersFunction
        UpdateShaderLayers();

        fillAmount = instanceMaterial.GetFloat("_FillAmount");
    }

    public void SetFillIn(int fillColor, int layerCount)
    {
        for (int i = 0; i < layerCount; i++)
        {
            colorIndices[currentWater + i] = fillColor;
        }
        currentWater += layerCount;

        if (CheckBottleComplete())
        {
            print(this.name + " complete!");
        }
    }

    public void SetPourOut(int layerCount)
    {
        for (int i = 1; i <= layerCount; i++)
        {
            colorIndices[currentWater - i] = 0;
        }
        currentWater -= layerCount;
    }

    public void UpdateShaderLayers()
    {
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
            transform.position = goTo(targetPosition, 1.0f);
            if (transform.position == targetPosition)
            {
                movingStep = 2;
            }
        }
        if (movingStep == 2) // rotating to pour
        {
            RotateToPour(pourToBottle);
        }
        if (movingStep == 3) // rotating to upright position
        {
            RotateBack();
        }
        if (movingStep == 4 & transform.position != startPosition) // moving back to original position
        {
            transform.position = goTo(startPosition, 1.0f);
            if (transform.position == startPosition)
            {
                SetOrderInLayer(0);
                pouring = false;
                pourToBottle.pouring = false;
                instanceMaterial.SetFloat("_SARM", 1.0f);
                movingStep = 0;
            }
        }

        if (movingStep == 2 || movingStep == 3)
        {
            instanceMaterial.SetFloat("_SARM", 1.0f - (0.7f * (direction == -1.0f ? (360.0f - transform.eulerAngles.z) / 90.0f : transform.eulerAngles.z / 90.0f)));
        }
    }

    Vector3 goTo(Vector3 targetPosition, float movementSpeed) // increment bottle position towards target position
    {
        return Vector3.MoveTowards(transform.position, targetPosition, movementSpeed);
    }

    void RotateToPour(BottleController PourToBottle)
    {
        transform.RotateAround(rotationPivot, Vector3.forward, direction * rotationSpeed * Time.deltaTime); //rotate around above other bottle center point

        rotationStart = waterPivots[currentWater - 1].transform.position.y;
        rotationEnd = currentWater - layersToPour == 0 ? waterPivots[0].transform.position.y - (bottleMaskSR.bounds.size.y / 2.0f) : waterPivots[currentWater - layersToPour - 1].transform.position.y;
        float fillChange = (layersToPour * layerHeight * Math.Clamp((fillDelta - (rotationPivot.y - rotationEnd)) / fillDelta, 0.0f, 1.0f));
        float fillChangeThis = fillAmount - fillChange;
        float fillChangeOther = PourToBottle.fillAmount + fillChange;
        instanceMaterial.SetFloat("_FillAmount", fillChangeThis);
        PourToBottle.instanceMaterial.SetFloat("_FillAmount", fillChangeOther);


        if (rotationStart > rotationPivot.y && !rotationStarted)
        {
            fillDelta = rotationPivot.y - rotationEnd;

            rotationStarted = true;
            rotationSpeed = 30.0f;
        }
        if (rotationEnd > rotationPivot.y)
        {
            fillAmount -= layersToPour * layerHeight;
            instanceMaterial.SetFloat("_FillAmount", fillAmount == -15.0f ? -20.0f : fillAmount);
            PourToBottle.fillAmount += layersToPour * layerHeight;
            PourToBottle.instanceMaterial.SetFloat("_FillAmount", PourToBottle.fillAmount);

            SetPourOut(layersToPour);
            UpdateShaderLayers();

            rotationStarted = false;
            rotationSpeed = 90.0f;
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
        this.selected = selected;
    }

    public void SetOrderInLayer(int order)
    {
        GetComponent<SpriteRenderer>().sortingOrder = order;
        bottleMaskSR.sortingOrder = order;
    }

    public void PourTo(Vector3 target, int layersToPour, BottleController bo) // initiate pouring animation
    {
        pouring = true;
        bo.pouring = true;
        pourToBottle = bo;
        direction = transform.position.x - target.x > 0 ? 1.0f : -1.0f;
        targetPosition = target + (transform.right * (direction * (bottleMaskSR.bounds.size.x / 2.0f)) + transform.up * bottleMaskSR.bounds.size.y / 2.0f);
        rotationPivot = target + (transform.up * (float)Math.Round(bottleMaskSR.bounds.size.y));
        this.layersToPour = layersToPour;
        targetRotation = direction < 0 ? (360.0f - 90.0f) : 90.0f;
        movingStep = 1;
        bo.SetFillIn(GetTopColor(), layersToPour);
        bo.UpdateShaderLayers();
    }



    public bool CheckBottleComplete() 
    {
        int emptyColor = 0;
        int colorToCheck = colorIndices[0];

        for (int i = 1; i < colorIndices.Length; i++)
        {
            if (colorIndices[i] == emptyColor || colorIndices[i] != colorToCheck)
            {
                return false;
            }
        }

        return true;
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
