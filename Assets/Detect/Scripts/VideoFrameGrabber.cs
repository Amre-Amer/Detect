using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Rendering;

public class VideoFrameGrabber : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Texture2D extractedFrame;
    float interval = .025f;

    public void StartVideo()
    {
        videoPlayer.Play();
        Invoke(nameof(GrabCurrentFrame), 1);
    }

    public void GrabCurrentFrame()
    {
        if (videoPlayer == null || videoPlayer.texture == null) return;
        AsyncGPUReadback.Request(videoPlayer.texture, 0, OnFrameReadback);
    }

    private void OnFrameReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.LogError("GPU readback error occurred.");
            return;
        }
        Texture2D extractedFrame0 = new Texture2D(videoPlayer.texture.width, videoPlayer.texture.height, TextureFormat.RGBA32, false);
        extractedFrame0.LoadRawTextureData(request.GetData<byte>());
        extractedFrame0.Apply();
        extractedFrame = extractedFrame0;
        GrabCurrentFrame();
    }
}
