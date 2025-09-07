using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int chunkNum;
    public int chunkLength;

    private bool initialized = false;

    private void OnEnable()
    {
        if (!initialized)
        {
            initialized = true; // first time setup
        }

        // Every time it's activated (from pool or fresh):
        GenerateLevel generator = FindObjectOfType<GenerateLevel>();
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