using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new LevelHolder", menuName = "Watersort/Level/Holder")]
public class LevelHolder : ScriptableObject
{
    public Level[] LevelCollection; 
}
