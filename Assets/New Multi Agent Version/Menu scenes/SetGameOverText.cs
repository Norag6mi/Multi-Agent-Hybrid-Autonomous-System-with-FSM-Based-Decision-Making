using UnityEngine;
using TMPro; // Required for TextMeshPro

public class SetGameOverText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI myText; // Drag your text UI here

    void Start()
    {
        if (myText != null)
        {
            // Set the text to whatever was saved (WIN or LOOSE)
            myText.text = BombMissionManager.finalStatus;
        }
    }
}