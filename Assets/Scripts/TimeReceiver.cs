using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using TMPro;

public class TimeReceiver : MonoBehaviour
{
    public TMP_Text timeText;

    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning = true;

    void Start()
    {
        udpClient = new UdpClient();
        udpClient.Connect("localhost", 9999);

        // Send an initial message to register the client on the server
        byte[] data = Encoding.ASCII.GetBytes("Hello Server");
        udpClient.Send(data, data.Length);

        receiveThread = new Thread(ReceiveData);
        receiveThread.Start();
    }

    void ReceiveData()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 9999);

        while (isRunning)
        {
            try
            {
                if (udpClient.Available > 0)
                {
                    byte[] data = udpClient.Receive(ref remoteEndPoint);
                    string message = Encoding.ASCII.GetString(data).Trim();
                    Debug.Log("Received: " + message);

                    // 在主线程更新UI
                    UnityMainThreadDispatcher.Instance().Enqueue(() => UpdateTimeText(message));
                }
                else
                {
                    Thread.Sleep(100); // Reduce CPU usage by sleeping briefly when no data is available
                }
            }
            catch (SocketException se)
            {
                if (isRunning)
                {
                    Debug.LogError("SocketException: " + se.Message);
                }
            }
            catch (Exception e)
            {
                if (isRunning)
                {
                    Debug.LogError("Error receiving UDP data: " + e.Message);
                }
            }
        }
    }

    void UpdateTimeText(string time)
    {
        if (timeText != null)
        {
            timeText.text = "Current Time: " + time;
        }
    }

    private void OnDestroy()
    {
        isRunning = false;
        if (udpClient != null)
        {
            udpClient.Close();
        }
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join();
        }
    }
}
