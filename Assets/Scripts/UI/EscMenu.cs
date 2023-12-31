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

        RestartButton.onClick.AddListener(() => SceneManager.LoadScene("Game"));
        MenuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        QuitButton.onClick.AddListener(() => Application.Quit());
    }
}
