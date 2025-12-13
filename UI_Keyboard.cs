using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.EventSystems; // Required for event handling

public class KeyboardToBoard : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TMP_InputField inputField; // The input bar above the keyboard
    public TMP_Text whiteboardDisplay; // The text on the big board
    public Button manualSendButton;    // Optional: A physical 'Send' button on UI

    void Start()
    {
        if (inputField != null)
        {
            // 1. Listen for the "Enter" key event from the Input Field itself
            inputField.onSubmit.AddListener(OnInputSubmit); 
            // Also listen for deselect/end edit as a backup
            inputField.onEndEdit.AddListener(OnInputSubmit);
        }

        if (manualSendButton != null)
        {
            manualSendButton.onClick.AddListener(() => OnInputSubmit(inputField.text));
        }
    }

    public void OnInputSubmit(string text)
    {
        // Prevent sending empty messages or newline-only messages
        if (string.IsNullOrWhiteSpace(text)) return;

        Debug.Log($"Sending text to whiteboard: {text}");

        // 2. Update Whiteboard (Networked or Local)
        UpdateWhiteboard(text);

        // 3. Clear Input Field
        inputField.text = "";

        // 4. Refocus Input Field (Optional - keeps keyboard open)
        inputField.ActivateInputField();
    }

    public void UpdateWhiteboard(string newText)
    {
        if (PhotonNetwork.InRoom)
        {
            photonView.RPC("RPC_UpdateText", RpcTarget.AllBuffered, newText);
        }
        else
        {
            ApplyTextLocally(newText);
        }
    }

    [PunRPC]
    void RPC_UpdateText(string text)
    {
        ApplyTextLocally(text);
    }

    void ApplyTextLocally(string text)
    {
        if (whiteboardDisplay != null)
        {
            whiteboardDisplay.text = text;
        }
    }

    // Check for physical Enter key press every frame (Backup method)
    void Update()
    {
        if (inputField.isFocused && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            OnInputSubmit(inputField.text);
        }
    }
}