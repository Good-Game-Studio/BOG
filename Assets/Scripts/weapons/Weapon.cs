using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponManager manager;
    public SpriteRenderer spriteRenderer;
    public PhotonView photonView;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        photonView = GetComponent<PhotonView>();
    }
      public void Equip(string id,Vector2 playerDirection)
    {
        photonView.RPC(nameof(EquipRPC), RpcTarget.AllBufferedViaServer,id, playerDirection);
    }
    [PunRPC]
    public void EquipRPC(string id, Vector2 playerDirection)
    {
        Destroy(GetComponent<Rigidbody2D>());
        Destroy(GetComponent<BoxCollider2D>());
        Destroy(GetComponent<CircleCollider2D>());
        PlayerDetails player = RoomManager._instance.players.Find(p => p.id == id);
        if (playerDirection.x> 0)
        {
            transform.localScale = new Vector3(1, transform.lossyScale.y, transform.lossyScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-1, transform.lossyScale.y, transform.lossyScale.z);

        }
        transform.position = (Vector2)player?.player.transform.position +(playerDirection * manager.weaponOffset);
        transform.parent =player?.player.GetComponentInChildren<SpriteRenderer>().transform;
    }
    public void Destroy()
    {
        photonView.RPC(nameof(DestroyRPC), RpcTarget.AllBufferedViaServer);
    }
    [PunRPC]
    public void DestroyRPC()
    {
        WeaponGenerator._instance.allGenerated.Remove(gameObject);
        Destroy(gameObject);
    }
}
