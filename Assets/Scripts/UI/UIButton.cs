using UnityEngine.UI;

public class UIButton : UIInteractable
{
    private Button _button;
    
    private void Awake()
    {
        _button = GetComponent<Button>();
    }
}
