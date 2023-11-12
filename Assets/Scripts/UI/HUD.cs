using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private GameObject EscMenu;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleEscMenu();
        }
    }

    public void ToggleEscMenu()
    {
        EscMenu.SetActive(!EscMenu.activeSelf);
    }
}
