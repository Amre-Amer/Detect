using UnityEngine;
using UnityEngine.Rendering;

public class VideoFrameGrabber : MonoBehaviour
{
    public GlobalsMgr g;

    void Awake()
    {
        g.videoPlayer.playOnAwake = false;
        g.videoPlayer.isLooping = true;
    }

    public void StartVideo()
    {
        g.videoPlayer.clip = g.videoClip;
        g.videoPlayer.Play();
        Invoke(nameof(GrabCurrentFrame), g.delay); // wait for video
    }

    public void GrabCurrentFrame()
    {
        if (g.videoPlayer == null || g.videoPlayer.texture == null) return;
        AsyncGPUReadback.Request(g.videoPlayer.texture, 0, OnFrameReadback);
    }

    private void OnFrameReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.LogError("GPU readback error occurred.");
            return;
        }
        Texture2D extractedFrame0 = new Texture2D(g.videoPlayer.texture.width, g.videoPlayer.texture.height, TextureFormat.RGBA32, false);
        extractedFrame0.LoadRawTextureData(request.GetData<byte>());
        extractedFrame0.Apply();
        g.extractedFrameVideo = extractedFrame0;
        // poor man's loop
        GrabCurrentFrame();
    }
}
