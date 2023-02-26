using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleAnimator : MonoBehaviour
{
    private Animator anim;
    private Bubbles normalBubble;
    private SmolBubbles smolBubbles;
    public bool nBubblePopped = false;
    public bool nBubbleRespawn = false;
    public bool sBubbleMerge = false;
    public bool sBubblePopped = false;
    public bool sBubbleRespawn = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void LateUpdate()
    {
        CheckAnimationState();
    }

    void CheckAnimationState()
    {
        if (nBubblePopped)
        {
            anim.SetTrigger("Pop");
            nBubblePopped = false;
            return;
        }

        if (nBubbleRespawn)
        {
            anim.SetTrigger("Respawn");
            nBubbleRespawn = false;
            return;
        }

        if (sBubbleMerge)
        {
            anim.SetTrigger("Merge");
            sBubbleMerge = false;
            return;
        }

        if (sBubblePopped)
        {
            anim.SetTrigger("Pop");
            sBubblePopped = false;
            return;
        }

        if (sBubbleRespawn)
        {
            anim.SetTrigger("Respawn");
            sBubbleRespawn = false;
            return;
        }
    }
}
