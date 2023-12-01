using System;
using UnityEngine;

public static class Events
{
    // End of level
    public static event Action OnGoToNextLevel;
    public static void GoToNextLevel() => OnGoToNextLevel?.Invoke();
}

