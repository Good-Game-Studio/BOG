using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponManager manager;
    string weaponName;
    public SpriteRenderer spriteRenderer;
    public PhotonView photonView;
    public Player player;
    public int bulletInMag,totalBullets;
    bool isReloading;
    bool isFiring;
    float lastFireTime;
    Vector2 bulletDirection;
    public Animator animatior;
    private void Awake()
    {
        animatior = GetComponent<Animator>();
        animatior.enabled = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        photonView = GetComponent<PhotonView>();
        weaponName = manager.name.Split(' ')[0]; 
        totalBullets = manager.totalBullet;
        bulletInMag = manager.bulletPerMag;
    }
    internal void Fire(Vector2 bulletDirection)
    {
        if (bulletInMag <= 0 || isReloading || isFiring)
        {
            return;
        }
        this.bulletDirection = bulletDirection;
        StartCoroutine(Firing());
    }
    IEnumerator ReloadDelay()
    {
        if (totalBullets >0)
        {
            isReloading = true;
            float time = 0;
            while (time< manager.reloadingTime)
            {
                player.reloadingImage.fillAmount = 1-time/manager.reloadingTime;
                time += Time.deltaTime;
                yield return null;
            }
            player.reloadingImage.fillAmount = 0;

            Reload();
            isReloading = false;
        }

    }
    void Reload()
    {
        if (totalBullets >= manager.bulletPerMag)
        {
            totalBullets -= manager.bulletPerMag;
            bulletInMag = manager.bulletPerMag;
        }
        else
        {
            totalBullets = 0;
            bulletInMag = totalBullets;
        }
       player?.UpdateWeaponInfo(weaponName,bulletInMag,totalBullets);
    }
    void SpawnBullet()
    {
        Vector2 spawnPosition = new Vector2(transform.position.x + (player.playerDirection.x * 1.1f), transform.position.y + (player.playerDirection.y * 1.1f));
        PhotonNetwork.Instantiate("BulletMedium", spawnPosition, Quaternion.LookRotation(player.playerDirection,Vector2.up), 0, new object[] { bulletDirection,player.playerDetails.id });
        bulletInMag--;
        lastFireTime = Time.time;
        player.UpdateWeaponInfo(weaponName, bulletInMag, totalBullets);
    }

    IEnumerator Firing()
    {
        isFiring = true;
        Sprite sprite = spriteRenderer.sprite;
        animatior.enabled = true;
        if (manager.isAutomatitc)
        {
            while (player.playerInputController.playerActions.Attack1.IsInProgress() && bulletInMag > 0)
            {
                SpawnBullet();
                yield return new WaitForSeconds(manager.fireRate);
            }
        }
        else
        {
            if (Time.time > lastFireTime + manager.fireRate || bulletInMag == manager.bulletPerMag)
            {
                SpawnBullet();
            }
            float initTime = animatior.GetCurrentAnimatorStateInfo(0).normalizedTime;
            while (animatior.GetCurrentAnimatorStateInfo(0).normalizedTime <=initTime+ 1f)
            {
                yield return null;
            }
        }
        animatior.enabled = false;
        spriteRenderer.sprite = sprite;
        isFiring = false;
        if (bulletInMag<=0)
        {
            StartCoroutine(ReloadDelay());
        }
    }

    public void Equip(string id,Vector2 playerDirection)
    {
        photonView.RPC(nameof(EquipRPC), RpcTarget.AllBufferedViaServer,id);
    }
    [PunRPC]
    public void EquipRPC(string id)
    {
        Destroy(GetComponent<Rigidbody2D>());
        Destroy(GetComponent<BoxCollider2D>());
        Destroy(GetComponent<CircleCollider2D>());
        PlayerDetails playerDetail = RoomManager._instance.players.Find(p => p.id == id);
        player = playerDetail.player.GetComponent<Player>();
        if (player == null)
        {
            return;
        }
        SetWeaponParrentAndDirection();
        player.UpdateWeaponInfo(weaponName, bulletInMag, totalBullets);
    }
    void SetWeaponParrentAndDirection()
    {
        var bodyScale = player.body.transform.localScale;
        player.body.transform.localScale = new Vector3(1, 1, 1);
        var hand = player.rightHand;
        transform.position = (Vector2)hand.transform.position + (Vector2.right * manager.weaponOffset);
        transform.SetParent(hand.transform);
        player.body.transform.localScale = bodyScale;
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
