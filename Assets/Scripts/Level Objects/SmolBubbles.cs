using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SmolBubbles : MonoBehaviour
{
    [Header("References")]
    public PlayerData Data;
    private BubbleAnimator anim;
    private PlayerMovement mov;
    private LevelManager level;
    
    [SerializeField] GameObject player;
    [SerializeField] GameObject lmg;
    [SerializeField] GameObject spawner;
    private Rigidbody2D bubbleRB;
    private Rigidbody2D playerRB;
    private Collider2D _collider;
    private SpriteRenderer outline;
    private Transform target;
    private Transform spawn;
    
    bool triggered;
    public float stoppingDistance = 0.8f;
    bool isBigBubble = false;
    public bool bigBubblePopped = false;
   

    void Start()
    {
        mov = player.GetComponent<PlayerMovement>();
        playerRB = player.GetComponent<Rigidbody2D>();
        bubbleRB = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CircleCollider2D>();
        spawn = spawner.GetComponent<Transform>();
        anim = GetComponent<BubbleAnimator>();
        outline = spawner.GetComponent<SpriteRenderer>();
        level = lmg.GetComponent<LevelManager>();
        target = player.GetComponent<Transform>();

        outline.enabled = false;
    }

    void Update()
    {
        if (triggered)
            Follow();
        
        if (isBigBubble && !mov.IsJumping && Time.timeScale != 0f && mov.LastPressedJumpTime > 0 && mov.isGrounded == false)
        {
            BubbleJump(this.gameObject);
        }
        if (level.respawnBubbles)
        {
            bigBubblePopped = false;
            level.respawnBubbles = false;
            StartCoroutine(Respawn(4f));
        }
    }   

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            triggered = true;
            _collider.enabled = false;
            level.numberOfBubbles += 1;
            outline.enabled = true;
        }
    }

    void Follow()
    {
        if (Vector2.Distance(transform.position, target.position) > stoppingDistance)
        {
            float targetSpeed = Vector2.Distance(transform.position, target.position) / 1.5f;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * 1.5f : Data.runDeccelAmount * 1.5f;
            float speedDif = targetSpeed - bubbleRB.velocity.x;
		    float movement = speedDif * accelRate;
            
            transform.position = Vector2.MoveTowards(transform.position, target.position, movement * Time.deltaTime);
        }
    }

    public void Merge()
    {
        anim.sBubbleMerge = true;
        stoppingDistance = 1f;
        isBigBubble = true;
    }

    void BubbleJump(GameObject bubble)
    {
        mov.IsJumping = true;
        mov.isGrounded = false;
            
        float bounceHeight = 22f;

        if (mov.IsRolling)
        {  
            mov.stopRoll = true;
            bounceHeight *= 1.2f;
        }
        if (playerRB.velocity.y < 0)
        {
            bounceHeight -= playerRB.velocity.y;
        }

        playerRB.velocity = new Vector2(playerRB.velocity.x, bounceHeight);

        anim.sBubblePopped = true;
        bigBubblePopped = true;
        isBigBubble = false;
    }

    public IEnumerator Respawn(float waitTime)
    {
        transform.position = spawn.position;
        triggered = false;
        stoppingDistance = 0.8f;
        mov.stopRoll = false;

        yield return new WaitForSeconds(waitTime);
        anim.sBubbleRespawn = true;
        outline.enabled = false;
        waitTime = 0.3f;    
        
        yield return new WaitForSeconds(waitTime);
        _collider.enabled = true;
    }
}