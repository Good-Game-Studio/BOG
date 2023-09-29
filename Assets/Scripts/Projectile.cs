using Photon.Pun;
using System.Collections;
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
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine || collision.CompareTag("Player") && collision.GetComponent<Player>().playerDetails.id == playerIdOfCreator)
        {
            return;
        }
        if (collision.CompareTag("MeleeWeapon"))
        {
            rb.velocity = -rb.velocity;
            return;
        }

        //StreamDestroy(); 
        StopCoroutine(Start());
        PhotonNetwork.Destroy(gameObject);
    }
    void StreamDestroy() 
    {
        photonView.RPC(nameof(DestroyRPC), RpcTarget.AllBufferedViaServer); 
    }
    [PunRPC] public void DestroyRPC() { }
}
