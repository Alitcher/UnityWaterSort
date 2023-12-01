using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    [SerializeField] private int _currentLevel = 1;

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public int GetCurrentLevel()
    {
        return _currentLevel;
    }
    
    public void IncreaseCurrentLevel() // I don't wanna call it GoToNextLevel because it doesn't actually initiate scene load
    {
        _currentLevel++;
        Debug.Log("Moving on to level " + _currentLevel);
    }
}
