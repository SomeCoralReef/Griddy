using UnityEngine;
using TMPro;
public class UI_DebugScreens : MonoBehaviour
{
    public TextMeshProUGUI pausedDebugText; // Reference to the TextMeshProUGUI component for displaying debug information
    public TimelineManager timelineManager; // Reference to the TimelineManager to check pause state

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (pausedDebugText == null)
        {
            Debug.LogError("PausedDebugText reference is not set in the UI_DebugScreens.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(timelineManager.isPaused)
        {
            pausedDebugText.text = "Game Paused";
        }
        else
        {
            pausedDebugText.text = "";
        }
    }
}
