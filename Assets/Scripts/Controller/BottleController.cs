using System;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Pool;


public class BottleController : MonoBehaviour
{
    [SerializeField] private LineRenderer liquidLineRenderer;
    [SerializeField] private float liquidWidth;
    public ColorSet bottleColors;
    public SpriteRenderer bottleMaskSR;

    [NonSerialized] public Material instanceMaterial;

    private bool pouring = false; // true while selected bottles pouringAnimationStep != 0
    private bool selected = false; // true when selected, false when pouringAnimationStep != 0
    private bool pouringRotationStarted = false; // true while water is pouring during rotation animation

    private int pouringAnimationStep = 0; // steps of pouring animation, goes between 1 - 4, 0 when no animation
    private int currentWater; // levels of water, goes between 0 - 4
    private int layersToPour; // number of water layers to pour during one pouring animation, goes between 1 - 4

    private float directionOfRotation; // direction of rotation, -1 if to the right, 1 if to the left
    private float rotationSpeed = 180.0f; // number of degrees to rotate per second
    private float fillAmount; // amount of water shown by the shadermaterial, goes between -15.0(-20.0) and 10.0
    private float fillDelta; // distance between two y-axis coordinates, that dictate when water is poured during the animation
    private float layerHeight = 6.25f; // height of one water layer in the shadermaterial

    private Vector3 startPosition; // original position
    private Vector3 selectedPosition; // position when selected
    private Vector3 targetPosition; // position to move to before rotation during pouring animation
    private Vector3 rotationPivot; // point to rotate around during animation

    public GameObject[] waterPivots = new GameObject[4]; // points on the water layers to decide when water is poured out

    private BottleController targetBottle; // BottleController component of other bottle that the water is poured into form this bottle
    private ObjectPool<BottleController> _pool;

    [SerializeField] private int[] colorIndices = { 0, 0, 0, 0 }; //check ColorSet scriptableObject the number is equivalent to the index of the color.

    [SerializeField] private ParticleSystem bottleCompleteParticles;


    private void Awake()
    {
        instanceMaterial = bottleMaskSR.material;
        fillAmount = instanceMaterial.GetFloat("_FillAmount");

        Events.OnChangeBottleMaterial += ChangeMaterial;
    }

    void Start()
    {
        startPosition = transform.position * 1.0f;
        selectedPosition = new Vector3(startPosition.x, startPosition.y + 10f, startPosition.z);
    }

    void Update()
    {
        if (selected & transform.position != selectedPosition & pouringAnimationStep == 0) // moving up when selected
        {
            transform.position = goTo(selectedPosition, 100.0f);
        }
        if (!selected & transform.position != startPosition & pouringAnimationStep == 0) // moving down when unselected
        {
            transform.position = goTo(startPosition, 100.0f);
        }

        if (pouringAnimationStep == 1) // moving to other bottle
        {
            transform.position = goTo(targetPosition, 250.0f);
            if (transform.position == targetPosition)
            {
                pouringAnimationStep = 2;
            }
        }
        if (pouringAnimationStep == 2) // rotating to pour
        {
            RotateToPour(targetBottle);
            SetActiveLiquidLine();
        }
        if (pouringAnimationStep == 3) // rotating to upright position
        {
            RotateBack();
            liquidLineRenderer.enabled = false; 
        }
        if (pouringAnimationStep == 4 & transform.position != startPosition) // moving back to original position
        {
            transform.position = goTo(startPosition, 250.0f);
            if (transform.position == startPosition)
            {
                SetOrderInLayer(0);
                pouring = false;
                targetBottle.pouring = false;
                instanceMaterial.SetFloat("_SARM", 1.0f);
                pouringAnimationStep = 0;
                Events.GameStateChange();
            }
        }
        if (pouringAnimationStep == 2 || pouringAnimationStep == 3)
        {
            float normalizedRotationZ = directionOfRotation == -1.0f ? (360.0f - transform.eulerAngles.z) / 90.0f : transform.eulerAngles.z / 90.0f;
            instanceMaterial.SetFloat("_SARM", 1.0f - (0.7f * normalizedRotationZ));
        }
    }

    // pouring animation functions

    public void StartPouringAnimation(BottleController targetBottle, int layersToPour) // initiate pouring animation
    {
        this.targetBottle = targetBottle;
        this.layersToPour = layersToPour;

        pouring = true;
        targetBottle.pouring = true;
   
        directionOfRotation = transform.position.x - targetBottle.transform.position.x > 0 ? 1.0f : -1.0f;
        targetPosition = targetBottle.transform.position + (transform.right * (directionOfRotation * (bottleMaskSR.bounds.size.x / 2.0f)) + transform.up * bottleMaskSR.bounds.size.y / 2.0f);
        rotationPivot = targetBottle.transform.position + (transform.up * (float)Math.Round(bottleMaskSR.bounds.size.y));

        targetBottle.FillIn(TopColor(), layersToPour);
        targetBottle.UpdateShaderLayers();

        pouringAnimationStep = 1;
    }

