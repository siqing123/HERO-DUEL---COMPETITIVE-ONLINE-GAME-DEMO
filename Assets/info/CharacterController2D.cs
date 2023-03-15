using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterController2D : MonoBehaviourPun
{
    private Animator animator;

    [SerializeField] private LayerMask m_WhatIsGround;                          
    [SerializeField] private Transform m_GroundCheck;

    private int owerId;
    [SerializeField]
    private float m_JumpForce = 10f;
    [SerializeField]
    private float m_doubleJumpForce = 9f;

    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;
    private Vector3 m_Velocity = Vector3.zero;
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = 0.05f;	// How much to smooth out the movement

    [SerializeField]
    private float speed;
    public bool doubleJump;
    const float k_GroundedRadius = .2f;

    public bool m_Grounded;

    public Transform XminYmin;
    public Transform XmaxYmax;

    private Vector3 mScale;

    bool isTouchingFront;
    public Transform frontCheck;
    public bool wallSliding;
    public float wallSlidingSpeed;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        mScale = transform.localScale;

    }
    void Start()
    {
        //doubleJump = true;
        XminYmin = GameObject.Find("positionXminYmin").transform;
        XmaxYmax = GameObject.Find("positionXmaxYmax ").transform;
    }

    // Update is called once per frame
    void Update()
    {
        

        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        check();

        if (m_Grounded && !Input.GetKey(KeyCode.Space))
        {
            doubleJump = false;
        }

        if (m_Rigidbody2D.velocity.x == 0)
        {
            animator.SetFloat("speed", 0);
        }

        if (Input.GetKey(KeyCode.D))
        {
            if(this.transform.position.x < XmaxYmax.position.x)
            {
                Move(speed, false);
            }
            else
            {
                Move(0, false);
            }
            animator.SetFloat("speed", Mathf.Abs(m_Rigidbody2D.velocity.x));
        }
        if (Input.GetKey(KeyCode.A))
        {
            if(this.transform.position.x > XminYmin.position.x)
            {
                Move(-speed, false);
            }
            else
            {
                Move(0, false);
            }
            animator.SetFloat("speed", Mathf.Abs(m_Rigidbody2D.velocity.x));
        }
        //if (Input.GetKey(KeyCode.Space))
        //{         
        //   Move(-0, true);
        //   animator.SetBool("isJumping", true);     
        //}

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (m_Grounded || doubleJump)
            {
                m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, doubleJump ? m_doubleJumpForce: m_JumpForce);
                doubleJump = !doubleJump;
            }
        }

        if(Input.GetKeyUp(KeyCode.Space) && m_Rigidbody2D.velocity.y > 0)
        {
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_Rigidbody2D.velocity.y * 0.5f);
        }
    }

    private void Move(float move,bool jump)
    {
        Vector3 targetVelocity = new Vector2(move * 5.0f, m_Rigidbody2D.velocity.y);
        // And then smoothing it out and applying it to the character
        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);


        if (move > 0 && !m_FacingRight)
        {
            Flip();
        }
        else if (move < 0 && m_FacingRight)
        {
            Flip();
        }

        if ( jump && m_Grounded)
        {
            // Add a vertical force to the player.
            
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
         
        }     
    }

    private void Flip()
    {

        m_FacingRight = !m_FacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
        mScale = theScale;
    }

    public void setId(int num)
    {
        owerId = num;
    }

    private void check()
    {

        float input = Input.GetAxisRaw("Horizontal");
        m_Grounded = false;
        isTouchingFront = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;               
                animator.SetBool("isJumping", false);
            }
        }

        isTouchingFront = Physics2D.OverlapCircle(frontCheck.position, k_GroundedRadius, m_WhatIsGround);

        if (isTouchingFront == true && m_Grounded == false && input != 0)
        {
            wallSliding = true;
        }
        else
        {
            wallSliding = false;
        }

        if(wallSliding)
        {
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x,Mathf.Clamp(m_Rigidbody2D.velocity.y, wallSlidingSpeed, float.MaxValue));
        }

        if (this.transform.position.y < XminYmin.position.y)
        {
            this.GetComponent<Hero>().takeDamage(100);
        }

        if (this.transform.position.y > XmaxYmax.position.y)
        {
            this.GetComponent<Hero>().takeDamage(100);
        }
    } 
}

