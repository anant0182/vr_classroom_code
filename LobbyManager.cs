using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public Button connectButton;
    public TMPro.TextMeshProUGUI statusText;

    public string landingRoomName = "LandingRoom";
    public string landingSceneName = "Landing";

    void Start()
    {
        connectButton.onClick.AddListener(OnConnectClicked);
        statusText.text = "Ready to connect.";
    }

    private void OnConnectClicked()
    {
        if (!PhotonNetwork.IsConnected)
        {
            statusText.text = "Connecting to Photon...";
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            // Already connected, safe to join
            JoinLandingRoom();
        }
    }

    // ✅ This is the correct place to join/create a room
    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected to Master. Joining landing room...";
        JoinLandingRoom();
    }

    private void JoinLandingRoom()
    {
        RoomOptions options = new RoomOptions { MaxPlayers = 20 };
        PhotonNetwork.JoinOrCreateRoom(landingRoomName, options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        statusText.text = "Joined Landing Room. Loading scene...";
        PhotonNetwork.LoadLevel(landingSceneName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = "Failed to join room: " + message;
    }
}
