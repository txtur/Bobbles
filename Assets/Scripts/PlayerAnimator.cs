using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private PlayerMovement mov;
    private Animator squish;
    private Animator sprite;
    private SpriteRenderer spriteRend;
    private SpriteRenderer squishRend;

    [Header("Particle FX")]
    [SerializeField] private GameObject jumpFX;
    [SerializeField] private GameObject landFX;
    private ParticleSystem _jumpParticle;
    private ParticleSystem _landParticle;

    public bool startedJumping {  private get; set; }
    public bool startedFunnyJump {  private get; set; }
    public bool justLanded { private get; set; }
    public bool startedRolling { private get; set; }
    public bool startedBounce { private get; set; }
    public bool startedCrouching { private get; set; }

    private void Start()
    {
        mov = GetComponent<PlayerMovement>();
        squishRend = this.gameObject.transform.GetChild(1).GetComponent<SpriteRenderer>();
        squish = squishRend.GetComponent<Animator>();
        spriteRend = this.gameObject.transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>();
        sprite = spriteRend.GetComponent<Animator>();

        _jumpParticle = jumpFX.GetComponent<ParticleSystem>();
        _landParticle = landFX.GetComponent<ParticleSystem>();

        ParticleSystem.MainModule jumpPSettings = _jumpParticle.main;
        jumpPSettings.startColor = Color.grey;
        ParticleSystem.MainModule landPSettings = _landParticle.main;
        landPSettings.startColor = Color.grey;
    }

    private void LateUpdate()
    {
        CheckAnimationState();
    }

    private void CheckAnimationState()
    {
        squish.SetFloat("Vel Y", mov.RB.velocity.y);
        sprite.SetFloat("Vel X", Mathf.Abs(Input.GetAxis("Horizontal")));
        sprite.SetFloat("Vel Y", mov.RB.velocity.y);

        if (startedJumping)
        {
            squish.SetTrigger("Jump");
            sprite.SetTrigger("Jump");
            GameObject obj = Instantiate(jumpFX, transform.position - (Vector3.up * transform.localScale.y / 2), Quaternion.Euler(-90, 0, 0));
            Destroy(obj, 1);
            startedJumping = false;
            return;
        }

        if (startedFunnyJump)
        {
            squish.SetTrigger("Jump");
            sprite.SetTrigger("Funny Jump");
            GameObject obj = Instantiate(jumpFX, transform.position - (Vector3.up * transform.localScale.y / 2), Quaternion.Euler(-90, 0, 0));
            Destroy(obj, 1);
            startedFunnyJump = false;
            return;
        }

        if (justLanded)
        {
            squish.SetTrigger("Land");
            GameObject obj = Instantiate(landFX, transform.position - (Vector3.up * transform.localScale.y / 1.5f), Quaternion.Euler(-90, 0, 0));
            Destroy(obj, 1);
            justLanded = false;
            return;
        }

        if (startedRolling)
        {
            squish.SetTrigger("Idle");
            sprite.SetTrigger("Roll");
            startedRolling = false;
            return;
        }

        if (startedBounce)
        {
            squish.SetTrigger("Jump");
            sprite.SetTrigger("Bounce");
            GameObject obj = Instantiate(jumpFX, transform.position - (Vector3.up * transform.localScale.y / 2), Quaternion.Euler(-90, 0, 0));
            Destroy(obj, 1);
            startedBounce = false;
            return;
        }

        if (startedCrouching)
        {
            
        }
    }
}