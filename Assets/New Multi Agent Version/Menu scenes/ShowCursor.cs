using UnityEngine;

public class ShowCursor : MonoBehaviour
{
    private void Start()
    {
        // Unlock the cursor
        Cursor.lockState = CursorLockMode.None;

        // Make the cursor visible
        Cursor.visible = true;
    }
}