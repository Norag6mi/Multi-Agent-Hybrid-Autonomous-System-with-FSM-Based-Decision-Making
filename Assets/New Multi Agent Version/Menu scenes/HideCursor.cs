using UnityEngine;

public class HideCursor : MonoBehaviour
{
    void Start()
    {
        // Locks the cursor to the center of the game screen
        Cursor.lockState = CursorLockMode.Locked; 
        
        // Makes the mouse pointer invisible during gameplay
        Cursor.visible = false;                 
    }
}