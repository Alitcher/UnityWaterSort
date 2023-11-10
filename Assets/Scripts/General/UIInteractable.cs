using UnityEngine;
using UnityEngine.EventSystems;

public class UIInteractable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Cursors
    public Texture2D CursorPointer;
    public Vector2 CursorPointerHotspot = new Vector2(25, 20);
    public Texture2D CursorDefault;
    public Vector2 CursorDefaultHotspot = Vector2.zero;

    public void OnPointerEnter(PointerEventData data)
    {
        Cursor.SetCursor(CursorPointer, CursorPointerHotspot, CursorMode.ForceSoftware);
    }

    public void OnPointerExit(PointerEventData data)
    {
        Cursor.SetCursor(CursorDefault, CursorDefaultHotspot, CursorMode.Auto);
    }

    private void OnDisable()
    {
        Cursor.SetCursor(CursorDefault, CursorDefaultHotspot, CursorMode.Auto);
    }
}
