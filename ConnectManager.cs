using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Needed for TextMeshPro
using UnityEngine.SceneManagement;

public class ConnectManager : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Text statusText;      // Drag your Status Text here
    public Button connectButton;     // Drag your Connect Button here

    [Header("Settings")]
    public string nextSceneName = "LandingRoom"; // The exact name of your next scene
    public bool isTeacher = true;    // CHECK this box in Inspector to test as Teacher, UNCHECK for Student

    // This static variable lets other scripts know who logged in
    public static bool IsUserTeacher;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        statusText.text = "Ready to connect...";
        
        // Add the click listener via code (or you can do it in Inspector)
        connectButton.onClick.AddListener(OnConnectClicked);
    }

    public void OnConnectClicked()
    {
        // 1. Disable button so they don't click twice
        connectButton.interactable = false;

        // 2. Save the role (Teacher or Student) for the next scene
        IsUserTeacher = isTeacher;

        // 3. Start the connection simulation
        StartCoroutine(ConnectRoutine());
    }

    // Update is called once per frame
    IEnumerator ConnectRoutine()
    {
        // Step 1: Show "Connecting..."
        statusText.text = "Connecting to Photon...";
        statusText.color = Color.yellow;
        
        // Wait for 2 seconds (simulating network lag)
        yield return new WaitForSeconds(2.0f);

        // Step 2: Show "Connected"
        statusText.text = "Connected!";
        statusText.color = Color.green;

        // Wait 1 second so the user sees the success message
        yield return new WaitForSeconds(1.0f);

        // Step 3: Load the next scene
        SceneManager.LoadScene(nextSceneName);
    }
}
