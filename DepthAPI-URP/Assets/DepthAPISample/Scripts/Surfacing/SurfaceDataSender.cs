using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class SurfaceDataSender : MonoBehaviour
{
    public string ListenerAddress = "192.168.86.26";
    public int ListenerPort = 60523;
    public bool SendTestData = false;

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
        try
        {
            // Prefer a using declaration to ensure the instance is Disposed later.
            using TcpClient client = new TcpClient(ListenerAddress, ListenerPort);

            // Get a client stream for reading and writing.
            NetworkStream stream = client.GetStream();

            // Send the message to the connected TcpServer.
            stream.Write(dataToSend, 0, dataToSend.Length);

            Debug.LogWarning($"SENDER: Sending {dataToSend.Length} bytes of data...");

            // Receive the server response.

            // Buffer to store the response bytes.
            var response = new Byte[256];

            // Read the first batch of the TcpServer response bytes.
            Int32 bytes = stream.Read(response, 0, response.Length);
            var responseData = System.Text.Encoding.ASCII.GetString(response, 0, bytes);
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
    
    private void SendTestDataToListener()
    {
        // Translate the passed message into ASCII and store it as a Byte array.
        // var data = System.Text.Encoding.ASCII.GetBytes(TestDataToSend);
        var testSurfacesData = Surface.SurfacesSerializedData.CreateFromMesh(
            4,
            new Vector3[]
            {
                new Vector3(-1F, 0F, -1F), new Vector3(0F, 0F, -1F), new Vector3(0F, 0F, 0F), new Vector3(-1F, 0F, 0F),
                new Vector3(0F, 0F, -1F), new Vector3(1F, 0F, -1F), new Vector3(1F, 0F, 0F), new Vector3(0F, 0F, 0F),
                new Vector3(-1F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 1F), new Vector3(-1F, 0F, 1F),
                new Vector3(0F, 0F, 0F), new Vector3(1F, 0F, 0F), new Vector3(1F, 0F, 1F), new Vector3(0F, 0F, 1F),
            },
            new Vector3[]
            {
                Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up,
                Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up,
                Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up,
                Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.up,
            },
            new int[]
            {
                -1, 0, 0, -1, 
                0, 1, 1, 0, 
                -1, 0, 0, -1, 
                0, 1, 1, 0,
            },
            new int[]
            {
                -1, -1, 0, 0, 
                -1, -1, 0, 0, 
                0, 0, 1, 1, 
                0, 0, 1, 1,
            }
        );
        var testSerializedData = Surface.SurfacesSerializedData.Serialize(testSurfacesData);
        SendDataToListener(testSerializedData);
    }
}
