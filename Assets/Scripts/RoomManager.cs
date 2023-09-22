using Cinemachine;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager _instance;
    public TextMeshProUGUI pingText,connectionStatus;

    internal CinemachineTargetGroup targetGroup;
    public List<PlayerDetails> players = new();

    public bool DidTimeout { private set; get; }
    static readonly RoomOptions s_RoomOptions = new RoomOptions
    {
        MaxPlayers = 4,
        EmptyRoomTtl = 5,
        PublishUserId = true,
    };

    void Awake()
    {
        connectionStatus.text = "Status : "+ "";
        _instance = this;
        targetGroup = GetComponentInChildren<CinemachineTargetGroup>();
        Assert.AreEqual(1, FindObjectsOfType<RoomManager>().Length);

    }
    private void Start()
    {
        PhotonNetwork.Disconnect();
        StartCoroutine(DoJoinOrCreateRoom("stg1"));
    }
    #region All Room Settings
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        connectionStatus.text=("Created Room");
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        PhotonNetwork.Instantiate("Player Holder", new Vector3(), Quaternion.identity, 0, new object[] { PhotonNetwork.LocalPlayer.UserId, PlayerPrefs.GetString("gamerTag") });
        if (PhotonNetwork.IsMasterClient)
        {
            WeaponGenerator._instance.StartSpawning();
        }
        StartCoroutine(SynchroniseGame());
    }

    public void JoinOrCreateRoom(string preferredRoomName)
    {
        //Debug.LogError(preferredRoomName);
        StopAllCoroutines();
        const float timeoutSeconds = 15f;
        StartCoroutine(DoCheckTimeout(timeoutSeconds));
        StartCoroutine(DoJoinOrCreateRoom(preferredRoomName));
    }

    IEnumerator DoCheckTimeout(float timeout)
    {
        DidTimeout = false;
        while (!PhotonNetwork.InRoom && timeout >= 0)
        {
            yield return null;
            timeout -= Time.deltaTime;
        }

        if (timeout <= 0)
        {
            DidTimeout = true;
            StopAllCoroutines();
        }

    }



     IEnumerator DoJoinOrCreateRoom(string preferredRoomName)
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        if (!PhotonNetwork.IsConnected)
        {
            connectionStatus.text = "Status : "+ "Connecting to server";
            PhotonNetwork.ConnectUsingSettings();
        }

        while (!PhotonNetwork.IsConnectedAndReady)
        {

            yield return null;
        }

        if (!PhotonNetwork.InLobby && PhotonNetwork.NetworkClientState != ClientState.JoiningLobby)
            connectionStatus.text = "Status : "+ $"Connecting to server ,state ={PhotonNetwork.NetworkClientState}";
        {
            PhotonNetwork.JoinLobby();

        }

        while (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
        {

            yield return null;
        }


        if (preferredRoomName != null)
        {
            connectionStatus.text = "Status : "+ "Joining or creating Room";

            bool isJoined = PhotonNetwork.JoinOrCreateRoom(preferredRoomName, s_RoomOptions, TypedLobby.Default);
        }
        else
        {
            connectionStatus.text = "Status : "+ "Joined Random Room";

            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Room Created on joining fail");
        PhotonNetwork.CreateRoom(null, s_RoomOptions, TypedLobby.Default);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        connectionStatus.text = "Status : "+ "Room Created on joining fail";
        PhotonNetwork.CreateRoom(null, s_RoomOptions, TypedLobby.Default);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        var leavingPlayer = players.Find(x => x.id == otherPlayer.UserId);
        if (leavingPlayer != null)
        {
           targetGroup.RemoveMember(leavingPlayer.player.transform);
            players.Remove(leavingPlayer);
        }

    }
    #endregion
    #region Synchronization Code
    IEnumerator SynchroniseGame()
    {
        yield return new WaitForSeconds(1);
        connectionStatus.text =  "";
        while (PhotonNetwork.IsConnected)
        {
            yield return new WaitForSeconds(1);
            pingText.text ="Ping : " +PhotonNetwork.GetPing().ToString();
        }
    }
    public void PlayerHit(string id,float damage)
    {
        photonView.RPC(nameof(PlayerHitRpc), RpcTarget.AllBufferedViaServer,id,damage);
    }
    [PunRPC]
    public void PlayerHitRpc(string id, float damage)
    {
        players.Find(x => x.id == id).player.GetComponent<Player>().health.GetDamage(damage);
    }
    public void PlayerDie(string id)
    {
        photonView.RPC(nameof(PlayerFellRpc), RpcTarget.AllBufferedViaServer, id);
    }
    [PunRPC]
    public void PlayerFellRpc(string id)
    {
        players.Find(x => x.id == id).player.GetComponent<Player>().Die();
    }
    #endregion

}
[System.Serializable]
public class PlayerDetails
{
    public string id;
    public string gamerTag;
    public GameObject player;
    public int life = 3;

    public PlayerDetails(string id, string name, GameObject player)
    {
        this.id = id;
        this.gamerTag = name;
        this.player = player;
        life = 3;
    }
}
