using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;

    // Buttons
    [SerializeField] private Button StartButton;
    [SerializeField] private Button QuitButton;

    // Cursors
    [SerializeField] private Texture2D CursorDefault;
    [SerializeField] private Vector2 CursorDefaultHotspot = Vector2.zero;

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

        Cursor.SetCursor(CursorDefault, CursorDefaultHotspot, CursorMode.Auto);
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
