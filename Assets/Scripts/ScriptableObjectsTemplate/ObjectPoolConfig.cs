using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Object Pool", menuName = "Watersort/Util/newObjectPool")]
public class ObjectPoolConfig : ScriptableObject
{
    public int defaultCapacity;
    public int maxSize;
}