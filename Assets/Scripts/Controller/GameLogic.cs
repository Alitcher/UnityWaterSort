using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class GameLogic : MonoBehaviour
{
    public static GameLogic Instance;

    private GameState gameState;
    private int currentLevel;

    public const int BottleCapacity = 4;
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

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;

        Events.OnGoToNextLevel += GoToNextLevel;
    }

    private void Start()
    {
        gameState = GameState.Instance;
        currentLevel = gameState.GetCurrentLevel();
        DestroyAllBottles(); // destroy all bottles before restart the scene
        GenerateLevel();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleBottleMovement();
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Game");
        }
        #endregion
    }

    public void GenerateLevel()
    {
        int bottleCount = levelsCollection.LevelCollection[currentLevel].BottleCount;
        int colorCount = levelsCollection.LevelCollection[currentLevel].colorCount;
        int numberOfEmptyBottles = bottleCount - colorCount;
        
        GenerateColorsForLevel(colorCount);

        // Generate bottles
        for (int i = 0; i < bottleCount; i++)
        {
            // Try to reuse an object from the pool
            BottleController bottle = pooler.objectPool.Get();
            // Set name and position
            bottle.name = "bottle-" + i;
            bottle.transform.position = levelsCollection.LevelCollection[currentLevel].BottlePosition[i];
            if (i < colorCount)
            {
                SetColorIndicies(bottle);
                bottle.GenerateColor(BottleCapacity);
            } else
            {
                bottle.GenerateColor(0);
            }
            bottleGameCollection.Add(bottle);
        }

        CheckIfGameFinished(); // Because there is a chance it already is
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
            int selectedIndex = UnityEngine.Random.Range(0, colorIndiciesPool.Count);
            bottle.SetColorAt(i, colorIndiciesPool[selectedIndex]);
            colorIndiciesPool.RemoveAt(selectedIndex);
        }
        if (bottle.CheckBottleComplete())
        {
            bottleCompleteCount++; // For when a bottle is already complete at start
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
        if (!EventSystem.current.IsPointerOverGameObject()) // If not over UI that's not meant to be clicked through
        {
            //RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) // check if ray hit a gameobject with a collider
            {
                BottleController hitBottle = hit.collider.gameObject.GetComponent<BottleController>();

                if (bottleSelected) // check if a bottle is already selected
                {
                    if (selectedBottle == hitBottle) // check if ray hit the bottle that is already selected
                    {
                        // Unselect the currently selected bottle
                        UnSelectSelectedBottle();
                    }
                    else if (secondSelectedBottle != hitBottle && !hitBottle.IsPouring()) // Pour
                    {
                        secondSelectedBottle = hitBottle;

                        if (!(secondSelectedBottle.CheckFull() || !CheckValidPour()))
                        {
                            selectedBottle.SetOrderInLayer(bottleOrder);
                            selectedBottle.StartPouringAnimation(secondSelectedBottle, layersToPour);
                            selectedBottle.SetSelected(false);
                            bottleSelected = false;

                            if (secondSelectedBottle.CheckBottleComplete())
                            {
                                print(secondSelectedBottle.name + " completed!");
                                bottleCompleteCount++;
                            }

                            CheckIfGameFinished();
                        }
                    }
                }
                else
                {
                    // Select the bottle that the ray hit
                    if (selectedBottle == null) // First pick
                    {
                        if (!hitBottle.CheckEmpty()) SetSelectedBottle(hitBottle);
                    }
                    else
                    {
                        bool A = hitBottle != selectedBottle && hitBottle != secondSelectedBottle;
                        bool B = (hitBottle == selectedBottle || hitBottle == secondSelectedBottle) && !selectedBottle.IsPouring();
                        if ((A || B) && !hitBottle.CheckEmpty())
                        {
                            secondSelectedBottle = null;
                            SetSelectedBottle(hitBottle);
                        }
                    }
                }
            }
            else if (bottleSelected) // Check if a bottle is already selected
            {
                // Unselect the currently selected bottle
                UnSelectSelectedBottle();
            }
        }

        
    }

    void SetSelectedBottle(BottleController hitBottle)
    {
        bottleOrder += 1;
        selectedBottle = hitBottle;
        layersToPour = selectedBottle.LayersToPour();
        selectedBottle.SetSelected(true);
        bottleSelected = true;
    }

    void UnSelectSelectedBottle()
    {
        bottleOrder -= 1;
        if (selectedBottle) selectedBottle.SetSelected(false);
        bottleSelected = false;
        selectedBottle = null;
    }

    bool CheckValidPour()
    {
        return ((selectedBottle.TopColor() == secondSelectedBottle.TopColor() || secondSelectedBottle.CheckEmpty()) &&
               layersToPour + secondSelectedBottle.GetCurrentWater() <= BottleCapacity) &&
               (!selectedBottle.CheckBottleComplete());
    }

    void CheckIfGameFinished()
    {
        if (bottleCompleteCount == levelsCollection.LevelCollection[currentLevel].colorCount) 
        {
            print("Game Clear!!");
            StartCoroutine(HUD.Instance.ShowLevelCompleteOverlayCoroutine());
        }
    }

    public void GoToNextLevel()
    {
        Debug.Log("Next Level soon!");
        UnSelectSelectedBottle();
        if (currentLevel < levelsCollection.LevelCollection.Length - 1) // Repeat lvl if no more lvls
        {
            gameState.IncreaseCurrentLevel();
        }
        SceneManager.LoadScene("Game");
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
    private void OnDestroy()
    {
        Events.OnGoToNextLevel -= GoToNextLevel;
    }
}
