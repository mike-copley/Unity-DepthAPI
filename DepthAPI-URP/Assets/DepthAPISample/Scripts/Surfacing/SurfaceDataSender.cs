using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class SurfaceDataSender : MonoBehaviour
{
    public string ListenerAddress = "192.168.86.26";
    public int ListenerPort = 60523;
    public bool SendTestData = false;
    public string TestDataToSend = "Mary had a little lamb.";
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (SendTestData)
        {
            SendTestData = false;
            SendTestDataToListener();
        }
    }

    public void SendDataToListener(byte[] dataToSend)
    {
        
    }
    
    private void SendTestDataToListener()
    {
        try
        {
            // Prefer a using declaration to ensure the instance is Disposed later.
            using TcpClient client = new TcpClient(ListenerAddress, ListenerPort);

            // Translate the passed message into ASCII and store it as a Byte array.
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(TestDataToSend);

            // Get a client stream for reading and writing.
            NetworkStream stream = client.GetStream();

            // Send the message to the connected TcpServer.
            stream.Write(data, 0, data.Length);

            Debug.LogWarning($"SENDER: Sent: {TestDataToSend}");

            // Receive the server response.

            // Buffer to store the response bytes.
            data = new Byte[256];

            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Debug.LogWarning($"SENDER: Received: {responseData}");

            // Explicit close is not necessary since TcpClient.Dispose() will be
            // called automatically.
            // stream.Close();
            // client.Close();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
