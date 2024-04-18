using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 7;
    public float leftRightSpeed = 5;
    public bool isJumping = false;
    public bool comingDown = false;
    public bool isRolling = false;
    public bool standingUp = false;
    public static bool canFly = false;
    public bool nowCanFly = false;
    public bool isFlying = false;
    public bool floating = false;
    public GameObject playerObject;
    private Animator animator;

     public GameObject mainCam;
     public AudioSource crashThud;

      public GameObject levelControl;

    private BoxCollider boxCollider;

    void Start(){
        animator=GetComponentInChildren<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        normalhitbox();
    }

    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed, Space.World);

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            if (this.gameObject.transform.position.x > LevelBoundary.leftSide)
            {
                transform.Translate(Vector3.left * Time.deltaTime * leftRightSpeed);
            }
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            if (this.gameObject.transform.position.x < LevelBoundary.rightSide)
            {
                transform.Translate(Vector3.left * Time.deltaTime * leftRightSpeed * -1);
            }
        }

        // Ajupir-se
        if (Input.GetKey(KeyCode.S))
        {
            if (isRolling == false)
            {
                 crouchhitbox();
                //transform.Translate(Vector3.up * Time.deltaTime * moveSpeed *-1);
                //playerObject.GetComponent<Animator>().Play("Quick Roll To Run");
                animator.SetBool("isrolling",true);
                StartCoroutine(RollSequence());
                
            }   
        }


        // Jumping
        if (isFlying == false)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                if (isJumping == false)
                {
                    isJumping = true;
                    //playerObject.GetComponent<Animator>().Play("Jump");
                    animator.SetBool("isjumping",true);
                    
                    StartCoroutine(JumpSequence());
                }
            }
        } else
        { }

        if (isJumping == true)
        {
            if (comingDown == false)
            {
                //transform.Translate(Vector3.up * Time.deltaTime * 4.5f, Space.World);
            } else
            {
                //transform.Translate(Vector3.up * Time.deltaTime * -4.5f, Space.World);
            }
        }

        if (canFly == true)
        {
            if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
            {
                if (isFlying == false)
                {
                    isFlying = true;
                    if (floating == false)
                    {
                        animator.SetBool("isflying",true);
                        transform.Translate(Vector3.up * Time.deltaTime * 500, Space.World);
                        StartCoroutine(FlyTimeout());
                    }


                }
            } else
            {
                isFlying = false;

            }
        }
        nowCanFly = canFly;
    }

    void OnTriggerEnter(Collider other){
         //Output the Collider's GameObject's name
          Debug.Log("Entered collision with " + other.gameObject.tag+' '+other.gameObject.name);
          if (other.gameObject.CompareTag("obstacle"))
            {
                other.GetComponent<BoxCollider>().enabled = false;
                animator.Play("Stumble Backwards");
                crashThud.Play();
                mainCam.GetComponent<Animator>().enabled = true;
                levelControl.GetComponent<EndRunSequence>().enabled = true;
                 // Disable this script
                this.enabled = false;
            }
    }


    void normalhitbox(){
         // Set new size
        boxCollider.size = new Vector3(0.67f,1,0.58f);
        // Set new center position
         boxCollider.center = new Vector3(0,0, -0.42f);

      
    }
    void jumphitbox(){
        // Set new center position
        boxCollider.center = new Vector3(0,1,-0.42f);
        // Set new size
        boxCollider.size = new Vector3(0.67f,0.78f,0.58f);
    }

   void crouchhitbox(){
        // Set new center position
        boxCollider.center = new Vector3(0,-0.79f,-0.42f);
        // Set new size
        boxCollider.size = new Vector3(0.67f,0.24f,0.58f);
    }


    IEnumerator JumpSequence()
    {   
        jumphitbox();
        yield return new WaitForSeconds(0.30f);
        comingDown = true;
        yield return new WaitForSeconds(0.30f);
        isJumping = false;
        comingDown = false;
        //playerObject.GetComponent<Animator>().Play("Standard Run");
        animator.SetBool("isjumping",false);
        normalhitbox();
    }

    IEnumerator RollSequence()
    {
        yield return new WaitForSeconds(0.45f);
        standingUp = true;
        yield return new WaitForSeconds(0.45f);
        isRolling = false;
        //playerObject.GetComponent<Animator>().Play("Standard Run");
        animator.SetBool("isrolling",false);
        normalhitbox();
    }

    IEnumerator FlyTimeout()
    {
        yield return new WaitForSeconds(5);
        transform.Translate(Vector3.up * Time.deltaTime * 3.5f, Space.World);
        //playerObject.GetComponent<Animator>().Play("Flying");
         animator.SetBool("isflying",false);
        floating = true;
    }
}
