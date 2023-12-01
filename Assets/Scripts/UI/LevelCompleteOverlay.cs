using UnityEngine.UI;
using UnityEngine;

public class LevelCompleteOverlay : MonoBehaviour
{
    public static LevelCompleteOverlay Instance;

    // Buttons
    [SerializeField] private Button NextButton;

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        NextButton.onClick.AddListener(() => Events.GoToNextLevel());
    }
}
