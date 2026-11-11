using System.Collections.Generic;
using TMPro;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.UI;

public class YOLOv8PersonDetector2 : MonoBehaviour
{
    public VideoFrameGrabber videoFrameGrabber;
    public WebcamFrameGrabber webcamFrameGrabber;
    public NmsVisualizer nmsVisualizer;
    [Header("Sentis Assets")]
    public ModelAsset modelAsset; 
    public RawImage displayUI;        

    [Header("Detection Parameters")]
    [Range(0f, 1f)] public float confidenceThreshold = 0.4f;
    [Range(0f, 1f)] public float iouThreshold = 0.45f;
    public Color boxColor = Color.green;
    public int boxThickness = 3;

    private Worker worker;
    private Model runtimeModel;
    private Texture2D outputTexture;
    public RawImage rawImageBack;
    int width = 640;
    int height = 640;
    public TMP_Text textFps;
    int cntFps;

    public bool ynWebcam;

    struct Detection
    {
        public Rect rect; 
        public float score;
    }

    void Start()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);

        // Prepare working output texture
        outputTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        displayUI.texture = outputTexture;

        if (ynWebcam) {
            webcamFrameGrabber.StartWebcam();
        } else
        {
            videoFrameGrabber.StartVideo();
        }

        InvokeRepeating(nameof(Fps), 1, 1);
    }

    void Update()
    {
        if (ynWebcam)
        {
            webcamFrameGrabber.UpdateWebcam();
            InferTexture2D(webcamFrameGrabber.extractedFrame);  
        } else
        {
            if (videoFrameGrabber.extractedFrame != null)
            {
                InferTexture2D(videoFrameGrabber.extractedFrame);  
            }
        }
        cntFps++;
    }

    void Fps()
    {
        textFps.text = "fps: " + cntFps;
        cntFps = 0;
    }

    void InferTexture2D(Texture2D tex)
    {
        Texture2D texInference = TextureExtensions.Resized(tex, 640, 640);
        rawImageBack.texture = texInference;
        RunInference(texInference);
    }

    public void RunInference(Texture2D tex)
    {
        float t = Time.realtimeSinceStartup;
        using Tensor<float> inputTensor = TextureConverter.ToTensor(tex, tex.width, tex.height, 3);

        // 2. Execute Model
        worker.Schedule(inputTensor);

        // 3. Fetch Raw Output Tensor [1, 84, 8400]
        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;
        // outputTensor.MakeReadable(); // Transfer data from GPU back to CPU array

        // 4. Process Raw Tensors to gather "Person" (Index 0) bounding boxes
        List<Detection> candidates = ParseYOLOv8Output(outputTensor);

        // 5. Apply Non-Maximum Suppression (NMS) to clear overlapping boxes
        List<Detection> finalDetections = ApplyNMS(candidates, iouThreshold);

        // 6. Draw results onto visual texture
        RenderDetections(tex, finalDetections);
        t = Time.realtimeSinceStartup - t;
        Debug.Log("inference: " + t + "\n");
    }

    private List<Detection> ParseYOLOv8Output(Tensor<float> output)
    {
        List<Detection> list = new List<Detection>();
        
        // YOLOv8 shapes: [batch=1, attributes=84, anchors=8400]
        int numAttributes = output.shape[1]; // 84
        int numAnchors = output.shape[2];    // 8400

        float[] data = output.DownloadToArray(); //.ToReadOnlyArray();

        for (int anchorIdx = 0; anchorIdx < numAnchors; anchorIdx++)
        {
            // Class index 0 is "person". 
            // In the flat layout array, rows are staggered by anchor layout:
            // Attribute indices: 0=cx, 1=cy, 2=w, 3=h, 4=person_score, 5=bike_score...
            float personScore = data[(4 * numAnchors) + anchorIdx];

            if (personScore >= confidenceThreshold)
            {
                // Fetch bounding box centers and dimensions (relative to 640x640 internal size)
                float cx = data[(0 * numAnchors) + anchorIdx] / width;
                float cy = data[(1 * numAnchors) + anchorIdx] / height;
                float w  = data[(2 * numAnchors) + anchorIdx] / width;
                float h  = data[(3 * numAnchors) + anchorIdx] / height;

                // Convert center-based box coordinates [cx, cy, w, h] to corner-based [xMin, yMin]
                float xMin = Mathf.Clamp01(cx - (w / 2f));
                float yMin = Mathf.Clamp01(cy - (h / 2f));

                Detection d = new Detection
                {
                    rect = new Rect(xMin, yMin, w, h),
                    score = personScore
                };
                list.Add(d);
            }
        }
        return list;
    }

    private List<Detection> ApplyNMS(List<Detection> boxes, float iouThresh)
    {
        // Sort items highest confidence scores first
        boxes.Sort((a, b) => b.score.CompareTo(a.score));

        List<Detection> isActive = new List<Detection>(boxes);
        List<Detection> result = new List<Detection>();

        while (isActive.Count > 0)
        {
            Detection current = isActive[0];
            result.Add(current);
            isActive.RemoveAt(0);

            for (int i = isActive.Count - 1; i >= 0; i--)
            {
                if (CalculateIoU(current.rect, isActive[i].rect) > iouThresh)
                {
                    isActive.RemoveAt(i);
                }
            }
        }
        return result;
    }

    private float CalculateIoU(Rect boxA, Rect boxB)
    {
        float intersectionArea = Mathf.Max(0, Mathf.Min(boxA.xMax, boxB.xMax) - Mathf.Max(boxA.xMin, boxB.xMin)) *
                                 Mathf.Max(0, Mathf.Min(boxA.yMax, boxB.yMax) - Mathf.Max(boxA.yMin, boxB.yMin));

        float totalArea = (boxA.width * boxA.height) + (boxB.width * boxB.height) - intersectionArea;
        return (totalArea <= 0) ? 0 : (intersectionArea / totalArea);
    }

    private void RenderDetections(Texture2D tex, List<Detection> detections)
    {
        // Clear frame by copying the raw initial source pixels
        Graphics.CopyTexture(tex, outputTexture);

        int texW = outputTexture.width;
        int texH = outputTexture.height;
        nmsVisualizer.ResetBBs();
        foreach (var det in detections)
        {
            // Map normalized 0-1 bounds cleanly to target resolution pixels
            // YOLO coordinates are inverted vertically relative to Unity Texture coordinates (0,0 is Bottom Left)
            int x = Mathf.RoundToInt(det.rect.x * texW);
            int y = Mathf.RoundToInt((1f - det.rect.yMax) * texH); 
            int width = Mathf.RoundToInt(det.rect.width * texW);
            int height = Mathf.RoundToInt(det.rect.height * texH);

            // game objects ???
            DrawBox(outputTexture, x, y, width, height, boxColor, boxThickness);
            BoundingBox bb = new BoundingBox();
            bb.rect.x = x;
            bb.rect.y = y;
            bb.rect.width = width;
            bb.rect.height = height;
            nmsVisualizer.DisplayNmsResultsAsQuadsOne(bb);
        }

        outputTexture.Apply(); // Commit pixel array modifications to GPU memory
    }

    private void DrawBox(Texture2D tex, int x, int y, int width, int height, Color color, int thickness)
    {
        Debug.Log("DrawBox: " + x.ToString("F0") + "," + y.ToString("F0") + "\n");
        for (int i = 0; i < thickness; i++)
        {
            DrawLine(tex, x, y + i, x + width, y + i, color);                     // Top Line
            DrawLine(tex, x, y + height - i, x + width, y + height - i, color); // Bottom Line
            DrawLine(tex, x + i, y, x + i, y + height, color);                     // Left Line
            DrawLine(tex, x + width - i, y, x + width - i, y + height, color); // Right Line
        }
    }

    private void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color color)
    {
        // Clamp to prevent edge out-of-bounds drawing crashes
        x0 = Mathf.Clamp(x0, 0, tex.width - 1);
        x1 = Mathf.Clamp(x1, 0, tex.width - 1);
        y0 = Mathf.Clamp(y0, 0, tex.height - 1);
        y1 = Mathf.Clamp(y1, 0, tex.height - 1);

        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;

        while (true)
        {
            tex.SetPixel(x0, y0, color);
            if (x0 == x1 && y0 == y1) break;
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    void OnDestroy()
    {
        // Critical cleanup to avoid massive memory leaks in Sentis
        worker?.Dispose();
    }
}
