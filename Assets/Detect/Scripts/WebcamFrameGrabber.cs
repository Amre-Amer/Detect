using UnityEngine;

public class WebcamFrameGrabber : MonoBehaviour
{
    public GlobalsMgr g;
    public void StartWebcam()
    {
        // Initialize the first available webcam
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            g.webCamTex = new WebCamTexture(devices[0].name);
            g.webCamTex.Play();
        }
        else
        {
            Debug.LogError("No webcam found!");
        }
    }

    public void UpdateWebcam()
    {
        // Check if the webcam is playing and has new frame data
        if (g.webCamTex != null && g.webCamTex.didUpdateThisFrame)
        {
            // Option 1: Access raw pixel colors (Color32 array)
            Color32[] pixels = g.webCamTex.GetPixels32();

            // Option 2: Copy the WebCamTexture into a Texture2D to save or process
            if (g.extractedFrameWebcam == null || g.extractedFrameWebcam.width != g.webCamTex.width || g.extractedFrameWebcam.height != g.webCamTex.height)
            {
                g.extractedFrameWebcam = new Texture2D(g.webCamTex.width, g.webCamTex.height, TextureFormat.RGBA32, false);
            }
            
            g.extractedFrameWebcam.SetPixels32(pixels);
            g.extractedFrameWebcam.Apply();

            // Example: Encode to PNG bytes if you need to save to disk
            // byte[] bytes = snapshot.EncodeToPNG();
        }
    }

    void OnDestroy()
    {
        if (g.webCamTex != null)
        {
            g.webCamTex.Stop();
        }
    }
}
