using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int chunkNum;
    public int chunkLength;

    public void RegisterLength(GenerateLevel generator)
    {
        generator.UpdateZPos(chunkLength);
    }
}