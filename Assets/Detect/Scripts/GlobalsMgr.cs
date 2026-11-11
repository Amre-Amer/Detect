using UnityEngine;
using UnityEngine.Video;
using Unity.InferenceEngine;
using UnityEngine.UI;
using TMPro;

public class GlobalsMgr : MonoBehaviour
{
    [Header("Video or Webcam")]
    public bool ynWebcam;
    [Header("Inference Engine")]
    public ModelAsset modelAsset; 
    public RawImage displayUI;        
    [Range(0f, 1f)] public float confidenceThreshold = 0.4f;
    [Range(0f, 1f)] public float iouThreshold = 0.45f;
    // public Color boxColor = Color.green;
    // public int boxThickness = 3;
    public Worker worker;
    public Model runtimeModel;

    [HideInInspector]
    public Texture2D extractedFrameVideo;
    [HideInInspector]
    public WebCamTexture webCamTex;
    [HideInInspector]
    public Texture2D extractedFrameWebcam;
    [HideInInspector]
    public Texture2D outputTexture;

    [Header("Other")]
    public RawImage rawImageBack;
    public int width = 640;
    public int height = 640;
    public TMP_Text textFps;
    public int cntFps;
    public VideoPlayer videoPlayer;
    public VideoClip videoClip;
    public float delay;

    void Awake()
    {
        delay = 1;
        width = 640;
        height = 640;
        confidenceThreshold = 0.4f;
        iouThreshold = 0.45f;
    }
}
