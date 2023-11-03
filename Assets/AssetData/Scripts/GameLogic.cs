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
            if (Physics.Raycast(ray, out hit))
            {
                if (!(hit.collider.gameObject == this.gameObject))
                {
                    if (bottleSelected)
                    {
                        if (selectedBottle == hit.collider.gameObject.GetComponent<BottleController>())
                        {
                            selectedBottle.SetSelected(false);
                            bottleSelected = false;
                        }
                        else
                        {
                            selectedBottle.pourTo(hit.collider.gameObject.transform.position);
                            selectedBottle.SetSelected(false);
                            bottleSelected = false;
                        }

                    }
                    else
                    {
                        selectedBottle = hit.collider.gameObject.GetComponent<BottleController>();
                        selectedBottle.SetSelected(true);
                        bottleSelected = true;
                    }
                }
            }
            else
            {
                if (bottleSelected)
                {
                    selectedBottle.SetSelected(false);
                    bottleSelected = false;
                }
            }
        }
    }
}
