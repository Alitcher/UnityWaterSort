using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public class GameLogic : MonoBehaviour
{
    public int CurrentLevel = 0;
    [SerializeField] private ObjectPoolConfig objectPoolConfig;
    [SerializeField] private LevelHolder levelsCollection;


    private bool bottleSelected = false;
    private BottleController selectedBottle;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            handleBottleMovement();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateLevel();
        }
    }

    public void GenerateLevel()
    {
        //for (int i = 0; i < levelData.BottleCount; i++)
        //{
        //    // Try to reuse an object from the pool
        //    BottleController bottle = objectPool.Get();

        //    if (bottle != null)
        //    {
        //        // Reset any necessary properties or behaviors on the bottle
        //        bottle.transform.position = transform.position;
        //        bottle.transform.rotation = transform.rotation;
        //        bottle.gameObject.SetActive(true);
        //    }
        //}
    }

    void handleBottleMovement() // initiates pouring animation and selects/unselects bottles based on raycasts
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)) // check if ray hit a gameobject with a collider
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
                    print("tarrget" + hit.collider.gameObject.transform.position);
                    bottleSelected = false;
                }
            }
            else
            { // select the bottle that the ray hit
                selectedBottle = hit.collider.gameObject.GetComponent<BottleController>();
                selectedBottle.SetSelected(true);
                bottleSelected = true;
            }
        }
        else if (bottleSelected) // check if a bottle is already slected
        { // unselect the currently selected bottle
            selectedBottle.SetSelected(false);
            bottleSelected = false;
        }
    }
}
