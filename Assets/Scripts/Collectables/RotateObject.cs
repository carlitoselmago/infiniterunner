using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotateSpeed = 1;
    private float startingPos = 0;

    private void OnEnable()
    {
        startingPos = Random.Range(0, 360); // defines initial rotation position
        transform.Rotate(0, 0, startingPos, Space.Self);
    }
    void Update()
    {
        transform.Rotate( 0,0, rotateSpeed, Space.Self);
    }
}