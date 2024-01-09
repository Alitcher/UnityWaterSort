using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameLogic : MonoBehaviour
{
    public static GameLogic Instance;

    private GameState gameState;
    private int currentLevel;

    /// <summary>
    /// Distance of bottles from the camera along the Z-axis
    /// </summary>
    public int bottleDistance;
    public const int BottleCapacity = 4;
    [SerializeField] private ObjectPoolConfig objectPoolConfig;
    [SerializeField] private LevelHolder levelsCollection;
    [SerializeField] private BottlePooler pooler;

    private int bottleOrder = 0;

    private bool bottleSelected = false;
    private BottleController selectedBottle;
    private BottleController secondSelectedBottle;

    [SerializeField] private List<int> colorIndiciesPool = new();
    [SerializeField] private List<BottleController> bottleGameCollection;


    #region Reset these value when reset the game
    private int bottleCompleteCount = 0;
    #endregion

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;

        Events.OnGameStateChange += CheckGameState;
        Events.OnGoToNextLevel += GoToNextLevel;
    }

    private void Start()
    {
        gameState = GameState.Instance;
        currentLevel = gameState.GetCurrentLevel();
        transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z + bottleDistance);

        Events.ChangeBGColor(levelsCollection.LevelCollection[currentLevel].BGWaterColor);
        DestroyAllBottles(); // destroy all bottles before restart the scene
        GenerateLevel(); // make new bottles
        Events.ChangeBottleMaterial(GameState.Instance.GetCurrentShader());
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleBottleMovement();
        }

        // Material change

        if (Input.GetKeyDown(KeyCode.Alpha1)) Events.ChangeBottleMaterial(1); //basic
        if (Input.GetKeyDown(KeyCode.Alpha2)) Events.ChangeBottleMaterial(2); //metallic
        if (Input.GetKeyDown(KeyCode.Alpha3)) Events.ChangeBottleMaterial(3); //glittery

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
            Vector2 xyPos = levelsCollection.LevelCollection[currentLevel].BottlePosition[i];
            bottle.transform.position = new Vector3(xyPos.x, xyPos.y, transform.position.z);
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

        // Because the game could be done or have no moves also at the start
        CheckGameState();
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

                        if (!(secondSelectedBottle.CheckFull() || !CheckValidPour(selectedBottle, secondSelectedBottle)))
                        {
                            selectedBottle.SetOrderInLayer(bottleOrder);
                            selectedBottle.StartPouringAnimation(secondSelectedBottle, selectedBottle.LayersToPour());
                            selectedBottle.SetSelected(false);
                            bottleSelected = false;

                            if (secondSelectedBottle.CheckBottleComplete())
                            {
                                print(secondSelectedBottle.name + " completed!");
                                bottleCompleteCount++;
                            }
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

    bool CheckValidPour(BottleController sourceBottle, BottleController destinationBottle)
    {
        return sourceBottle != destinationBottle && // not pouring to itself
            (sourceBottle.TopColor() == destinationBottle.TopColor() || destinationBottle.CheckEmpty()) && // color same or destination empty
            sourceBottle.LayersToPour() + destinationBottle.GetCurrentWater() <= BottleCapacity && // destination has space
            (!sourceBottle.CheckBottleComplete()); // source isn't already complete
    }

    /// <summary>
    /// Performs checks necessary after game state changes, such as pouring and starting a new level
    /// </summary>
    void CheckGameState()
    {
        CheckIfGameFinished();
        CheckNoMoreMoves();
    }

    void CheckIfGameFinished()
    {
        if (GameFinished()) 
        {
            print("Game Clear!!");
            StartCoroutine(HUD.Instance.ShowLevelCompleteOverlayCoroutine());
        }
    }

    bool GameFinished()
    {
        return bottleCompleteCount == levelsCollection.LevelCollection[currentLevel].colorCount;
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



    void CheckNoMoreMoves()
    {
        foreach (BottleController sourceBottle in bottleGameCollection)
        {
            foreach (BottleController destinationBottle in bottleGameCollection)
            {
                if (CheckValidPour(sourceBottle, destinationBottle))
                {
                    return;
                }
            }
        }

        if (!GameFinished())
        {
            print("No More Moves :(");
            StartCoroutine(HUD.Instance.ShowNoMoreMovesOverlayCoroutine());
        }
    }

    private void OnDestroy()
    {
        Events.OnGameStateChange -= CheckGameState;
        Events.OnGoToNextLevel -= GoToNextLevel;
    }
}
