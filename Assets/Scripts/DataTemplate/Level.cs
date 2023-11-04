using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Level", menuName = "Watersort/Level/newLevel")]
public class Level : ScriptableObject
{
    public string LevelId;
    public int BottleCount;

    [SerializeField]
    private int colorCount;

    public int ColorCount
    {
        get { return colorCount; }
        set
        {
            // Ensure ColorCount is never more than BottleCount - 1
            colorCount = Mathf.Clamp(value, 1, BottleCount - 1);
        }
    }
}
