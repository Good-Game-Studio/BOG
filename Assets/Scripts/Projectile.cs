using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float moveSpeed;
    Vector2 direction = Vector2.zero;
    [SerializeField] Rigidbody2D rb;
    public int damage;
    PhotonView photonView;
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        SetDirection((Vector2)photonView?.InstantiationData[0]);
        StartCoroutine(DelayDestroy());

    }
    public void SetDirection(Vector2 dir)
    {
        rb.velocity = dir * moveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision) 
    {
        if (!photonView.IsMine)
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
    IEnumerator DelayDestroy()
    {
        yield return new WaitForSeconds(2.5f);
        PhotonNetwork.Destroy(gameObject);

    }
}
