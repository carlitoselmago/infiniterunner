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


/*using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int chunkNum;
    public int chunkLength;

    private void OnEnable()
    {
        GenerateLevel generator = FindObjectOfType<GenerateLevel>();
        generator.UpdateZPos(chunkLength);
    }
}*/