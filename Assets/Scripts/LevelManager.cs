using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    [Header("Script References")]
    private PlayerInput playerInput;
    private SmolBubbles smolBubbles;

    [Header("Game Objects")]
    [SerializeField] GameObject SB1;
    [SerializeField] GameObject SB2;
    [SerializeField] GameObject SB3;
    GameObject other;
    SpriteRenderer otherRend;

    [Header("Public Variables")]
    public float numberOfBubbles;
    public bool respawnBubbles = false;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        smolBubbles = SB1.GetComponent<SmolBubbles>();
    }

    void Update()
    { 
        if (numberOfBubbles == 3)
        {
            Merge();
        }
        
        if (numberOfBubbles == 2) 
            smolBubbles.stoppingDistance = 1.4f;

        if (smolBubbles.bigBubblePopped == true)
        {
            SB2.SetActive(true);
            SB3.SetActive(true);

            if (GameObject.FindGameObjectsWithTag("SmolBubble").Length == 3)
            {
                respawnBubbles = true;
            }
        }
    }

    void Merge()
    {
        SB2.SetActive(false);
        SB3.SetActive(false);

        if (GameObject.FindGameObjectsWithTag("SmolBubble").Length == 1)
        {
            smolBubbles.Merge();
            numberOfBubbles = 0;
        }
    }
}
