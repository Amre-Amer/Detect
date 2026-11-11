using System.Collections.Generic;
using UnityEngine;

public class OutputsMgr : MonoBehaviour
{
    public GameObject markerPrefab;
    public List<GameObject> markers = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckMarkers();
    }

    void CheckMarkers()
    {
        
    }
}
