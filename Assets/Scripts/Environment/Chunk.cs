using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Transform endPoint;
    public static int chunkLength = 200;

    void OnDrawGizmos()
    {
        if (endPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(endPoint.position, 0.5f);
            Gizmos.DrawLine(endPoint.position, endPoint.position + endPoint.forward * 2);
        }
    }

}