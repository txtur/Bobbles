using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubbles : MonoBehaviour
{
    public PlayerData Data;
    public GameObject player;
    PlayerMovement mov;
    Rigidbody2D RB;
    Collider2D _collider;
    private BubbleAnimator anim;

    void Start()
    {
        mov = player.GetComponent<PlayerMovement>();
        RB = player.GetComponent<Rigidbody2D>();
        _collider = GetComponent<CircleCollider2D>();
        anim = GetComponent<BubbleAnimator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            mov.IsJumping = true;
            mov.isGrounded = false;
            mov._rollsLeft += Data.rollAmount;
            
            float bounceHeight = 22f;

            if (mov.IsRolling)
            {  
                mov.stopRoll = true;
                bounceHeight *= 1.2f;
            }

            RB.velocity = new Vector2(RB.velocity.x, bounceHeight);
            _collider.enabled = false;

            StartCoroutine(EnableCollision(4f)); 

            anim.nBubblePopped = true;
        }
    }

    private IEnumerator EnableCollision(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        anim.nBubbleRespawn = true;
        mov.stopRoll = false;
        waitTime = 0.3f;
        yield return new WaitForSeconds(waitTime);
        _collider.enabled = true;
    }

}