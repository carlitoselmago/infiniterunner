using UnityEngine;

public class ImageCubePiece : MonoBehaviour
{
    private ImageCubeWall wall;

    public void Initialize(ImageCubeWall manager)
    {
        wall = manager;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Player"))
            return;

        if (wall == null)
        {
            Debug.LogError("ImageCubeWall reference missing.");
            return;
        }

        wall.BreakAt(collision.GetContact(0).point);
    }
}