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


/*
 * TRY OUT!
 * 
 * using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int chunkNum;
    public int baseLength = 25;          // default section length
    public int overrideLength = -1;      // -1 means no override

    public int GetLength()
    {
        return (overrideLength > 0) ? overrideLength : baseLength;
    }

    public void RegisterLength(GenerateLevel generator)
    {
        generator.UpdateZPos(GetLength());
    }
}
*/