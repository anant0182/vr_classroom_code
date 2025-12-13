using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine.Video;

public class presentation : MonoBehaviour
{
    //public GameObject[] slides; // an array of game objects representing each slide
    private int currentSlide = 0; // the index of the current slide
    public PhotonView pv;
    public InputDevice _rightController;
    public InputDevice _leftController;
    public InputDevice _HMD;

    public GameObject[] ppt;
    public VideoPlayer[] videoPlayer;
    public int i = 0;
    private int is_playing = 0;
    private bool p_AButton=false;
    private bool p_BButton=false;
    private bool p_XButton=false;
    public RawImage apiresultImage;
    public int mode = 0; //there can be three modes: 0 for presentation, 1 for video, 2 for API
    void Start()
    {
        pv.RPC("HideSlide", RpcTarget.AllBuffered, currentSlide);
        // set the apiresultImage to be active and mode = 2
        apiresultImage.gameObject.SetActive(true);
        mode = 2;
        // ShowSlide(currentSlide); // show the first slide when the presentation starts
    }

    void Update()
    {
        if (!_rightController.isValid || !_leftController.isValid || !_HMD.isValid)
            InitializeInputDevices();
        if (_leftController.TryGetFeatureValue(CommonUsages.primaryButton, out bool Xbutton))
        {
            // we will use this to change modes
            Debug.Log("X button: " + Xbutton);
            if (Xbutton != p_XButton && Xbutton)
            {
                mode = (mode + 1) % 3;
                Debug.Log("Mode: " + mode);
            }
            p_XButton = Xbutton;
            
        }

        if (_rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool Abutton))
        {
            //_leftMaxScore = theFloat;
            //rightScoreDisplay.text = Abutton.ToString();
            Debug.Log("A button: " + Abutton);
            if (Abutton!=p_AButton && Abutton)
            {
                pv.RPC("NextSlide", RpcTarget.AllBuffered);

            }
            p_AButton = Abutton;
        }

        if (_rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool Bbutton))
        {
            Debug.Log("B button: " + Bbutton);
            if (Bbutton != p_BButton && Bbutton)
            {
                pv.RPC("PreviousSlide", RpcTarget.AllBuffered);
            }
            p_BButton = Bbutton;
        }
        /*if (Input.GetKeyDown(KeyCode.RightArrow)) // advance to the next slide when the right arrow key is pressed
         {
             NextSlide();
         }
         else if (Input.GetKeyDown(KeyCode.LeftArrow)) // go back to the previous slide when the left arrow key is pressed
         {
             PreviousSlide();
         }*/

        
    }
    private void InitializeInputDevices()
    {

        if (!_rightController.isValid)
            InitializeInputDevice(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right, ref _rightController);
        if (!_leftController.isValid)
            InitializeInputDevice(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left, ref _leftController);
        if (!_HMD.isValid)
            InitializeInputDevice(InputDeviceCharacteristics.HeadMounted, ref _HMD);

    }

    private void InitializeInputDevice(InputDeviceCharacteristics inputCharacteristics, ref InputDevice inputDevice)
    {
        List<InputDevice> devices = new List<InputDevice>();
        //Call InputDevices to see if it can find any devices with the characteristics we're looking for
        InputDevices.GetDevicesWithCharacteristics(inputCharacteristics, devices);

        //Our hands might not be active and so they will not be generated from the search.
        //We check if any devices are found here to avoid errors.
        if (devices.Count > 0)
        {
            inputDevice = devices[0];
        }
    }
    [PunRPC]
    public void HideSlide()
    {
        for (int i = 0; i < ppt.Length; i++)
        {
            ppt[i].gameObject.SetActive(false);
            videoPlayer[i] = ppt[i].GetComponent<VideoPlayer>();
        }
    }
    [PunRPC]
    public void StartSlide()
    {
        if (!ppt[i].gameObject.activeSelf)
            ppt[i].gameObject.SetActive(true);
        else
        {
            ppt[i].gameObject.SetActive(false);
        }
    }
    [PunRPC]
    public void NextSlide()
    {
        if(mode==0){
            apiresultImage.gameObject.SetActive(false);

            ppt[i].gameObject.SetActive(false);
            i++;
            if (i == ppt.Length)
            {
                i = 0;
            }
            for (int i = 0; i < videoPlayer.Length; i++)
            {
                videoPlayer[i].gameObject.SetActive(false);
            }
            ppt[i].gameObject.SetActive(true);
        }
        if (videoPlayer[i] != null && mode == 1)
        {
            apiresultImage.gameObject.SetActive(false);
            Debug.Log("Trying to play video");
            ppt[i].gameObject.SetActive(false);
            videoPlayer[i].gameObject.SetActive(true);
            videoPlayer[i].time = 0;
            videoPlayer[i].Play();
        }
        if(mode==2){
            apiresultImage.gameObject.SetActive(true);
        }
    }
    [PunRPC]
    public void PreviousSlide()
    {
        if(mode==0){
            apiresultImage.gameObject.SetActive(false);
            ppt[i].gameObject.SetActive(false);
            i--;
            if (i == -1)
            {
                i = ppt.Length - 1;
            }
            ppt[i].gameObject.SetActive(true);
        }
        if (videoPlayer[i] != null && mode == 1)
        {
            apiresultImage.gameObject.SetActive(false);
            Debug.Log("Trying to pause video");
            videoPlayer[i].Pause();
        }
        if(mode==2){
            apiresultImage.gameObject.SetActive(true);
        }
        
    }


}
