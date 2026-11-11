using TMPro;
using UnityEngine;

public class WebcamDisplay : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    public Renderer quadRenderer;
    public TMP_Text textFps;

    int width = 1152;
    int height = 600;
    int fps = 30;
    int cntFps;

    void Start()
    {
        StartWebcam();
        InvokeRepeating(nameof(Fps), 1, 1);
    }

    void Update()
    {
        cntFps++;
    }

    void StartWebcam()
    {
        // quadRenderer = GetComponent<Renderer>();

        // Ensure there is at least one webcam connected
        if (WebCamTexture.devices.Length > 0)
        {
            // Pick the first available default camera device
            // string deviceName = WebCamTexture.devices[0].name;
            string deviceName = "OBS Virtual Camera";

            // Initialize the texture (Requesting 1920x1080 at 30 FPS)
            webcamTexture = new WebCamTexture(deviceName, width, height, fps);

            // Assign the webcam texture to the Quad's material
            quadRenderer.material.mainTexture = webcamTexture;

            // Start the webcam stream
            webcamTexture.Play();
            
            // Adjust the Quad's aspect ratio based on the webcam's properties
            // AdjustScale();
        }
        else
        {
            Debug.LogError("No webcam device detected.");
        }
    }

    void Fps()
    {
        textFps.text = "fps: " + cntFps;
        cntFps = 0;
    }

    void AdjustScale()
    {
        // Default aspect ratio correction (e.g., matching a 16:9 format)
        // Note: webcamTexture.width/height might return 16x16 until a few frames have passed.
        transform.localScale = new Vector3(width, height, 1f).normalized;
    }

    void OnDestroy()
    {
        // Properly stop the webcam when the object or scene is destroyed
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }
}
