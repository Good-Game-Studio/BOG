using Photon.Pun;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

public class Player : MonoBehaviour
{
    PhotonView photonView;
    PlayerMovementController playerMovementController;
    internal PlayerInputController playerInputController;
    public float weaponOffset;
    public Vector2 playerDirection = Vector2.right;
    public Health health;
    public Slider healthSlider;
    [SerializeField] float fireRate = 0.5f;
    float nextFire = 0.0f;
    public List<Weapon> weapons = new();
    public Weapon currentWeapon;
    public PlayerDetails playerDetails;
    public TextMeshProUGUI lifeText, gamerTagText;
    internal bool isActionPressed;
    public GameObject body;

    public PlayerAttributes playerAttributes;
    public Animator animator;
    public GameObject rightHand;
    public MeleeWeapon meleeWeapon;

    int meleeStamina = 10;

    bool isMeleeAttacking = false;

    private void Awake()
    {
        playerInputController = GetComponent<PlayerInputController>();
        photonView = GetComponent<PhotonView>();
        playerDetails = new PlayerDetails((string)photonView.InstantiationData[0], (string)photonView.InstantiationData[1], gameObject);
        lifeText.text = playerDetails.life.ToString();
        gamerTagText.text = playerDetails.gamerTag;
        RoomManager._instance.players.Add(playerDetails);
        RoomManager._instance.targetGroup.AddMember(transform,1,3);
        playerMovementController = GetComponent<PlayerMovementController>();
    }
 
    private void OnEnable()
    {
        health.OnGetAttack += GetAttack;
    }
    private void OnDisable()
    {
        playerMovementController = null;
        health.OnGetAttack -= GetAttack;
    }
    public void Die()
    {
        playerDetails.life -= 1;
        lifeText.text = playerDetails.life.ToString();
        if (photonView.IsMine)
        {
            transform.position = new Vector2();
        }
        if (playerDetails.life <= 0)
        {
            gameObject.SetActive(false);
        }
        health.Refill();
    }

    private void GetAttack()
    {
        healthSlider.value = health.currentHealth / health.totalHealth;
    }

    public void Fire()
    {
        if (currentWeapon != null && !isMeleeAttacking)
        {
            Vector2 bulletDirection = playerDirection;
            bulletDirection.y += UnityEngine.Random.Range(-5 / playerAttributes.accuracy, 5 / playerAttributes.accuracy);
            currentWeapon.Fire(bulletDirection);
        }
    }

   
    public void Jump(InputAction.CallbackContext obj)
    {
        playerMovementController.Jump();
    }

    public void Dash(InputAction.CallbackContext obj)
    {
        playerMovementController.Dash();
    }

    public void MeleeAttack(InputAction.CallbackContext obj)
    {
        if (isMeleeAttacking || !playerMovementController.staminaConsumed(meleeStamina))
            return;

        isMeleeAttacking = true;
        if(currentWeapon != null)
        {
            currentWeapon.GetComponent<SpriteRenderer>().DOColor(Color.clear, 0f);
        }
        meleeWeapon.gameObject.SetActive(true);
        meleeWeapon.GetComponent<SpriteRenderer>().DOColor(Color.white, 0.05f);
        meleeWeapon.GetComponent<SpriteRenderer>().DOColor(Color.clear, 0.05f).SetDelay(0.5f).OnComplete( delegate 
        {
            meleeWeapon.gameObject.SetActive(false);
            if (currentWeapon != null)
            {
                currentWeapon.GetComponent<SpriteRenderer>().DOColor(Color.white, 0f);
            }

            isMeleeAttacking = false;
        });
    }

    public void Move(InputAction.CallbackContext obj)
    {
        try
        {
            playerMovementController.SetMoveDirection(obj.ReadValue<Vector2>());
        }
        catch (System.Exception)
        {
            //throw;
        }
        if (obj.ReadValue<Vector2>() != Vector2.zero && obj.ReadValue<Vector2>() != playerDirection)
        {
            playerDirection = obj.ReadValue<Vector2>().normalized;
            playerDirection.y = 0;
            SetBodyAndWeaponTransform();
        }
    }
    public void SetBodyAndWeaponTransform()
    {
        if (playerDirection.x > 0)
        {
            body.transform.localScale = new Vector3(1, transform.lossyScale.y, transform.lossyScale.z);
        }
        else if (playerDirection.x < 0)
        {
            body.transform.localScale = new Vector3(-1, transform.lossyScale.y, transform.lossyScale.z);
        }

    }
    public void UpdateWeaponInfo(string nameOfWeapon,int bulletInMag,int totalBullets)
    {
        RoomManager._instance.weaponInfoText.text = $"{nameOfWeapon} : {bulletInMag} / {totalBullets}";
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (!photonView.IsMine)
        {
            return;
        }
        if (collision.collider.CompareTag("Bullet"))
        {
            RoomManager._instance.PlayerHit(playerDetails.id, collision.collider.GetComponent<Projectile>().damage);
            GetComponent<Rigidbody2D>().AddForce(collision.otherRigidbody.velocity.normalized * 100f * (health.totalHealth / health.currentHealth));
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine)
            return;
        if (collision.CompareTag("Finish"))
        {
            RoomManager._instance.PlayerDie(playerDetails.id);
        }

        if (collision.CompareTag("MeleeWeapon"))
        {
            RoomManager._instance.PlayerHit(playerDetails.id, collision.GetComponent<MeleeWeapon>().damage);
            GetComponent<Rigidbody2D>().AddForce((collision.transform.position - transform.position) * 100f * (health.totalHealth / health.currentHealth));
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        if (collision.CompareTag("Weapon") && isActionPressed&& currentWeapon == null)
        {
            currentWeapon?.gameObject.SetActive(false);
            currentWeapon = collision.gameObject.GetComponent<Weapon>();
            currentWeapon.Equip(playerDetails.id,playerDirection);
            weapons.Add(currentWeapon);
            //SetBodyAndWeaponTransform();
            currentWeapon.transform.rotation = Quaternion.identity;
            isActionPressed = false;
            animator.SetLayerWeight(1, 0f);
            animator.SetLayerWeight(2, 1f);
            rightHand.SetActive(true);
        }
    }
}
[System.Serializable]
public class Health
{
    public bool isInvulnerable;
    public float totalHealth;
    public float currentHealth;
    public DelegateFunc OnGetAttack;
    public Health(int health)
    {
        this.totalHealth = health;
        currentHealth = totalHealth;
    }
    public void Refill()
    {
        currentHealth = totalHealth;
    }
    public void GetDamage(float damageAmount)
    {
        if (isInvulnerable)
            return;
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
        }
        OnGetAttack();
    }
}
public delegate void DelegateFunc();
