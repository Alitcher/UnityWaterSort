using UnityEngine;

[CreateAssetMenu(fileName = "new Level", menuName = "Watersort/Level/newLevel")]
public class Level : ScriptableObject
{
    public string LevelId;
    public Color BGWaterColor;
    public int BottleCount;
    public Vector2[] BottlePosition; // Array size must equal to BottleCount

    public int colorCount; // ColorCount is never more than BottleCount - 1
}
