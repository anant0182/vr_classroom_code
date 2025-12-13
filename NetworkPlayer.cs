using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using Unity.XR.CoreUtils; // XROrigin lives here

public class NetworkPlayer : MonoBehaviour
{
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    private PhotonView photonView;

    public Transform headRig;
    public Transform leftHandRig;
    public Transform rightHandRig;
    public float smoothFactor = 0.5f;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        XROrigin origin = FindObjectOfType<XROrigin>();

        gameObject.name = photonView.InstantiationData[0].ToString() + "Base";

        // Calculate the offset between the headset and the avatar's head
        Vector3 headOffset = headRig.position - origin.transform.Find("Camera Offset/Main Camera").position;

        // Set the initial position of the avatar's head to match the headset's position
        transform.position = transform.position - headOffset;

        // Assign rig components
        headRig = origin.transform.Find("Camera Offset/Main Camera");
        leftHandRig = origin.transform.Find("Camera Offset/LeftHand Controller");
        rightHandRig = origin.transform.Find("Camera Offset/RightHand Controller");
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            XROrigin origin = FindObjectOfType<XROrigin>();

            // Calculate the offset between the headset and the avatar's head
            Vector3 headOffset = headRig.position - origin.transform.Find("Camera Offset/Main Camera").position;
            transform.position = transform.position - headOffset;

            // Smoothly move the avatar
            transform.position = Vector3.Lerp(transform.position, headRig.position, Time.deltaTime * smoothFactor);

            // Map XR nodes to avatar transforms
            MapPosition(head, XRNode.Head);
            MapPosition(leftHand, XRNode.LeftHand);
            MapPosition(rightHand, XRNode.RightHand);
        }
    }

    void MapPosition(Transform target, XRNode node)
    {
        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(node, inputDevices);

        if (inputDevices.Count > 0)
        {
            inputDevices[0].TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
            inputDevices[0].TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);

            target.position = position;
            target.rotation = rotation;
        }
    }
}
