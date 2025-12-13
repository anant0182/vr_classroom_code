using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;
public class WhiteboardPen : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    public Whiteboard whiteboard;
    private RaycastHit touch;
    private bool lastTouch;
    private Quaternion lastAngle;
    //private VRTK_ControllerReference controller;
    public string colourpen;
    public InputHelpers.Button drawButton;
    // Start is called before the first frame update
    void Start()
    {
        //this.whiteboard = GameObject.Find("Whiteboard").GetComponent<Whiteboard>();
    }

    // Update is called once per frame
    void Update()
    {
        float tipHeight = transform.Find("Tip").transform.localScale.y;
        Vector3 tip = transform.Find("Tip").transform.position;
        if (lastTouch)
        {
            tipHeight *= 1.1f;
        }
        if (Physics.Raycast(tip, transform.up, out touch, tipHeight))
        {
            Debug.Log("here");
            Debug.Log(touch.collider.tag);
            if (!(touch.collider.tag == "Whiteboard"))
                return;
            this.whiteboard = GameObject.Find("VERDE_GRANDE").GetComponent<Whiteboard>();

            //VRTK_ControllerHaptics.TriggerHapticPulse(controller, 0.1f);
            pv.RPC("SetColor", RpcTarget.AllBuffered, colourpen);
            Debug.Log(touch.textureCoord.x);
            Debug.Log(touch.textureCoord.y);

            pv.RPC("SetTouchPosition", RpcTarget.AllBuffered, touch.textureCoord.x, touch.textureCoord.y);
            pv.RPC("ToggleTouch", RpcTarget.AllBuffered, true);

            Debug.Log("touching!");
            if (!lastTouch)
            {
                lastTouch = true;
                lastAngle = transform.rotation;
            }
        }
        else
        {
            lastTouch = false;
        }
        if (lastTouch)
        {
            transform.rotation = lastAngle;
        }
    }
    

}
