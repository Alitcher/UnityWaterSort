using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Level", menuName = "Watersort/Level/newLevel")]
public class Level : ScriptableObject
{
    public string LevelId;
    public int BottleCount;
    public Vector3[] BottlePosition; // Array size must equal to BottleCount

    public int colorCount; // ColorCount is never more than BottleCount - 1
}
