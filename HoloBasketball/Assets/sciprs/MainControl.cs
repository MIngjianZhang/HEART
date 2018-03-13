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

public class MainControl : MonoBehaviour
{
    public Rigidbody prefab;
    private float speed = 12.0f;
    public Text counttext;
    public Text missiontext;
    public Text errortext;
    public GameObject hand;
    public Slider Progressslide;
    private float fireRate = 10.0F;
    private float nextFire = 0.0F;
    private string request = "Request not received yet";
    //private float timecount = 0.0f;
    private double EMGMaximum = 0.0;
    private double EMGtemp = 0.0;
    private double Success = 0.0;
    //private double TutorialEMG = 0.0;
    private double startgametime = 0.0;
    private bool Tutorialed = false;



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

    void Update()
    {
        errortext.text = request + " " + hand.transform.localEulerAngles.y;
        float a = 0;
        missiontext.text = "Request not received yet";
        /*if (!request.Equals("Request not received yet")) {
            if (Tutorialed == false) {
                missiontext.text = "Keep using your maximum strength. Tutorial Part";
                double tutorial = Double.Parse(request);
                if (Time.deltaTime > 1 && Time.deltaTime < 5)
                {
                    errortext.text = " " + Time.deltaTime + "  : Tutorial Part";
                    counttext.text = (5 - Time.deltaTime) + " seconds left in tutorial part";
                    if (EMGMaximum < tutorial)
                    {
                        EMGMaximum = tutorial;
                    }
                }
                Tutorialed = true;
                startgametime = Time.deltaTime;
            }
            if (Tutorialed == true) {
                {
                    try
                    {
                        double data = Double.Parse(request);

                        if ((Time.time - startgametime) % 10 < 2 && ((Time.time - startgametime)) < 300)
                        {
                            missiontext.text = "Prepare to shoot in 2 seconds.";
                            counttext.text = "Count down " + (2 - (Time.time - startgametime) % 10);
                        }
                        if (((Time.time - startgametime) % 10) >= 2 && ((Time.time - startgametime) % 10) < 5 && ((Time.time - startgametime)) < 300)
                        {
                            missiontext.text = "Use your MAXIMUM strength now~";
                            counttext.text = "Count down " + (5 - (Time.time - startgametime) % 10);
                            Progressslide.value += ToSingle(0.03);
                            if (data > EMGtemp)
                            {
                                EMGtemp = data;
                            }
                            if (data > EMGMaximum)
                            {
                                EMGMaximum = data;
                            }
                        }
                        if (((Time.time - startgametime) % 10) >= 5 && ((Time.time - startgametime) % 10) < 10 && ((Time.time - startgametime)) < 300 && ((Time.time - startgametime)) > nextFire)
                        {
                            missiontext.text = "Shooting";
                            counttext.text = "Count down " + (10 - (Time.time - startgametime) % 10);
                            nextFire = ToSingle(Time.time - startgametime) + fireRate;
                            if (EMGtemp > EMGMaximum * 0.8)
                            {
                                ThrowNewBallSuccess();
                                Success = Success + 1;
                            }
                            if (EMGtemp <= EMGMaximum * 0.8)
                            {
                                ThrowNewBallFail();
                            }

                            if ((hand.transform.localEulerAngles.y <= 50 && hand.transform.localEulerAngles.y >= 0) ||
                                (hand.transform.localEulerAngles.y >= 300 && hand.transform.localEulerAngles.y <= 360))
                            {
                                hand.transform.Rotate(0.0f, a + (float)EMGtemp, 0.0f);
                            }
                            else
                            {
                                if (hand.transform.localEulerAngles.y > 50 && hand.transform.localEulerAngles.y < 180)
                                {
                                    hand.transform.localEulerAngles = new Vector3(

                                        0.0f,
                                        50f,
                                        0.0f
                                    );
                                }

                                if (hand.transform.localEulerAngles.y < 300 && hand.transform.localEulerAngles.y > 180)
                                {
                                    hand.transform.localEulerAngles = new Vector3(
                                        0.0f,
                                        300f,
                                        0.0f
                                        );
                                }
                            }
                        }

                        else if ((Time.time - startgametime) >= 300)
                        {
                            missiontext.text = "CONGRATULATIONS! You shot " + Success + " out of 30 balls!";
                            counttext.text = "Thank you for playing";
                            Progressslide.value = 1;
                        }
                    }

                    catch (Exception e) {
                        errortext.text = "Error: " + e.ToString();
                    }
                }


                a = hand.transform.localEulerAngles.y;
            }
        }
        */
    }

    private void ThrowNewBallSuccess()
    {
        var ball = (Rigidbody)Instantiate(prefab, transform.position, transform.rotation);
        ball.velocity = (transform.forward + transform.up * 0.8f) * speed;
    }

    private void ThrowNewBallFail()
    {
        var ball = (Rigidbody)Instantiate(prefab, transform.position, transform.rotation);
        ball.velocity = (transform.forward + transform.up * 0.4f) * speed;
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
