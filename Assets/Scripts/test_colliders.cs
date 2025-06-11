using UnityEngine;

public class test_colliders : MonoBehaviour
{
   public float speed = 5.0f;

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        transform.position += movement * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other){
         //Output the Collider's GameObject's name
          Debug.Log("Entered collision with " + other.gameObject.name);
    }
}