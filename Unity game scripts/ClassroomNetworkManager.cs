using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

[System.Serializable]
public class DefaultRoom
{
    public string name;
    public int sceneIndex;
    public int maxPlayer;
}

public class ClassroomNetworkManager : MonoBehaviourPunCallbacks
{
    public List<DefaultRoom> defaultRooms;
    public MenuControl menuUI;

    public void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Trying to connect to server");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected To Server.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined the lobby");

        foreach (GameObject obj in menuUI.SucComp)
            obj.SetActive(true);
    }

    public void InitializeRoom()
    {
        DefaultRoom roomSettings = new DefaultRoom();
        roomSettings.name = menuUI.roomName.text;
        roomSettings.sceneIndex = 1;
        roomSettings.maxPlayer = 10;

        if (menuUI.IsTeacher)
        {
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = (byte)roomSettings.maxPlayer,
                IsVisible = true,
                IsOpen = true
            };

            PhotonNetwork.CreateRoom(roomSettings.name, roomOptions, TypedLobby.Default);
        }
        else
        {
            PhotonNetwork.JoinRoom(roomSettings.name);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room");

        if (menuUI.IsTeacher)
        {
            // Teacher goes straight to LectureRoom
            PhotonNetwork.LoadLevel("LectureRoom");
        }
        else
        {
            // Student goes to WaitingRoom first
            PhotonNetwork.LoadLevel("WaitingRoom");
        }
    }

    // Called when scene changes are done
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (menuUI.IsTeacher && scene.name == "LectureRoom")
        {
            // Teacher marks room as ready
            Hashtable props = new Hashtable();
            props["teacherReady"] = true;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            Debug.Log("Teacher marked classroom as ready.");
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        // Students in WaitingRoom listen for teacherReady
        if (!menuUI.IsTeacher && SceneManager.GetActiveScene().name == "WaitingRoom")
        {
            if (propertiesThatChanged.ContainsKey("teacherReady") &&
                (bool)propertiesThatChanged["teacherReady"] == true)
            {
                Debug.Log("Teacher is ready! Moving to lecture room...");
                PhotonNetwork.LoadLevel("LectureRoom");
            }
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        menuUI.resultt.text = "Can't join room!";
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        menuUI.resultt.text = "Can't create room!";
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
}
