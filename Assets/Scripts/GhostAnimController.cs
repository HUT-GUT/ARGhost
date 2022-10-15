using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAnimController : MonoBehaviour
{
    public Animator animator;
    private Game1Manager game1Manager;

    private void Awake()
    {
        game1Manager = GameObject.Find("GameManager").GetComponent<Game1Manager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setRunawayTrigger()
    {
        if (game1Manager.currentState == Game1Manager.UserState.Ready)
        {
            animator.SetTrigger("Runaway");
        }
    }
}
