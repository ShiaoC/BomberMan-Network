using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class network : MonoBehaviour
{
    public TMP_InputField ipInputField;
    public TMP_InputField portInputField;
    public TMP_Text playerCountText;
    public TMP_Text readyCountText;
    public Button connectButton;
    public Button readyButton;
    public Button cancelReadyButton;

    public GameObject disconnectCheck;
    public GameObject connectSuccessCheck;
    public GameObject LoadGame;
    public GameObject MenuPanel;

    public static int playerId = -1;
    public static int PlayerCount = 0;
    private TcpClient tcpClient;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024];
    private bool isRunning = true;
    private Thread receiveThread;

    public static network instance;

    private void Awake()
    {
        UnityMainThreadDispatcher.EnsureExists();
        instance = this;
        DontDestroyOnLoad(gameObject); // Prevent the object from being destroyed on scene load
    }

    private void Start()
    {
        connectButton.onClick.AddListener(ConnectToServer);
        readyButton.onClick.AddListener(OnReadyButtonClicked);
        cancelReadyButton.onClick.AddListener(OnCancelReadyButtonClicked);
    }

    private void ConnectToServer()
    {
        try
        {
            string ipAddress = ipInputField.text;
            int port = int.Parse(portInputField.text);

            tcpClient = new TcpClient(ipAddress, port);
            stream = tcpClient.GetStream();

            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
            if (int.TryParse(response, out playerId))
            {
                Debug.Log("Connected to server with player ID: " + playerId);
                disconnectCheck.SetActive(false);
                connectSuccessCheck.SetActive(true);
            }
            else
            {
                Debug.LogError("Error parsing player ID from server response: " + response);
            }

            receiveThread = new Thread(ReceiveData);
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Error connecting to server: " + e.Message);
        }
    }

    public void SendData(string message)
    {
        try
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending data: " + e.Message);
        }
    }

    private void ReceiveData()
    {
        while (isRunning)
        {
            try
            {
                if (!tcpClient.Connected)
                {
                    break;
                }

                if (stream.DataAvailable)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) continue;
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                    Debug.Log("Received: " + message);
                    ProcessReceivedData(message);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception e)
            {
                if (isRunning)
                {
                    Debug.LogError("Error receiving data: " + e.Message);
                }
                break;
            }
        }
    }

    private void ProcessReceivedData(string message)
    {
        string[] messages = message.Split('\n');

        foreach (string msg in messages)
        {
            if (string.IsNullOrWhiteSpace(msg))
                continue;

            if (msg.StartsWith("PlayerCount/"))
            {
                string[] parts = msg.Split('/');
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[1], out int playerCount))
                    {
                        PlayerCount = playerCount;
                        print(PlayerCount);
                        UnityMainThreadDispatcher.Instance().Enqueue(() => UpdatePlayerCount(playerCount));
                    }
                }
            }
            else if (msg.StartsWith("ReadyCount/"))
            {
                string[] parts = msg.Split('/');
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[1], out int readyCount))
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => UpdateReadyCount(readyCount));
                    }
                }
            }
            else if (msg == "StartGame")
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    StartGame();
                    LoadGame.SetActive(true);
                    MenuPanel.SetActive(false);
                });
            }
            else if (msg.StartsWith("Remove/"))
            {
                string[] parts = msg.Split('/');
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[1], out int objectId))
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => RemoveObject(objectId));
                    }
                }
            }
            else if (msg.StartsWith("Position/"))
            {
                // Handle position update from the server
            }
            else if (msg.StartsWith("Bomb/"))
            {
                // Handle bomb update from the server
            }
            else if (msg == "EndGame")
            {
                UnityMainThreadDispatcher.Instance().Enqueue(EndGame);
            }
            else
            {
                Debug.LogError("Received malformed message: " + msg);
            }
        }
    }

    private void UpdatePlayerCount(int count)
    {
        playerCountText.text = count.ToString();
        print("players count" + count.ToString());
    }

    private void StartGame()
    {
        Debug.Log("All players are ready. Starting the game!");
        MyPlayerPrefs.SetPlayers(PlayerCount); 
        
    }

    public void EndGame()
    {
        Debug.Log("Game over!");
        string message = playerId + "/EndGame";
        SendData(message);
    }

    private void SpawnObject(int objectId)
    {
        // Implementation for spawning an object with the given object ID.
    }

    private void UpdateReadyCount(int count)
    {
        readyCountText.text = $"{count}";
    }

    public void RemoveObject(int objectId)
    {
        // Implementation for removing an object with the given object ID.
    }

    private void OnReadyButtonClicked()
    {
        string message = playerId + "/Ready";
        SendData(message);
    }

    private void OnCancelReadyButtonClicked()
    {
        string message = playerId + "/CancelReady";
        SendData(message);
    }

    public void OnDestroy()
    {
        isRunning = false;
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join();
        }
        if (stream != null)
        {
            stream.Close();
        }
        if (tcpClient != null)
        {
            tcpClient.Close();
        }
    }
}
