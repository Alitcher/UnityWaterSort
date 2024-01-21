using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;

    // Buttons
    [SerializeField] private Button StartButton;
    [SerializeField] private Button QuitButton;

    [SerializeField] private Color BGWaterColor;

    [SerializeField] private TextMeshProUGUI versionText;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        StartButton.onClick.AddListener(() => SceneManager.LoadScene("Game"));
        QuitButton.onClick.AddListener(() => Application.Quit());
    }

    private void Start()
    {
        versionText.text = $"version: {Application.version}";
        Events.ChangeBGColor(BGWaterColor);
    }
}
