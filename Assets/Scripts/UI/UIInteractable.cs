using UnityEngine;
using UnityEngine.EventSystems;

public class UIInteractable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Cursors
    [SerializeField] private Texture2D CursorPointer;
    [SerializeField] private Vector2 CursorPointerHotspot = new(25, 20);
    [SerializeField] private Texture2D CursorDefault;
    [SerializeField] private Vector2 CursorDefaultHotspot = Vector2.zero;

    public void OnPointerEnter(PointerEventData data)
    {
        Cursor.SetCursor(CursorPointer, CursorPointerHotspot, CursorMode.ForceSoftware);
    }

    public void OnPointerExit(PointerEventData data)
    {
        Cursor.SetCursor(CursorDefault, CursorDefaultHotspot, CursorMode.ForceSoftware);
    }

    private void OnDisable()
    {
        Cursor.SetCursor(CursorDefault, CursorDefaultHotspot, CursorMode.ForceSoftware);
    }
}
