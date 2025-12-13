using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Wolf3D.ReadyPlayerMe.AvatarSDK;
using UnityEngine.Animations.Rigging;
using PlayFab;
using PlayFab.ClientModels;


public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;
    public RuntimeAnimatorController con;
    string url;
    string localUserName;
    //string MyPlayfabID;
    AvatarLoader avatarLoader;
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined a room");
        //GameObject vrc = PhotonNetwork.Instantiate("VR Constraints", transform.position, transform.rotation);
        //GameObject avatarScripts = PhotonNetwork.Instantiate("Avatar Scripts", transform.position, transform.rotation);
        //spawnedPlayerPrefab = PhotonNetwork.Instantiate("New Avatar", transform.position, transform.rotation);
        //spawnedPlayerPrefab.transform.SetParent(avatarScripts.transform);
        //vrc.transform.SetParent(spawnedPlayerPrefab.transform);
        GetAccountInfoRequest request = new GetAccountInfoRequest();
        PlayFabClientAPI.GetAccountInfo(request, Successs, fail);
        //photonView.RPC("LoadAvatar", RpcTarget.AllBufferedViaServer, MyPlayfabID);
        //LoadAvatar();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        PhotonNetwork.LoadLevel(0);
        base.OnJoinRoomFailed(returnCode, message);
    }

    void Successs(GetAccountInfoResult result)
    {

        string MyPlayfabID = result.AccountInfo.PlayFabId;
        localUserName = result.AccountInfo.Username;
        PhotonView photonView = PhotonView.Get(this);
        //LoadOwnAvatar();
        object[] x = { localUserName };
        GameObject avatarBase = PhotonNetwork.Instantiate("Avatar Base", transform.position, transform.rotation, 0, x);
        photonView.RPC("LoadAvatar", RpcTarget.AllBuffered, MyPlayfabID, localUserName);
    }

    void fail(PlayFabError error)
    {

        Debug.LogError(error.GenerateErrorReport());
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.Destroy(spawnedPlayerPrefab);
    }
    [PunRPC]
    private void LoadAvatar(string playfabID, string username)
    {
        avatarLoader = new AvatarLoader();
        //getUrl(MyPlayfabID);
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = playfabID,
        }, (GetUserDataResult result) => onDataReceived(result, username), onError);
    }

    
    private void AvatarImportedCallback(GameObject avatar)
    {
        // called after GLB file is downloaded and imported
        Debug.Log("Avatar Imported!");
    }

    //void getUrl(string MyPlayfabID)
    //{
    //    PlayFabClientAPI.GetUserData(new GetUserDataRequest()
    //    {
    //        PlayFabId = MyPlayfabID,
    //    }, onDataReceived, onError);
    //}

    void onDataReceived(GetUserDataResult result, string username)
    {
        Debug.Log("Received Data");
        if (result.Data != null && result.Data.ContainsKey("Avatar_URL"))
        {
            url = result.Data["Avatar_URL"].Value;
            avatarLoader.LoadAvatar(url, AvatarImportedCallback, (GameObject avatar, AvatarMetaData metaData) => AvatarLoadedCallback(avatar, metaData, username));
        }
        else
        {
            Debug.Log("Player data not complete");
            url = "https://d1a370nemizbjq.cloudfront.net/dab51027-af9e-45b4-9db4-6a5a4612dda4.glb";
            avatarLoader.LoadAvatar(url, AvatarImportedCallback, (GameObject avatar, AvatarMetaData metaData) => AvatarLoadedCallback(avatar, metaData, username));
        }
    }

    void onError(PlayFabError error)
    {
        Debug.Log("The error is" + error);
        url = "https://d1a370nemizbjq.cloudfront.net/dab51027-af9e-45b4-9db4-6a5a4612dda4.glb";
        avatarLoader.LoadAvatar(url, AvatarImportedCallback, (GameObject avatar, AvatarMetaData metaData) => AvatarLoadedCallback(avatar, metaData, "abc"));
    }

    private void AvatarLoadedCallback(GameObject avatar, AvatarMetaData metaData, string username)
    {
        // called after avatar is prepared with components and anim controller 
        Debug.Log("Avatar Loaded!");

        /*
        avatar.name = username;
        spawnedPlayerPrefab = avatar;
        object[] x = { username };
        GameObject avatarBase = GameObject.Find(username + "Base");
        BoxCollider bc = avatarBase.AddComponent<BoxCollider>();
        bc.center = new Vector3(0, 0.9f, 0);
        bc.size = new Vector3(1, 1.8f,1);
        bc.material = Resources.Load<PhysicMaterial>("BaseMat");
        //avatarBase.transform.rotation = new Quaternion(0, -90, 0, 0);

        Rigidbody rb = avatarBase.AddComponent<Rigidbody>();
        rb.useGravity = false;
        //avatarBase.name = localUserName + "Base";
        //PhotonView photonView = PhotonView.Get(avatarBase);
        //photonView.RPC("ChangeName", RpcTarget.All, avatarBase,localUserName);
        //GameObject avatarBase = PhotonNetwork.Instantiate("Avatar Base", transform.position, transform.rotation, 0, x);

        GameObject VRConstraints = avatarBase.transform.GetChild(0).gameObject;
        spawnedPlayerPrefab.transform.SetParent(avatarBase.transform);
        spawnedPlayerPrefab.transform.localPosition=new Vector3(0, 0, 0);
        //spawnedPlayerPrefab.transform.localRotation = new Quaternion(0, -90, 0, 0);

        VRConstraints.transform.SetParent(spawnedPlayerPrefab.transform);

        Animator animator = spawnedPlayerPrefab.GetComponent<Animator>();
        animator.runtimeAnimatorController = con;

        RigBuilder rigBuilder = spawnedPlayerPrefab.AddComponent<RigBuilder>();
        rigBuilder.layers.Add(new RigBuilder.RigLayer(VRConstraints.GetComponent<Rig>(), true));
        PhotonAnimatorView anim = spawnedPlayerPrefab.AddComponent<PhotonAnimatorView>();
        anim.SetParameterSynchronized("IsMoving",PhotonAnimatorView.ParameterType.Bool,PhotonAnimatorView.SynchronizeType.Continuous);
        spawnedPlayerPrefab.AddComponent<PhotonTransformView>();

        spawnedPlayerPrefab.AddComponent<VRFootIK>();

        //VRAnimatorController vrAnimatorController = spawnedPlayerPrefab.AddComponent<VRAnimatorController>();

        //GameObject vrc = PhotonNetwork.Instantiate("VR Constraints", transform.position, transform.rotation);
        //vrc.transform.SetParent(spawnedPlayerPrefab.transform);
        VRAnimatorController vrAnimatorController = avatarBase.GetComponent<VRAnimatorController>();
        //vrAnimatorController.vrRig = avatarBase.GetComponent<VRRig>();
        vrAnimatorController.base1 = avatarBase;
        vrAnimatorController.animator = spawnedPlayerPrefab.GetComponent<Animator>();

        GameObject rightArmIK = VRConstraints.transform.GetChild(0).gameObject;
        //Debug.Log(rightArmIK);
        //Debug.Log("found");
        TwoBoneIKConstraint rIKConstraint = rightArmIK.GetComponent<TwoBoneIKConstraint>();
        rIKConstraint.data.root = GameObject.Find(username + "/Armature/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm").transform;
        rIKConstraint.data.mid = GameObject.Find(username + "/Armature/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm").transform;
        rIKConstraint.data.tip = GameObject.Find(username + "/Armature/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand").transform;


        //TwoBoneIKConstraint constraint;
        //constraint = rightArmIK.GetComponent<TwoBoneIKConstraint>();
        //constraint.data.root= GameObject.Find("RightArm").transform;

        GameObject leftArmIK = VRConstraints.transform.GetChild(1).gameObject;
        TwoBoneIKConstraint lIKConstraint = leftArmIK.GetComponent<TwoBoneIKConstraint>();
        lIKConstraint.data.root = GameObject.Find(username + "/Armature/Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm").transform;
        lIKConstraint.data.mid = GameObject.Find(username + "/Armature/Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm").transform;
        lIKConstraint.data.tip = GameObject.Find(username + "/Armature/Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm/LeftHand").transform;

        GameObject head = VRConstraints.transform.GetChild(2).gameObject;
        MultiParentConstraint headC = head.GetComponent<MultiParentConstraint>();
        headC.data.constrainedObject = GameObject.Find(username + "/Armature/Hips/Spine/Spine1/Spine2/Neck/Head").transform;
        // GameObject headobj = GameObject.Find(username + "/Armature/Hips/Spine/Spine1/Spine2/Neck/Head");
        //headobj.setActive(false);
        rigBuilder.Build();
        */
    }


}


