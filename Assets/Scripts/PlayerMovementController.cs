using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovementController : MonoBehaviour
{
    public Player player;
    protected Rigidbody2D rb;

    public float moveSpeed;
    public float moveMaxSpeed;
    [Space]
    [Space]
    [Space]
    public float jumpPower;
    public float jumpMaxSpeed;
    public int totalJumps;
    public int currentJumps;
    int jumpStamina = 10;
    internal Vector2 direction;
    bool onWall;
    PhotonView photonView;
    [Space]
    [Space]
    [Space]
    public float wallMoveDownSpeed;
    public float wallMoveDownSpeedLimit;
    public bool allowMovement = true;

    [Space]
    [Header ("Attributes")]
    public float currentArmor;

    [Space]
    [Header("Stamina")]
    public Slider staminaBar;
    float currentStamina;
    bool staminaRegenAllowed = true;
    float staminaRegenTimerThresh = 1.5f;
    float staminaRegenTimer = 0f;
    float maxStamina;

    [Space]
    [Header("Dash")]
    float dashForce = 50f;
    bool allowDashing = true;
    float currentDashTimer;
    float dashCooldown = 0.3f;
    int dashStamina = 10;
    float dashTime;
    Vector2 lastVelocity = Vector2.zero;
    [SerializeField] PlayerEcho playerEcho;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();

        if (!photonView.IsMine)
        {
            return;
        }
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody2D>();

        //Set Attributes
        maxStamina = player.playerAttributes.stamina;
        dashTime = (100 / player.playerAttributes.dodgeDistance) / 30;
        dashCooldown = dashCooldown + 0.2f;
    }
    private void Start()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        currentJumps = totalJumps;


        staminaBar.maxValue = maxStamina;
        currentStamina = maxStamina;
        staminaBar.value = currentStamina;
    }
    private void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        Movement();

        if (staminaRegenAllowed && currentStamina < maxStamina)
        {
            currentStamina += 1;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            staminaBar.value = currentStamina;
        }
        else if (!staminaRegenAllowed)
        {
            staminaRegenTimer += Time.deltaTime;
            if (staminaRegenTimer > staminaRegenTimerThresh)
            {
                staminaRegenAllowed = true;
                staminaRegenTimer = 0f;
            }
        }
    }
    private void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        ClampSpeed();
    }
    internal void SetMoveDirection(Vector2 direction)
    {
        if (direction == this.direction )
        {
            return;
        }
        this.direction = direction;
    }

    private void Movement()
    {
        if (direction == Vector2.zero || !allowMovement)
        {
            player.animator.SetBool("Running", false);
            return;
        }
        transform.Translate(direction.normalized * moveSpeed * 0.02f * (100 / player.playerAttributes.speed));
        player.animator.SetBool("Running", true);
    }
    void ClampSpeed()
    {
        if (rb.velocity.y > jumpMaxSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, Vector2.ClampMagnitude(rb.velocity, jumpMaxSpeed).y);
        }
        if (rb.velocity.y < 0 && rb.gravityScale == 0)
        {
            if (rb.velocity.y < -wallMoveDownSpeedLimit)
            {
                rb.velocity = new Vector2(rb.velocity.x, Vector2.ClampMagnitude(rb.velocity, wallMoveDownSpeedLimit).y);
            }
        }

    }
    public void Jump()
    {
        if (currentJumps <= 0 || !allowMovement || !staminaConsumed(jumpStamina))
        {
            return;
        }
        rb.AddForce(Vector2.up * jumpPower * 0.2f * (100 / player.playerAttributes.jumpHeight), ForceMode2D.Impulse);
        currentJumps -= 1;
        player.animator.SetBool("Jumping", true);
    }

    public void Dash()
    {
        if (!allowDashing || !staminaConsumed(dashStamina))
            return;

        allowDashing = false;
        allowMovement = false;
        currentDashTimer = dashTime + Time.time;  //set dash timer for count down
        lastVelocity = rb.velocity;
        rb.velocity = Vector2.zero;

        StartCoroutine(DashCroutine());
    }

    IEnumerator DashCroutine()
    {
        player.health.isInvulnerable = true;
        rb.gravityScale = 0;
        playerEcho.StartAfterImages();
        while (currentDashTimer > Time.time)
        {
            rb.velocity = Vector2.right * player.playerDirection.x * dashForce;

            yield return new WaitForSeconds(0.01f);
        }

        StartCoroutine(DashCooldownCoroutine());
    }

    IEnumerator DashCooldownCoroutine()
    {
        playerEcho.DisableAfterImages();
        allowMovement = true;
        player.health.isInvulnerable = false;
        rb.gravityScale = 1;
        rb.velocity = lastVelocity;
        float dashCDTimer = dashCooldown + Time.time; //set dash cooldown timer for count down
        while (dashCDTimer > Time.time)
        {
            yield return new WaitForSeconds(0.1f);
        }
        allowDashing = true;   
    }

    public bool staminaConsumed(int staminaCost)
    {
        if (staminaCost <= currentStamina)
        {
            currentStamina -= staminaCost;
            staminaRegenAllowed = false;
            staminaRegenTimer = 0;
            staminaBar.value = currentStamina;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        if (collision.collider.CompareTag("Land"))
        {

            currentJumps = totalJumps;
            player.animator.SetBool("Jumping", false);

        }
        if (collision.collider.CompareTag("Wall"))
        {
            onWall = true;
            currentJumps = totalJumps;
            rb.gravityScale = 0;
            player.animator.SetBool("Jumping", false);

        }
        if (collision.collider.CompareTag("Player"))
        {
            //Debug.Log("player");
            player.animator.SetBool("Jumping", false);
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        if (collision.collider.CompareTag("Wall"))
        {
            onWall = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        rb.gravityScale = 1;
        onWall = false;
    }
  

}
