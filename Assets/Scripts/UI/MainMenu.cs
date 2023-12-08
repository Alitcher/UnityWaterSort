using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;

    // Buttons
    [SerializeField] private Button StartButton;
    [SerializeField] private Button QuitButton;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        StartButton.onClick.AddListener(() => StartGame());
        QuitButton.onClick.AddListener(() => QuitGame());

    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
