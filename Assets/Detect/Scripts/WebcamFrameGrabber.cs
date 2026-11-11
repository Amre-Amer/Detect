using UnityEngine;

public class WebcamFrameGrabber : MonoBehaviour
{
    private WebCamTexture webCamTex;
    public Texture2D extractedFrame;

    public void StartWebcam()
    {
        // Initialize the first available webcam
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            webCamTex = new WebCamTexture(devices[0].name);
            webCamTex.Play();
        }
        else
        {
            Debug.LogError("No webcam found!");
        }
    }

    public void UpdateWebcam()
    {
        // Check if the webcam is playing and has new frame data
        if (webCamTex != null && webCamTex.didUpdateThisFrame)
        {
            // Option 1: Access raw pixel colors (Color32 array)
            Color32[] pixels = webCamTex.GetPixels32();

            // Option 2: Copy the WebCamTexture into a Texture2D to save or process
            if (extractedFrame == null || extractedFrame.width != webCamTex.width || extractedFrame.height != webCamTex.height)
            {
                extractedFrame = new Texture2D(webCamTex.width, webCamTex.height, TextureFormat.RGBA32, false);
            }
            
            extractedFrame.SetPixels32(pixels);
            extractedFrame.Apply();

            // Example: Encode to PNG bytes if you need to save to disk
            // byte[] bytes = snapshot.EncodeToPNG();
        }
    }

    void OnDestroy()
    {
        if (webCamTex != null)
        {
            webCamTex.Stop();
        }
    }
}
