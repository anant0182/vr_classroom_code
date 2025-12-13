using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Required for loading scenes
using TMPro; // For setting status text, assuming you have one

public class RoomSelectionManager : MonoBehaviour
{
    // --- Public References ---
    [Header("Buttons")]
    public Button lectureRoomButton;
    public Button waitingRoomButton;

    [Header("UI Status")]
    // Add a reference to a Text or TextMeshPro component for status messages
    public TextMeshProUGUI statusText; 

    [Header("Scene Names")]
    public string lectureSceneName = "MainClassroom"; // Exact name of your classroom scene

    void Start()
    {
        // Check for necessary references
        if (lectureRoomButton == null) { Debug.LogError("RoomSelectionManager: lectureRoomButton is not assigned!"); return; }
        if (waitingRoomButton == null) { Debug.LogError("RoomSelectionManager: waitingRoomButton is not assigned!"); return; }
        if (statusText == null) { Debug.LogWarning("RoomSelectionManager: statusText is not assigned, status will not be displayed."); }

        // 1. Check user role. Assuming AuthData.Role is correctly set during login.
        bool isTeacher = AuthData.Role == "teacher";
        
        LogStatus($"User Role detected: {AuthData.Role}. Is Teacher: {isTeacher}");

        // 2. Configure the Lecture Room Button (Teacher's Entry Point)
        if (isTeacher)
        {
            // Teacher can access the Lecture Room
            lectureRoomButton.interactable = true;
            lectureRoomButton.onClick.AddListener(OnLectureRoomClicked);
            
            LogStatus("Lecture Room: INTERACTABLE. Click to start class.");
            
            // 3. Configure Waiting Room for Teacher: Should be non-interactable
            waitingRoomButton.interactable = false;
        }
        else // Role is Student or other
        {
            // Students cannot access the Lecture Room directly
            lectureRoomButton.interactable = false;
            
            // Students can access the Waiting Room
            waitingRoomButton.interactable = true;

            LogStatus("Lecture Room: DISABLED. Please enter the Waiting Room.");
        }
    }

    /// <summary>
    /// This function runs when the Teacher clicks the Lecture Room button.
    /// </summary>
    void OnLectureRoomClicked()
    {
        // This will run when the button is successfully clicked and the listener is active.
        LogStatus($"Loading Scene: {lectureSceneName}...");
        
        // This line loads the next scene. If this fails, check your Build Settings.
        SceneManager.LoadScene(lectureSceneName);
        
        // Note: Code execution stops here until the scene is loaded. 
        // If the scene name is wrong, Unity will throw an error in the console.
    }

    /// <summary>
    /// Helper to log messages to both the console and the UI status text.
    /// </summary>
    /// <param name="message">The message to display.</param>
    private void LogStatus(string message)
    {
        Debug.Log($"[RoomManager] {message}");
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}