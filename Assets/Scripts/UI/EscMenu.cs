using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EscMenu : MonoBehaviour
{
    // Buttons
    [SerializeField] private Button MenuButton;
    [SerializeField] private Button QuitButton;

    private void Awake()
    {
        MenuButton.onClick.AddListener(() => BackToMenu());
        QuitButton.onClick.AddListener(() => QuitGame());
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
