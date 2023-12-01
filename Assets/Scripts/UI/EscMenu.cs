using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EscMenu : MonoBehaviour
{
    public static EscMenu Instance;

    // Buttons
    [SerializeField] private Button RestartButton;
    [SerializeField] private Button MenuButton;
    [SerializeField] private Button QuitButton;

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;

        RestartButton.onClick.AddListener(() => RestartGame());
        MenuButton.onClick.AddListener(() => BackToMenu());
        QuitButton.onClick.AddListener(() => QuitGame());
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Game");
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
