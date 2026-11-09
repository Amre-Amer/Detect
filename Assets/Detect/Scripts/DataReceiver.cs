using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class DataReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread receiveThread;
    private int port = 5005;
    
    // Thread safe container for incoming data
    private string lastReceivedMessage = "";
    private bool newDataAvailable = false;
    private readonly object lockObject = new object();

    [System.Serializable]
    public class DetectionData
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public int frame_width;
        public int frame_height;
    }

    void Start()
    {
        // Start UDP thread
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void Update()
    {
        // Process message on the Unity Main Thread
        if (newDataAvailable)
        {
            string jsonString = "";
            lock (lockObject)
            {
                jsonString = lastReceivedMessage;
                newDataAvailable = false;
            }

            try
            {
                DetectionData data = JsonUtility.FromJson<DetectionData>(jsonString);
                Debug.Log($"Received from Python -> X: {data.x}, Y: {data.y}");
                
                // Do something with the coordinates here (e.g., move UI or 3D elements)
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing JSON: {e.Message}");
            }
        }
    }

    private void ReceiveData()
    {
        udpClient = new UdpClient(port);
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, port);

        while (true)
        {
            try
            {
                byte[] dataBytes = udpClient.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(dataBytes);

                lock (lockObject)
                {
                    lastReceivedMessage = text;
                    newDataAvailable = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"UDP Socket Exception: {e.Message}");
                break;
            }
        }
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}
