using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameLogic : MonoBehaviour
{
    public const int BottleCapacity = 4;
    public static int CurrentLevel = 3;
    [SerializeField] private ObjectPoolConfig objectPoolConfig;
    [SerializeField] private LevelHolder levelsCollection;
    [SerializeField] private BottlePooler pooler;

    private int bottleOrder = 0;

    private bool bottleSelected = false;
    private BottleController selectedBottle;
    private BottleController secondSelectedBottle;

    [SerializeField] private List<int> colorIndiciesPool = new List<int>();
    [SerializeField] private List<BottleController> bottleGameCollection;


    #region Reset these value when reset the game
    private int bottleCompleteCount = 0;
    #endregion
    private void Start()
    {
        DestroyAllBottles(); // destroy all bottles before restart the scene
        GenerateLevel();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleBottleMovement();
        }
        Debug.Log(bottleOrder);
        #region Debug Control
        if (Input.GetMouseButtonDown(1))
        {
            DestroyAllBottles();
        }
        if (Input.GetMouseButtonDown(2))
        {
            GenerateLevel();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Game");
        }
        #endregion
    }

    public void GenerateLevel()
    {
        int bottleCount = levelsCollection.LevelCollection[CurrentLevel].BottleCount;
        int colorCount = levelsCollection.LevelCollection[CurrentLevel].colorCount;
        int numberOfEmptyBottles = bottleCount - colorCount;
        
        GenerateColorsForLevel(colorCount);

        // Generate bottles
        for (int i = 0; i < bottleCount; i++)
        {
            // Try to reuse an object from the pool
            BottleController bottle = pooler.objectPool.Get();
            // Set name and position
            bottle.name = "bottle-" + i;
            bottle.transform.position = levelsCollection.LevelCollection[CurrentLevel].BottlePosition[i];
            if (i < levelsCollection.LevelCollection[CurrentLevel].BottleCount - numberOfEmptyBottles)
            {
                SetColorIndicies(bottle);
                bottle.GenerateColor(BottleCapacity);
            } else
            {
                bottle.GenerateColor(0);
            }
            bottleGameCollection.Add(bottle);
        }
    }

    public void GenerateColorsForLevel(int colorCount) 
    {
        for (int i = 0; i < BottleCapacity; i++)
        {
            for (int c = 1; c <= colorCount; c++)
            {
                colorIndiciesPool.Add(c);
            }
        }
    }

    public void ClearColorsLevel() 
    {
        colorIndiciesPool.Clear();
    }

    public void SetColorIndicies(BottleController bottle)
    {
        for (int i = 0; i < BottleCapacity; i++)
        {
            int selectedIndex = Random.Range(0, colorIndiciesPool.Count);
            bottle.SetColorAt(i, colorIndiciesPool[selectedIndex]);
            colorIndiciesPool.RemoveAt(selectedIndex);
        }
    }

    public void DestroyAllBottles()
    {
        for (int i = 0; i < bottleGameCollection.Count; i++)
        {
            pooler.objectPool.Release(bottleGameCollection[i]);
        }

        bottleGameCollection.Clear();
    }

    public int layersToPour;

    void HandleBottleMovement() // initiates pouring animation and selects/unselects bottles based on raycasts
    {
        //RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) // check if ray hit a gameobject with a collider
        {
            BottleController hitBottle = hit.collider.gameObject.GetComponent<BottleController>();

            if (bottleSelected) // check if a bottle is already selected
            {
                if (selectedBottle == hitBottle) // check if ray hit the bottle that is already selected
                { // unselect the currently selected bottle
                    bottleOrder -= 1;
                    selectedBottle.SetSelected(false);
                    bottleSelected = false;
                    selectedBottle = null;
                }
                else if (secondSelectedBottle != hitBottle && !hitBottle.pouring)// Pour
                {
                    secondSelectedBottle = hitBottle;

                    if (secondSelectedBottle.CheckFull())
                    {
                        return;
                    }

                    if (!CheckValidPour()) 
                    {
                        return;
                    }
                    selectedBottle.SetOrderInLayer(bottleOrder);
                    selectedBottle.PourTo(secondSelectedBottle.gameObject.transform.position, layersToPour, secondSelectedBottle);
                    selectedBottle.SetSelected(false);
                    bottleSelected = false;

                    if (secondSelectedBottle.CheckBottleComplete()) 
                    {
                        print(secondSelectedBottle.name + " completed!");
                        bottleCompleteCount++;
                    }

                }
            }
            else
            { // select the bottle that the ray hit
                if (selectedBottle == null)
                {
                    bottleOrder += 1;
                    selectedBottle = hitBottle;
                    layersToPour = selectedBottle.GetLayersToPour();
                    selectedBottle.SetSelected(true);
                    bottleSelected = true;
                } 
                else
                {
                    bool A = hitBottle != selectedBottle && hitBottle != secondSelectedBottle;
                    bool B = (hitBottle == selectedBottle || hitBottle == secondSelectedBottle) && !selectedBottle.pouring;
                    if ((A || B) && !hitBottle.CheckEmpty())
                    {
                        bottleOrder += 1;
                        secondSelectedBottle = null;
                        selectedBottle = hitBottle;
                        layersToPour = selectedBottle.GetLayersToPour();
                        selectedBottle.SetSelected(true);
                        bottleSelected = true;
                    }
                }
            }
        }
        else if (bottleSelected) // check if a bottle is already slected
        { // unselect the currently selected bottle
            bottleOrder -= 1;
            selectedBottle.SetSelected(false);
            bottleSelected = false;
            selectedBottle = null;
        }

    }

    bool CheckValidPour()
    {
        return (selectedBottle.GetTopColor() == secondSelectedBottle.GetTopColor() &&
               layersToPour + secondSelectedBottle.GetCurrentWater <= BottleCapacity) ||
               secondSelectedBottle.CheckEmpty() || !selectedBottle.CheckBottleComplete();
    }

    bool CheckGameFinished()
    {
        if (bottleCompleteCount == levelsCollection.LevelCollection[CurrentLevel].colorCount) 
        {
            print("Game Clear!!");
            return true;
        }
        return false;
    }



    bool CheckNoMoreMove() //optional
    {
        /*
        TODO: Check if there are no more moves left in the game.
        1. Iterate through each pair of bottles on the game board (source and destination).
        2. For each pair, check if you can pour from the source bottle to the destination bottle using the CheckValidPour() function. 
            2.1. If you can, it means there's a valid move, and it returns False.
            2.2. If it goes through all pairs and finds no valid moves, it means the game has reached a point where you can't make any more moves, so it returns True.
        */
        return false;
    }


}
