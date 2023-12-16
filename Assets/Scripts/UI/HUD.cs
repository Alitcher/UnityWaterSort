using System.Collections;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public static HUD Instance;

    [SerializeField] private GameObject EscMenu;
    [SerializeField] private NoMoreMovesOverlay NoMoreMovesOverlay;
    [SerializeField] private LevelCompleteOverlay LevelCompleteOverlay;
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

    public IEnumerator ShowNoMoreMovesOverlayCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        NoMoreMovesOverlay.gameObject.SetActive(true);
    }

    public IEnumerator ShowLevelCompleteOverlayCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        LevelCompleteOverlay.PlayLevelCompleteParticles();
        LevelCompleteOverlay.gameObject.SetActive(true);
    }
}
