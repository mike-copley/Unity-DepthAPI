using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Oculus.Interaction;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class SurfaceDataListener : MonoBehaviour
{
    public bool ListenForSurfaceData = false;
    public String AddressOverride = "192.168.86.26";
    public int PortOverride = 60523;

    public UnityEvent SurfaceDataReceived;
    public SurfaceAssetCreator SurfaceAssetCreator;
    
    public byte[] SurfaceDataReceivedEventData { get; private set; }

    // NOTE: this must be thread safe since it is populated by the listening thread
    // and then invokes events from the main thread
    private Queue<byte[]> queuedReceivedData = new Queue<byte[]>();
    
    private bool isListeningForSurfaceData = false;
    
    private static SurfaceDataListener instance = null;

    // 1 MB temp buffer for reading data
    private static byte[] tempBuffer = new byte[1024 * 1024];
    
    async void Listen()
    {
        TcpListener server = null;
        
        try
        {
            var hostName = Dns.GetHostName();
            Debug.LogWarning($"LISTENER: host name = {hostName}");

            // var he = Dns.GetHostEntry(hostName);
            // Debug.LogWarning($"LISTENER: num host entries = {he.AddressList.Length}");
            // foreach (var ip in he.AddressList)
            // {
            //     Debug.LogWarning($"LISTENER: ({hostName}) Address type = {ip.AddressFamily.ToString()}, is {ip}");
            // }
            
            var hostEntry = Dns.GetHostEntry("localhost");
            foreach (var ip in hostEntry.AddressList)
            {
                Debug.LogWarning($"LISTENER: (localhost) Address type = {ip.AddressFamily.ToString()}, is {ip}");
            }
            
            server = new TcpListener(
                string.IsNullOrEmpty(AddressOverride) ? 
                hostEntry.AddressList[0] : IPAddress.Parse(AddressOverride), 
                PortOverride);

            // Start listening for client requests.
            server.Start();

            Debug.LogWarning("LISTENER: I am listening for connections on " +
                             IPAddress.Parse(((IPEndPoint)server.LocalEndpoint).Address.ToString()) +
                             " on port number " + ((IPEndPoint)server.LocalEndpoint).Port.ToString());

            // Buffer for reading data
            Byte[] bytes = tempBuffer;
            String data = null;

            Debug.LogWarning("LISTENER: Waiting for a connection... ");
            
            // Enter the listening loop.
            while (ListenForSurfaceData 
                   // NOTE: this is needed if using this in "Play mode" to handle
                   // terminating the listen when the instance is destroyed
                   // && instance != null
                   )
            {
                if (!server.Pending())
                {
                    await Task.Delay(1000);
                    continue;
                }
                
                // Perform a blocking call to accept requests.
                // You could also use server.AcceptSocket() here.
                using var client = server.AcceptTcpClient();
                Debug.LogWarning("LISTENER: Connected!");

                data = null;

                // Get a stream object for reading and writing
                var stream = client.GetStream();

                var bytesRead = 0;

                // Loop to receive all the data sent by the client.
                while((bytesRead = stream.Read(bytes, 0, bytes.Length))!=0)
                {
                    // Translate data bytes to a ASCII string.
                    // data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    // Debug.LogWarning($"LISTENER: Received: {data}");
                    
                    var surfaceDataReceived = new byte[bytesRead];
                    for (var index = 0; index < bytesRead; index++)
                        surfaceDataReceived[index] = bytes[index];

                    lock (queuedReceivedData)
                    {
                        queuedReceivedData.Enqueue(surfaceDataReceived);
                    }
                    
                    // Process the data sent by the client.
                    data = "ACK";//data.ToUpper();

                    var msg = System.Text.Encoding.ASCII.GetBytes(data);

                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    Debug.LogWarning($"LISTENER: Sent: {data}");
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            server?.Stop();
            Debug.LogWarning("LISTENER: I am NOT listening for connections.");
        }

        ListenForSurfaceData = false;
        isListeningForSurfaceData = false;
        Debug.LogWarning($"LISTENER: ListenForSurfaceData = {ListenForSurfaceData}");
        Debug.LogWarning($"LISTENER: isListeningForSurfaceData = {isListeningForSurfaceData}");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isListeningForSurfaceData && ListenForSurfaceData)
        {
            isListeningForSurfaceData = true;
            Task.Run(Listen);
        }
        
        lock(queuedReceivedData)
        {
            if (queuedReceivedData.Count > 0)
            {
                SurfaceDataReceivedEventData = queuedReceivedData.Dequeue();
                // SurfaceDataReceived.Invoke();
                SurfaceAssetCreator.HandleSurfaceDataReceived();
            }
        }
    }
}
