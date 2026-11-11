using System.Collections.Generic;
using UnityEngine;

public class NmsVisualizer : MonoBehaviour
{
    [Tooltip("Assign a simple unlit color material (e.g., transparent green boundary)")]
    public Material boundingBoxMaterial; 

    public void ResetBBs()
    {
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
        Rect rect = new Rect
        {
            width = box.rect.xMax - box.rect.xMin,
            height = box.rect.yMax - box.rect.yMin,
            x = box.rect.xMin + (box.rect.width / 2f),
            y = box.rect.yMin + (box.rect.height / 2f)
        };

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        quad.transform.SetParent(this.transform);
        
        quad.transform.position = new Vector3(rect.x, rect.y, 0f); 
        quad.transform.localScale = new Vector3(rect.width, rect.height, 10f);

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

