using System.Collections;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public static HUD Instance;

    [SerializeField] private GameObject EscMenu;
    [SerializeField] private GameObject LevelCompleteOverlay;
    [SerializeField] private TMP_Text LevelText;
    
    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        LevelText.text = "Level " + GameState.Instance.GetCurrentLevel();
    }

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

    public IEnumerator ShowLevelCompleteOverlayCoroutine()
    {
        yield return new WaitForSeconds(2);
        LevelCompleteOverlay.SetActive(true);
    }
}
