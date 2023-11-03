using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GameLogic : MonoBehaviour
{

    private bool bottleSelected = false;
    private BottleController selectedBottle;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            RaycastHit hit = new RaycastHit();
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
                        selectedBottle.pourTo(hit.collider.gameObject.transform.position);
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
}
