using UnityEngine;
using UnityEngine.Video;
using Unity.InferenceEngine;
using UnityEngine.UI;
using TMPro;

public class GlobalsMgr : MonoBehaviour
{
    [Header("Sentis Assets")]
    public ModelAsset modelAsset; 
    public RawImage displayUI;        

    [Header("Detection Parameters")]
    [Range(0f, 1f)] public float confidenceThreshold = 0.4f;
    [Range(0f, 1f)] public float iouThreshold = 0.45f;
    public Color boxColor = Color.green;
    public int boxThickness = 3;

    public Worker worker;
    public Model runtimeModel;
    public Texture2D outputTexture;
    public RawImage rawImageBack;
    public int width = 640;
    public int height = 640;
    public TMP_Text textFps;
    public int cntFps;

    public bool ynWebcam;
    public VideoPlayer videoPlayer;
    public Texture2D extractedFrameVideo;
    public WebCamTexture webCamTex;
    public Texture2D extractedFrameWebcam;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
