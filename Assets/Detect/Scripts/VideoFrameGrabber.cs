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
        // InvokeRepeating(nameof(GrabCurrentFrame), interval, interval);
        Invoke(nameof(GrabCurrentFrame), 1);
        //  GrabCurrentFrame();
    }

    public void GrabCurrentFrame()
    {
        // Debug.Log("GrabCurrentFrame\n");
        if (videoPlayer == null || videoPlayer.texture == null) return;

        // Request the GPU to read back the video player texture data
        AsyncGPUReadback.Request(videoPlayer.texture, 0, OnFrameReadback);
    }

    private void OnFrameReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.LogError("GPU readback error occurred.");
            return;
        }

        // Create a texture matching the video's dimensions
        Texture2D extractedFrame0 = new Texture2D(videoPlayer.texture.width, videoPlayer.texture.height, TextureFormat.RGBA32, false);

        // extractedFrame = TextureExtensions.Resized(extractedFrame, 640, 640);
        
        // Load the raw data into the texture
        extractedFrame0.LoadRawTextureData(request.GetData<byte>());
        extractedFrame0.Apply();

        extractedFrame = extractedFrame0;
        // extractedFrame = TextureExtensions.Resized(extractedFrame0, 640, 640);
        
        // 'extractedFrame' is now ready to use, save, or apply to a material
        // Debug.Log("Frame grabbed successfully!");
        // Invoke(nameof(GrabCurrentFrame), 1);
        GrabCurrentFrame();
    }
}
