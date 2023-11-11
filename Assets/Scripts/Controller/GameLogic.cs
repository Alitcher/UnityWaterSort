using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public class GameLogic : MonoBehaviour
{
    public static int CurrentLevel = 4;
    [SerializeField] private ObjectPoolConfig objectPoolConfig;
    [SerializeField] private LevelHolder levelsCollection;
    [SerializeField] private BottlePooler pooler;

    private bool bottleSelected = false;
    private bool pouring = false;
    private BottleController selectedBottle;

    private List<int> colorIndiciesPool = new List<int>();
   [SerializeField] private List<BottleController> bottleGameCollection;

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

        if (pouring && selectedBottle.transform.position == selectedBottle.startPosition)
        {
            bottleSelected = false;
            pouring = false;
        }

        #region Debug Control
        if (Input.GetMouseButtonDown(1))
        {
            DestroyAllBottles();
        }
        if (Input.GetMouseButtonDown(2))
        {
            GenerateLevel();
        }
        #endregion
    }

    public void GenerateLevel()
    {
        int bottleCount = levelsCollection.LevelCollection[CurrentLevel].BottleCount;
        int colorCount = levelsCollection.LevelCollection[CurrentLevel].colorCount;
        int numberOfEmptyBottles = bottleCount - colorCount;
        //colorPool = new List<Color>();
        for (int i = 0; i < 4; i++)
        {
            for (int c = 1; c <= colorCount; c++)
            {
                colorIndiciesPool.Add(c);
            }
        }
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
                bottle.GenerateColor(4);
            } else
            {
                bottle.GenerateColor(0);
            }
            bottleGameCollection.Add(bottle);
        }
    }

    public void SetColorIndicies(BottleController bottle)
    {
        for (int i = 0; i < 4; i++)
        {
            int selectedIndex = Random.Range(0, colorIndiciesPool.Count);
            bottle.colorIndices[i] = colorIndiciesPool[selectedIndex];
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

    void HandleBottleMovement() // initiates pouring animation and selects/unselects bottles based on raycasts
    {
        //RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && !pouring) // check if ray hit a gameobject with a collider
        {
            if (bottleSelected) // check if a bottle is already selected
            {
                if (selectedBottle == hit.collider.gameObject.GetComponent<BottleController>()) // check if ray hit the bottle that is already selected
                { // unselect the currently selected bottle
                    selectedBottle.SetSelected(false);
                    bottleSelected = false;
                }
                else
                { // initiate pouring animation from selected bottle to the bottle hit by ray
                    selectedBottle.PourTo(hit.collider.gameObject.transform.position);
                    pouring = true;
                }
            }
            else
            { // select the bottle that the ray hit
                selectedBottle = hit.collider.gameObject.GetComponent<BottleController>();
                if (!selectedBottle.CheckEmpty())
                {
                    selectedBottle.SetSelected(true);
                    bottleSelected = true;
                }
            }
        }
        else if (bottleSelected) // check if a bottle is already slected
        { // unselect the currently selected bottle
            selectedBottle.SetSelected(false);
            bottleSelected = false;
        }
    }

    bool CheckGameFinished()
    {
        /*
         TODO: Checks if the game is finished or not.
        1. Iterate at each bottle on the game board one by one.

        2. For each bottle, check if the color of the first ball (at the top of the bottle) is not 0 (which means there is a color).
            2.1. If the color is 0 (no color), it skips that bottle and goes to check the next one.
            2.2. If the color is not 0, it then checks all the colors and indices in that bottle.

        3. For each color index inside a bottle, If any indices in the bottle is not 0 and is not the same as the first index's color, 
           it means the bottle has mixed colors, and the game is not over, so it returns False.
        4. After checking all bottles, if no bottle has mixed colors, and each bottle has a single color or is empty, 
           it means the game is done, and it returns True.
         */
        return false;
    }

    bool CheckValidSelection()
    {
        /*
         * * Alicia
         TODO: Check for restrictions. The player can't pick a bottle if:
        1. The bottle is empty
        2. The bottle is being filled in by another bottle at the moment.
         */

        return false;
    }

    bool CheckValidPour()
    {
        /*
         * Alicia
         TODO: Check for restrictions. The source bottle cannot pour in the destination bottle if:
        1. The liquid color on top of destination bottle does not match the color on top of the source(selected) bottle.
        2. The destination bottle is full.

        HINT: Iterate through the source and destination bottle in from top most and find the first color that is not 0 (empty).

         */
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
