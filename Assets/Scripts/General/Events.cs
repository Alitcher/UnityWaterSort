using System;

public static class Events
{
    public static event Action<int> OnChangeBottleMaterial;
    public static void ChangeBottleMaterial(int value) => OnChangeBottleMaterial?.Invoke(value);

    // Any change of game state ie how liquids are positioned in bottles, including instancing liquids at start
    public static event Action OnGameStateChange;
    public static void GameStateChange() => OnGameStateChange?.Invoke();

    // End of level
    public static event Action OnGoToNextLevel;
    public static void GoToNextLevel() => OnGoToNextLevel?.Invoke();
}