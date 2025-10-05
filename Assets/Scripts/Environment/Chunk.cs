using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int chunkNum;
    public int chunkLength;
    public int cullBuffer = 0;  // extra length beyond ground layer

    public void RegisterLength(GenerateLevel generator)
    {
        generator.UpdateZPos(chunkLength);
    }
}