    Vector3 goTo(Vector3 targetPosition, float movementSpeed) // increment bottle position towards target position
    {
        return Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
    }

    void RotateToPour(BottleController targetBottle)
    {
        transform.RotateAround(rotationPivot, Vector3.forward, directionOfRotation * rotationSpeed * Time.deltaTime);

        float rotationStart = waterPivots[currentWater - 1].transform.position.y;
        float rotationEnd = currentWater - layersToPour == 0 ? waterPivots[0].transform.position.y - (bottleMaskSR.bounds.size.y / 2.0f) : waterPivots[currentWater - layersToPour - 1].transform.position.y;

        float fillChange = (layersToPour * layerHeight * Math.Clamp((fillDelta - (rotationPivot.y - rotationEnd)) / fillDelta, 0.0f, 1.0f));
        float fillChangeThis = fillAmount - fillChange;
        float fillChangeOther = targetBottle.fillAmount + fillChange;

        instanceMaterial.SetFloat("_FillAmount", fillChangeThis);
        targetBottle.instanceMaterial.SetFloat("_FillAmount", fillChangeOther);


        if (rotationStart > rotationPivot.y && !pouringRotationStarted)
        {
            fillDelta = rotationPivot.y - rotationEnd;

            pouringRotationStarted = true;
            rotationSpeed = 60.0f;
        }
        if (rotationEnd > rotationPivot.y)
        {
            fillAmount -= layersToPour * layerHeight;
            instanceMaterial.SetFloat("_FillAmount", fillAmount == -15.0f ? -20.0f : fillAmount);
            targetBottle.fillAmount += layersToPour * layerHeight;
            targetBottle.instanceMaterial.SetFloat("_FillAmount", targetBottle.fillAmount);

            PourOut(layersToPour);
            UpdateShaderLayers();

            pouringRotationStarted = false;
            rotationSpeed = 180.0f;

            if (targetBottle.CheckBottleComplete())
            {
                targetBottle.bottleCompleteParticles.Play();
            }
            pouringAnimationStep = 3;
        }
    }

    void RotateBack()
    {
        transform.RotateAround(rotationPivot, Vector3.forward, -1.0f * directionOfRotation * rotationSpeed * Time.deltaTime);

        if (directionOfRotation == -1.0f)
        {
            if (transform.eulerAngles.z < 90.0f)
            {
                pouringAnimationStep = 4;
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            }
        }
        else
        {
            if (transform.eulerAngles.z > 270.0f)
            {
                pouringAnimationStep = 4;
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            }
        }
    }

    private void SetActiveLiquidLine()
    {
        liquidLineRenderer.startColor = bottleColors.colors[TopColor()]; // set color here
        liquidLineRenderer.endColor = bottleColors.colors[TopColor()];// set color here
        liquidLineRenderer.SetPosition(0, rotationPivot);
        liquidLineRenderer.SetPosition(1, rotationPivot - Vector3.up * liquidWidth);

        liquidLineRenderer.enabled = true;
    }





    // get-set functions

    public bool IsPouring() => pouring;

    public int GetCurrentWater() => currentWater;

    public void SetSelected(bool selected) => this.selected = selected;

    public void SetColorAt(int index, int color) =>  colorIndices[index] = color;

    public void SetPool(ObjectPool<BottleController> pool) => _pool = pool;

    public void SetOrderInLayer(int order)
    {
        GetComponent<SpriteRenderer>().sortingOrder = order;
        bottleMaskSR.sortingOrder = order;
    }









    //bottle controller functions

    public void GenerateColor(int layers)
    {
        currentWater = layers;

        instanceMaterial.SetFloat("_FillAmount", currentWater == 0 ? -15.0f : 10.0f); // replacement for resetShaderLayersFunction
        UpdateShaderLayers();

        fillAmount = instanceMaterial.GetFloat("_FillAmount");
    }

    public bool CheckEmpty()
    {
        return colorIndices[0] == 0;
    }

    public bool CheckFull()
    {
        return colorIndices[colorIndices.Length - 1] != 0;
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

    void FillIn(int fillColor, int layerCount)
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

    void PourOut(int layerCount)
    {
        for (int i = 1; i <= layerCount; i++)
        {
            colorIndices[currentWater - i] = 0;
        }
        currentWater -= layerCount;
    }

    public int LayersToPour()
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

    public int TopColor()
    {
        // Find the topmost color that isn't zero and return it.
        for (int i = colorIndices.Length - 1; i >= 0; i--)
        {
            if (colorIndices[i] != 0)
            {
                return colorIndices[i];
            }
        }
        return 0; // If no color is found, return 0.
    }

    void UpdateShaderLayers()
    {
        for (int i = 0; i < GameLogic.BottleCapacity; i++)
        {
            instanceMaterial.SetColor("_C" + (i + 1), bottleColors.colors[colorIndices[i]]);
        }
    }

    void ChangeMaterial(int matNr)
    {
        instanceMaterial.SetFloat("_MaterialNumber", matNr);
    }

    private void OnDestroy()
    {
        Events.OnChangeBottleMaterial -= ChangeMaterial;
    }
}
