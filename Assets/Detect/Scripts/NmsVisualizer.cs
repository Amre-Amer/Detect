using System.Collections.Generic;
using UnityEngine;

public class NmsVisualizer : MonoBehaviour
{
    [Tooltip("Assign a simple unlit color material (e.g., transparent green boundary)")]
    public Material boundingBoxMaterial; 

    // Example structure matching your NMS outputs
    /// <summary>
    /// Spawns 3D Quads based on the filtered results from ApplyNMS
    /// </summary>
    public void ResetBBs()
    {
        // Optional: Clear existing bounding boxes from the previous frame
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }

    public void DisplayNmsResultsAsQuads(List<BoundingBox> filteredBoxes)
    {
        ResetBBs();

        foreach (var box in filteredBoxes)
        {
            DisplayNmsResultsAsQuadsOne(box);
        }
    }

    public void DisplayNmsResultsAsQuadsOne(BoundingBox box)
    {
        // 1. Calculate world positions from the coordinates
        Rect rect = new Rect
        {
            width = box.rect.xMax - box.rect.xMin,
            height = box.rect.yMax - box.rect.yMin,
            x = box.rect.xMin + (box.rect.width / 2f),
            y = box.rect.yMin + (box.rect.height / 2f)
        };

        // 2. Instantiate a primitive Unity Quad
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        quad.transform.SetParent(this.transform);
        
        // 3. Position and scale the Quad
        // Set Z to a slight offset forward from your canvas/image reference plane to avoid z-fighting
        quad.transform.position = new Vector3(rect.x, rect.y, 0f); 
        quad.transform.localScale = new Vector3(rect.width, rect.height, 1f);

        // 4. Apply a transparent shader or line material to see through the box
        if (boundingBoxMaterial != null)
        {
            quad.GetComponent<Renderer>().material = boundingBoxMaterial;
        }
    }
}

public struct BoundingBox
{
    public Rect rect;
    public float confidence;
}


// public struct BoundingBox {
//     public float xMin, yMin, xMax, yMax;
//     public float confidence;
// }


