using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float moveSpeed;
    Vector2 direction = Vector2.zero;
    [SerializeField] Rigidbody2D rb;
    public float damage;
    PhotonView photonView;
    internal string playerIdOfCreator;
    public float lifeOfBullet;
    public float lifetime = 2f;
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        SetDirection((Vector2)photonView?.InstantiationData[0]);
        playerIdOfCreator = (string)photonView?.InstantiationData[1];
        damage = (float)photonView?.InstantiationData[3];

    }
    IEnumerator Start()
    {
        yield return new WaitForSeconds((float)photonView?.InstantiationData[2]);
        PhotonNetwork.Destroy(gameObject);
    }
    public void SetDirection(Vector2 dir)
    {
        rb.velocity = dir * moveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision) 
    {
        if (!photonView.IsMine || collision.collider.CompareTag("Player") && collision.collider.GetComponent<Player>().playerDetails.id == playerIdOfCreator)
        {
            return;
        }
        //StreamDestroy(); 
        PhotonNetwork.Destroy(gameObject);
    }
    void StreamDestroy() 
    {
        photonView.RPC(nameof(DestroyRPC), RpcTarget.AllBufferedViaServer); 
    }
    [PunRPC] public void DestroyRPC() { }
}
