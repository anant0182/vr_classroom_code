using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;


public class MenuControl : MonoBehaviour
{

    public InputField userl;
    public InputField passl;
    public InputField email;
    public InputField url;
    public InputField roomName;

    //public string room;

    public string usernam;
    public Text resultt;
    public bool IsAuthenticated = false;
    public bool IsTeacher = false;

    public GameObject[] LogComp;
    public GameObject[] SucComp;

    public ClassroomNetworkManager nw;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //string Encrypt(string pass)
    //{
    //    System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
    //    byte[] bs = System.TextEncoding.UTF8.GetBytes(pass);
    //    bs = x.ComputeHash(bs);

    //    System.Text.StringBuilder s = new System.Text.StringBuilder();
    //    foreach(byte b in bs)
    //    {
    //        s.Append(b.ToString("x2").ToLower());
    //    }

    //    return s.ToString();
    //}

    public void onClickLogin()
    {
        LoginWithPlayFabRequest loginRequest = new LoginWithPlayFabRequest();
        loginRequest.Username = userl.text;
        loginRequest.Password = passl.text;
        loginRequest.Username = "nand";
        loginRequest.Password = "ketang";
        Debug.Log(loginRequest.Username);
        Debug.Log(loginRequest.Password);
        PlayFabClientAPI.LoginWithPlayFab(loginRequest, result =>
        {
            usernam = loginRequest.Username;
            resultt.text = "Logged in as " + usernam;
            Debug.Log("Login Successful");
            IsAuthenticated = true;
            //NetworkManager nw = new NetworkManager();
            //nw.ConnectToServer();
            OnLogin();
        }, error =>
        {
            IsAuthenticated = false;
            resultt.text = error.ErrorMessage;
            Debug.Log("Login Unsuccessful");
            Debug.Log(error.ErrorMessage);
        }, null);
    }

    public void OnLogin()
    {
        nw.ConnectToServer();
        foreach (GameObject obj in LogComp)
        {
            obj.SetActive(false);
        }
    }

    public void onClickRegister()
    {
        var regRequest = new RegisterPlayFabUserRequest();
        regRequest.Username = userl.text;
        regRequest.Password = passl.text;
        regRequest.Email = email.text;
        //regRequest.Username = "nand";
        //regRequest.Password = "ketang";
        //regRequest.Email = "nandg8@gmail.com";
        Debug.Log(regRequest.Username);
        Debug.Log(regRequest.Password);
        Debug.Log(regRequest.Email);
        PlayFabClientAPI.RegisterPlayFabUser(regRequest, RegSuccess, RegLoss);
    }
    //    error =>
    //    {
    //        IsAuthenticated = false;
    //        resultt.text = error.ErrorMessage;
    //        Debug.Log("Register Unsuccessful");
    //        Debug.Log(error.ErrorMessage);
    //    });
    //}

    void RegSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Successfully Registered");
        IsAuthenticated = true;
        GetUrl();
    }

    void RegLoss(PlayFabError error)
    {
        Debug.Log("Successfully Unregistered");
        IsAuthenticated = false;
        /*GetUrl();*/
    }

    public void GetUrl()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                //{"Avatar_URL", url.text}
                {"Avatar_URL", "https://d1a370nemizbjq.cloudfront.net/dab51027-af9e-45b4-9db4-6a5a4612dda4.glb"}
            }
        };

        PlayFabClientAPI.UpdateUserData(request, result =>
        {
            Debug.Log("Successfully Got URL");
            IsAuthenticated = true;
        }, error =>
        {
            IsAuthenticated = false;
            resultt.text = error.ErrorMessage;
            Debug.Log("URL Unsuccessful");
            Debug.Log(error.ErrorMessage);
        }, null);
    }

    public void onClickLearn()
    {
        IsTeacher = false;
    }

    public void onClickTeach()
    {
        IsTeacher = true;
    }

    public void onClickJoin()
    {
        //room = roomName.text;
        nw.InitializeRoom();
    }

    public void onClickCreate()
    {
        //room = roomName.text;
        nw.InitializeRoom();
    }

    
}
