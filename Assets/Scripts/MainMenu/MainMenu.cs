using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;

    // Buttons
    public Button StartButton;
    public Button QuitButton;

    // Cursors
    public Texture2D CursorDefault;
    public Vector2 CursorDefaultHotspot = Vector2.zero;

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
