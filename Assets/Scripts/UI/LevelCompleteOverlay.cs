using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteOverlay : MonoBehaviour
{
    public static LevelCompleteOverlay Instance;

    // Buttons
    [SerializeField] private Button NextButton;

    [SerializeField] private ParticleSystem LevelCompleteParticles;

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        NextButton.onClick.AddListener(() => Events.GoToNextLevel());
    }

    public void PlayLevelCompleteParticles()
    {
        LevelCompleteParticles.Play();
    }
}
