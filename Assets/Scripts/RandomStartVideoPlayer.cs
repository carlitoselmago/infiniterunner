using UnityEngine;
using UnityEngine.Video;

public class RandomStartVideoPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        long startFrame = (long)Random.Range(0, (int)videoPlayer.frameCount);
        videoPlayer.frame = startFrame;
    }
}