using System.Collections.Generic;
using UnityEngine;
using Unity.InferenceEngine;

public class YoloV8PersonDetector : MonoBehaviour
{
    public NmsVisualizer nmsVisualizer;
    [Header("Assets")]
    public ModelAsset modelAsset; // Your yolov8n.onnx file
    public Texture2D inputTexture;       // Texture to run inference on

    [Header("Thresholds")]
    [Range(0f, 1f)] public float confidenceThreshold = 0.5f;
    [Range(0f, 1f)] public float iouThreshold = 0.45f;

    private const int PersonClassId = 0; // COCO index 0 is always 'person'
    private Worker m_Worker;

    void Start()
    {
        // Load the model and create a worker using Sentis API
        Model model = ModelLoader.Load(modelAsset);
        // m_Worker = new Worker(model, BackendType.GPUCompute);
        m_Worker = new Worker(model, BackendType.CPU);
        InvokeRepeating(nameof(Inference), 1, 1);        
    }

    void Inference()
    {
        if (inputTexture != null)
        {
            float t = Time.realtimeSinceStartup;
            ExecuteInference();
            t = Time.realtimeSinceStartup - t;
            Debug.Log(Time.realtimeSinceStartup.ToString("F2") + " elapsed: " + t + "\n");
        }
    }

    public void ExecuteInference()
    {
        // 1. Transform input texture to a tensor matching YOLOv8 format [1, 3, 640, 640]
        using Tensor<float> inputTensor = TextureConverter.ToTensor(inputTexture, 640, 640, 3);
        
        m_Worker.Schedule(inputTensor);

        // 2. Fetch the output tensor
        Tensor<float> outputTensor = m_Worker.PeekOutput() as Tensor<float>;
        
        // Make the tensor readable on CPU
        outputTensor.ReadbackAndClone(); 

        // Shape properties: rows = 84, columns = 8400
        int numClasses = outputTensor.shape[1] - 4; // 84 - 4 = 80
        int numBoxes = outputTensor.shape[2];       // 8400 candidate locations

        List<BoundingBox> candidateBoxes = new List<BoundingBox>();

        // 3. Process the tensor, isolating only index 4 (the person class confidence)
        for (int i = 0; i < numBoxes; i++)
        {
            // Row index 4 corresponds to Class 0 (Person)
            float personConfidence = outputTensor[0, 4, i];

            if (personConfidence >= confidenceThreshold)
            {
                // Grab bounding box dimensions (Rows 0, 1, 2, 3)
                float cx = outputTensor[0, 0, i];
                float cy = outputTensor[0, 1, i];
                float w  = outputTensor[0, 2, i];
                float h  = outputTensor[0, 3, i];

                // Convert from center coordinates to corner coordinates
                float xmin = cx - (w / 2f);
                float ymin = cy - (h / 2f);

                candidateBoxes.Add(new BoundingBox
                {
                    rect = new Rect(xmin, ymin, w, h),
                    confidence = personConfidence
                });
            }
        }

        // 4. Clean up overlaps using Non-Maximum Suppression (NMS)
        List<BoundingBox> finalPeopleBoxes = ApplyNMS(candidateBoxes, iouThreshold);

        Debug.Log("people: " + finalPeopleBoxes.Count + "\n");

        nmsVisualizer.DisplayNmsResultsAsQuads(finalPeopleBoxes);
    }

    private List<BoundingBox> ApplyNMS(List<BoundingBox> boxes, float iouThresh)
    {
        // Sort boxes descending by confidence score
        boxes.Sort((a, b) => b.confidence.CompareTo(a.confidence));
        
        List<BoundingBox> keptBoxes = new List<BoundingBox>();
        bool[] isFiltered = new bool[boxes.Count];

        for (int i = 0; i < boxes.Count; i++)
        {
            if (isFiltered[i]) continue;

            keptBoxes.Add(boxes[i]);

            for (int j = i + 1; j < boxes.Count; j++)
            {
                if (isFiltered[j]) continue;

                if (CalculateIoU(boxes[i].rect, boxes[j].rect) > iouThresh)
                {
                    isFiltered[j] = true; // Reject overlapping candidate box
                }
            }
        }
        return keptBoxes;
    }

    private float CalculateIoU(Rect boxA, Rect boxB)
    {
        float x1 = Mathf.Max(boxA.xMin, boxB.xMin);
        float y1 = Mathf.Max(boxA.yMin, boxB.yMin);
        float x2 = Mathf.Min(boxA.xMax, boxB.xMax);
        float y2 = Mathf.Min(boxA.yMax, boxB.yMax);

        float intersectionArea = Mathf.Max(0, x2 - x1) * Mathf.Max(0, y2 - y1);
        float totalArea = (boxA.width * boxA.height) + (boxB.width * boxB.height) - intersectionArea;

        return totalArea <= 0 ? 0 : intersectionArea / totalArea;
    }

    private void OnDestroy()
    {
        m_Worker?.Dispose();
    }

    void OApplicationQuit()
    {
        m_Worker?.Dispose();
    }
}
