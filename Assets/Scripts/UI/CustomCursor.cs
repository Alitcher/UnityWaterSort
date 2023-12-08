using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    public static CustomCursor Instance;
    
    [SerializeField] private Texture2D CursorDefault;
    [SerializeField] private Vector2 CursorDefaultHotspot = Vector2.zero;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        
        Cursor.SetCursor(CursorDefault, CursorDefaultHotspot, CursorMode.ForceSoftware);
    }
}
