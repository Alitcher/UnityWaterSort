using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NoMoreMovesOverlay : MonoBehaviour
{
    public static NoMoreMovesOverlay Instance;

    // Buttons
    [SerializeField] private Button RestartButton;

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        RestartButton.onClick.AddListener(() => SceneManager.LoadScene("Game"));
    }
}
