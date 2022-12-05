using UnityEngine;
using System;
using System.Collections.Generic;
using SocketIOClient;
using System.Reactive.Subjects;
using System.Linq;

public class DoplConnect : MonoBehaviour
{
    [Serializable]
    public class Frame
    {
        public CatheterData[] catheterdataList;
    }

    [Serializable]
    public class CatheterData
    {
        public uint sensorid;
        public Coordinates coordinates;

        public CatheterData(CatheterData data)
        {
            sensorid = data.sensorid;
            coordinates = new Coordinates
            {
                position = new Position
                {
                    x = data.coordinates.position.x,
                    y = data.coordinates.position.y,
                    z = data.coordinates.position.z,
                },

                rotation = new Rotation
                {
                    x = data.coordinates.rotation.x,
                    y = data.coordinates.rotation.y,
                    z = data.coordinates.rotation.z,
                    w = data.coordinates.rotation.w,
                },
            };
        }
    }

    [Serializable]
    public class Coordinates
    {
        public Position position;
        public Rotation rotation;
    }

    [Serializable]
    public class Position
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class Rotation
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }

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
            var element = data.GetValue();
            var json = element.GetRawText();
            var frame = JsonUtility.FromJson<Frame>(json);
            
            if (frame.catheterdataList != null && frame.catheterdataList.Length > 0)
            {
                OnCatheterDataEvent?.Invoke(frame.catheterdataList);
            }
        });
    }

    private async void Connect()
    {
        Debug.Log("dopl connecting");

        await _socket.ConnectAsync();
    }
}
