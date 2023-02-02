using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public Board board;
    public float speed = 5f;
    private Rigidbody2D myBody;
    private Animator anim;
    private bool isGrounded;
    private bool jumped;
  

    //private GameObject winText;
    private AudioSource audioManager;

    private int jump;
    private int maxJumps = 2;
    private float jumpPower = 10f;


    public Transform groundCheckPosition;


    public LayerMask groundLayer;
    public LayerMask tileLayer;

    /*public bool IsDead {
        get { return isDead; }
        set { isDead = value; }
        }
    */
    private void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
       // winText = GameObject.Find("WinText");
       // winText.SetActive(false);
        audioManager = GetComponent<AudioSource>();
      


    }
    // Start is called before the first frame update
    void Start()
    {
        jump = maxJumps;
    }

    // Update is called once per frame
    void Update() //called 60 times (every frame)
    {


        //Debug.DrawRay(groundCheckPosition.position, Vector2.down * 0.1f, Color.red);
        if (GameManager.IsInputEnabled)
        {

            CheckIfGrounded();
            PlayerJump();
        }


    }

    private void FixedUpdate() //called every 2-3 frames - can edit this in project settings-time. Place for physics
    {
        if (GameManager.IsInputEnabled)
               PlayerWalk();
    }

    void PlayerWalk()
    {

        float h = Input.GetAxisRaw("Horizontal");  //raw- means whole number

        if (h > 0)
        {

            if (CheckEdge())
            {
                myBody.velocity = new Vector2(speed, myBody.velocity.y);
                ChangeDirection(1);
            }
            else
            {
                myBody.velocity = new Vector2(0f, myBody.velocity.y); ChangeDirection(1);
                BounceFromEdge(); //find a better way to keep borders
                
            }

        }
        else if (h < 0)
        {

            if (CheckEdge())
            {
                myBody.velocity = new Vector2(-speed, myBody.velocity.y);
                ChangeDirection(-1);
            }
            else
            {
                myBody.velocity = new Vector2(0f, myBody.velocity.y); ChangeDirection(-1);
                BounceFromEdge();
            }
        }
        else
        {

            myBody.velocity = new Vector2(0f, myBody.velocity.y);
        }
       // print("speed = " + Mathf.Abs((int)myBody.velocity.x));
        anim.SetInteger("Speed", Mathf.Abs((int)myBody.velocity.x));
    }

    void ChangeDirection(int direction)
    {

        Vector3 tempScale = transform.localScale;
        tempScale.x = direction;
        transform.localScale = tempScale;

    }


    void CheckIfGrounded()
    {
        isGrounded = Physics2D.Raycast(groundCheckPosition.position, Vector2.down, 0.1f, groundLayer);
        if (isGrounded)
        {
            // we are on the ground and we jumped before
            if (jumped)
            {

                jumped = false;
                anim.SetBool("Jump", false);
                jump = maxJumps;
            }
        }
    }

    void PlayerJump()
    {
        if (isGrounded)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                jumped = true;
                myBody.velocity = new Vector2(myBody.velocity.x, jumpPower);

                anim.SetBool("Jump", true);
                jump--;
            }

        }
        else if ((jumped) && (jump > 0))
        { // we are in the air and we jumped before 
            if (Input.GetKeyDown(KeyCode.Space))
            {
                myBody.velocity = new Vector2(myBody.velocity.x, jumpPower);

                jump--;
            }

        }
    }

   
    void BounceFromEdge()
    {
        float tempX = transform.position.x;
        float tempY = transform.position.y;

        if (tempX > 0) tempX -= 0.01f;
        else tempX += 0.01f;

        if (tempY > 0) tempY -= 0.01f;
        else tempY += 0.01f;

        transform.position = new Vector2(tempX, tempY); 

    }
    bool CheckEdge()
    {
        RectInt bounds = board.Bounds;
        
        

        if (transform.position.x<(bounds.xMax-0.5f) && transform.position.x > (bounds.xMin+0.5f)
            && transform.position.y < (bounds.yMax-0.5f)&& transform.position.y > (bounds.yMin+0.5f))
            {
                return true;
            }
        
        return false;
    }
   
}//class





































