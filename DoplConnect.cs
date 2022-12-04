using UnityEngine;
using System;
using System.Collections.Generic;
using SocketIOClient;
using System.Reactive.Subjects;
using DoplTechnologies.Protos;
using System.Linq;

public class DoplConnect : MonoBehaviour
{
    [Tooltip("Path to a file containing the url of the dopl connect socket server")]
    public string FileWithSocketUrl = "https://plutovr.s3.us-west-2.amazonaws.com/ryan/ngrok.txt";

    public event Action<CatheterData[]> OnCatheterDataEvent;

    private SocketIOUnity _socket;
    private Subject<object> _test;

    private void Awake()
    {
        using (var wc = new System.Net.WebClient())
        {
            string socketUrl = wc.DownloadString(FileWithSocketUrl);
            Debug.Log("Connecting to dopl socket: " + socketUrl);

            _socket = new SocketIOUnity(socketUrl, new SocketIOOptions
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
            });
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        RegisterSocketEvents();
        Connect();
    }

    private void OnDestroy()
    {
        _socket.Disconnect();
    }

    private void RegisterSocketEvents()
    {
        Debug.Log("Registering socket events!");
        _socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Connected to dopl socket");
        };

        _socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("Disconnected from dopl socket");
        };

        _socket.OnError += (sender, e) =>
        {
            Debug.Log("Error connecting to dopl socket");
        };

        _socket.On("onframe", (data) =>
        {
            var frame = data.GetValue<Frame>();
            
            if (frame.CatheterData != null && frame.CatheterData.Count > 0)
            {
                OnCatheterDataEvent?.Invoke(frame.CatheterData.ToArray());
            }
        });
    }

    private async void Connect()
    {
        Debug.Log("dopl connecting");

        await _socket.ConnectAsync();
    }
}
