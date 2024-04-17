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
    void Start(){
        animator=GetComponentInChildren<Animator>();
        Debug.Log(animator);
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
                transform.Translate(Vector3.up * Time.deltaTime * 4.5f, Space.World);
            } else
            {
                transform.Translate(Vector3.up * Time.deltaTime * -4.5f, Space.World);
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

    IEnumerator JumpSequence()
    {
        yield return new WaitForSeconds(0.30f);
        comingDown = true;
        yield return new WaitForSeconds(0.30f);
        isJumping = false;
        comingDown = false;
        //playerObject.GetComponent<Animator>().Play("Standard Run");
        animator.SetBool("isjumping",false);
    }

    IEnumerator RollSequence()
    {
        yield return new WaitForSeconds(0.45f);
        standingUp = true;
        yield return new WaitForSeconds(0.45f);
        isRolling = false;
        //playerObject.GetComponent<Animator>().Play("Standard Run");
        animator.SetBool("isrolling",false);
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
