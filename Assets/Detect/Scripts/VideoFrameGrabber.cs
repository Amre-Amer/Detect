using UnityEngine;
using UnityEngine.Rendering;

public class VideoFrameGrabber : MonoBehaviour
{
    public GlobalsMgr g;
    float interval = .025f;

    public void StartVideo()
    {
        g.videoPlayer.Play();
        Invoke(nameof(GrabCurrentFrame), 1);
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
        GrabCurrentFrame();
    }
}
