using UnityEngine;

public class HotkeyMenuController : MonoBehaviour
{
    [SerializeField]GameObject menu;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            menu.SetActive(!menu.activeSelf);
    }
}
