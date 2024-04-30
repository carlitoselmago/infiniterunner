using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 7;
    private float initialmoveSpeed = 0;
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

    private float targetHeight = 8.0f;
    private float startY;
    private float originY;
     private float jumpedHeight;
    
    private float speed = 2.0f; // Adjusted speed for a more natural feel
 
    private float elapsedTime = 0.0f; // Track time since the start of the jump

    public GameObject flycoin;

    public AudioSource coinFX;

 

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        startY = transform.position.y;
        originY=startY;
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
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            if (isRolling == false)
            {
                crouchhitbox();
                //transform.Translate(Vector3.up * Time.deltaTime * moveSpeed *-1);
                //playerObject.GetComponent<Animator>().Play("Quick Roll To Run");
                animator.SetBool("isrolling", true);
                StartCoroutine(RollSequence());

            }
        }


        // Jumping
        if (isFlying == false)
        {
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow))
            {
                if (isJumping == false)
                {
                    isJumping = true;
                    //playerObject.GetComponent<Animator>().Play("Jump");
                    animator.SetBool("isjumping", true);

                    StartCoroutine(JumpSequence());
                }
            }
        }
        else
        { }

        if (isJumping == true)
        {
            if (comingDown == false)
            {
                //transform.Translate(Vector3.up * Time.deltaTime * 4.5f, Space.World);
            }
            else
            {
                //transform.Translate(Vector3.up * Time.deltaTime * -4.5f, Space.World);
            }
        }
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
        {

            initialmoveSpeed = moveSpeed;

            animator.SetBool("isflying", true);
            if (!isFlying)
            {
                //create array of coins
                float currentZ = this.transform.position.z + 60;

                for (int i = 0; i < 18; i++)
                {
                    GameObject newcoin = Instantiate(flycoin, Vector3.zero, Quaternion.identity);
                    newcoin.transform.localPosition = new Vector3(this.transform.position.x, targetHeight, currentZ + (i * 3));

                }
                StartCoroutine(FlyTimeout());
            }
            isFlying = true;
           
        }

        //test fly
        if (floating)
        {
           // PerformFall();
            jumpedHeight=interpolateValueY(false,jumpedHeight,originY,2.8f);
           
        }
        else
        {
            if (isFlying)
            {
               // PerformFly();
                startY=interpolateValueY(true,startY,targetHeight,1f);
                //Debug.Log(startY);
                //Debug.Log(targetHeight);
            }
        }

        /*
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
                        
                       // StartCoroutine(FlyTimeout());
                    }
                }
            } else
            {
                isFlying = false;
            }
        }
        nowCanFly = canFly;
        */
    }

    void OnTriggerEnter(Collider other)
    {
        //Output the Collider's GameObject's name
        Debug.Log("Entered collision with " + other.gameObject.tag + ' ' + other.gameObject.name);
        if (other.gameObject.CompareTag("obstacle"))
        {
            /*
            other.GetComponent<BoxCollider>().enabled = false;
            animator.Play("Stumble Backwards");
            crashThud.Play();
            mainCam.GetComponent<Animator>().enabled = true;
            levelControl.GetComponent<EndRunSequence>().enabled = true;
            // Disable this script
            this.enabled = false;
            */
        }
        if (other.gameObject.CompareTag("coin"))
        {
            coinFX.Play();
            CollectableControl.coinCount += 1;
            other.gameObject.SetActive(false);
        }
    }


    void normalhitbox()
    {
        // Set new size
        boxCollider.size = new Vector3(0.67f, 1, 0.58f);
        // Set new center position
        boxCollider.center = new Vector3(0, 0, -0.42f);


    }
    void jumphitbox()
    {
        // Set new center position
        boxCollider.center = new Vector3(0, 1, -0.42f);
        // Set new size
        boxCollider.size = new Vector3(0.67f, 0.78f, 0.58f);
    }

    void crouchhitbox()
    {
        // Set new center position
        boxCollider.center = new Vector3(0, -0.79f, -0.42f);
        // Set new size
        boxCollider.size = new Vector3(0.67f, 0.24f, 0.58f);
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
        animator.SetBool("isjumping", false);
        normalhitbox();
    }

    IEnumerator RollSequence()
    {
        yield return new WaitForSeconds(0.45f);
        standingUp = true;
        yield return new WaitForSeconds(0.45f);
        isRolling = false;
        //playerObject.GetComponent<Animator>().Play("Standard Run");
        animator.SetBool("isrolling", false);
        normalhitbox();
    }
    IEnumerator FlyTimeout()
    {
        yield return new WaitForSeconds(4);
        
        floating = true;
         animator.SetBool("isflying", false);
        jumpedHeight=this.transform.position.y;
        yield return new WaitForSeconds(1);
       
        isFlying = false;
        startY=originY;
         yield return new WaitForSeconds(1);
        floating = false;
       
   
    }
    IEnumerator FlyTimeout_OLD()
    {
        float targetHeight = 5.0f;
        float speed = 5.0f;
        bool movingUp = false;
        float startY = 0;
        float step = speed * Time.deltaTime;
        float newY = Mathf.Lerp(transform.position.y, targetHeight + startY, step);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        if (Mathf.Abs(transform.position.y - (targetHeight + startY)) < 0.01f)
        {
            transform.position = new Vector3(transform.position.x, targetHeight + startY, transform.position.z);
            movingUp = false;
        }
        // transform.Translate(Vector3.up * Time.deltaTime * 500, Space.World);
        yield return new WaitForSeconds(4);
        //transform.Translate(Vector3.up * Time.deltaTime * 3.5f, Space.World);
        //playerObject.GetComponent<Animator>().Play("Flying");
        animator.SetBool("isflying", false);
        jumpedHeight=this.transform.position.y;
        isFlying=false;
        floating = true;
    }

    private float interpolateValueY(bool easingOut=true,float origin=0.0f,float target=5.0f,float intspeed=0.2f){
        float fraction = Time.deltaTime * intspeed;
        
        // Check the type of easing
        if (easingOut)
        {
            // Easing out: movement starts quickly and slows down
            fraction = 1 - Mathf.Pow(1 - fraction, 3);
            if (moveSpeed<30-0f){
                moveSpeed+=fraction*10;
            }
        }
        else
        {
            // Easing in: movement starts slowly and speeds up
            fraction = Mathf.Pow(fraction, 0.9f); // Adjust the power to make the easing smoother (the smaller the faster)
             if (moveSpeed>12.0f){
                moveSpeed-=fraction*2;
            }
        }

        // Interpolate based on the currentY and targetY using the calculated fraction
        float currentY = Mathf.Lerp(origin, target, fraction);

        // Update the GameObject's position
        transform.position = new Vector3(transform.position.x, currentY, transform.position.z);
        origin = currentY;
        
        return origin;
    }
    private void PerformFly()
    {
        /*
        elapsedTime += Time.deltaTime;
        float timeFraction = elapsedTime / (speed / 2); // Time fraction over the total duration

        if (timeFraction < 1.0f)
        {
            // Ease out effect using Mathf.SmoothStep for smoother transition
            float newY = Mathf.SmoothStep(startY, startY + targetHeight, timeFraction);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            moveSpeed += 0.05f;
        }
        else
        {
            // Ensure it ends exactly at the target height and stop the jump
            transform.position = new Vector3(transform.position.x, startY + targetHeight, transform.position.z);
        }
        */
       float tweenspeed=1.0f;
         // Calculate the new Y position with easing out effect
        float newY = Mathf.Lerp(startY, targetHeight, 1 - Mathf.Pow(1 - Time.deltaTime * tweenspeed, 3));

        // Apply the new Y position
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Optionally update startY to continue the movement from the current position
        startY = newY;
    }

    private void PerformFall()
    {
        elapsedTime += Time.deltaTime;
        float timeFraction = elapsedTime / (5); // Time fraction over the total duration
       
        if (timeFraction < 2f)
        {
            
            // Ease out effect using Mathf.SmoothStep for smoother transition
            float initialY = jumpedHeight;//startY + jumpedHeight; // Start falling from this height
            
            float newY = Mathf.SmoothStep(initialY, startY, timeFraction);
            Debug.Log(newY);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            moveSpeed -= 0.01f; // Increase move speed (assuming it's needed for your context)
        }
        else
        {
            // Ensure it ends exactly at the startY and stop the fall
            transform.position = new Vector3(transform.position.x, startY, transform.position.z);
            //isFlying = false; // Consider renaming this flag to better suit your context, like isFalling or movementFinished
            
        }
    }



}