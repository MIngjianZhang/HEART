using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;

#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Sockets;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Networking.Connectivity;
#endif

public class Textcontroller : MonoBehaviour {

    public Text counttext;
    public Text missiontext;
    public Text errortext;
    public Slider progressbar;
    private string request = "Request not received yet";
    private double meow;

#if WINDOWS_UWP
    Windows.Networking.Sockets.StreamSocketListener socketListener;
#endif

    // Use this for initialization
#if !WINDOWS_UWP
    void Start()
    {
#endif
#if WINDOWS_UWP
    async void Start()
    {
        try
        {
            socketListener = new Windows.Networking.Sockets.StreamSocketListener();
            socketListener.ConnectionReceived += SocketListener_ConnectionReceived;
            await socketListener.BindServiceNameAsync("1337");
        }
        catch (Exception e)
        {
            counttext.text = "Error: " + e.ToString();
        }
#endif
    }


    // Update is called once per frame
    void Update () {
        missiontext.text = "Prepare to shoot! 20 of 30";
        counttext.text = "Count down: 2";
        errortext.text = "Connected!";
        progressbar.value = ToSingle(0.66);
        if (!request.Equals("Request not received yet"))
        {
            errortext.text = request;
        }
	}

    public static float ToSingle(double value)
    {
        return (float)value;
    }
#if WINDOWS_UWP
    private async void SocketListener_ConnectionReceived(Windows.Networking.Sockets.StreamSocketListener sender, 
                                                            Windows.Networking.Sockets.StreamSocketListenerConnectionReceivedEventArgs args)
    {
        //Read line from the remote client.
        Stream inStream = args.Socket.InputStream.AsStreamForRead();
        StreamReader reader = new StreamReader(inStream);
        request = await reader.ReadLineAsync();
    }
#endif
}
