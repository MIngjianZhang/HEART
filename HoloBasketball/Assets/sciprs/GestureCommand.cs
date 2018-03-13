using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GestureCommand : MonoBehaviour {

    private UnityEngine.XR.WSA.Input.GestureRecognizer recognizer;
    public Rigidbody prefab;
    public float speed = 12.0f;
    public float speedofhand = 4.0f;
    public Text counttext;
    public Text missiontext;
    public Text errortext;
    public Slider progressbar;
    public GameObject hand;
    public int a = 0;   

    // Use this for initialization
    void Start () {
        recognizer = new UnityEngine.XR.WSA.Input.GestureRecognizer();
        recognizer.SetRecognizableGestures(UnityEngine.XR.WSA.Input.GestureSettings.Tap); 
        recognizer.TappedEvent += Recognizer_TappedEvent;
        recognizer.StartCapturingGestures();
        throwNewBall();
        //hand.transform.Rotate(0, -Time.deltaTime * speedofhand,0);
        //hand.transform.Rotate(0.0f, 40.0f, 0.0f);
    }

    private void Recognizer_TappedEvent(UnityEngine.XR.WSA.Input.InteractionSourceKind source, int tapCount, Ray headRay)
    {
        if(a == 0)
        {
            missiontext.text = "Testing your strength";
            counttext.text = "Count down : 3";
        }
        else if (a == 1)
        {
            missiontext.text = "Testing your strength";
            counttext.text = "Recorded!";
        }
        else if (a == 2)
        {
            missiontext.text = "Prepare to shoot! 1 of 3";
            counttext.text = "Count down : 2";
            
        }
        else if (a == 3)
        {
            missiontext.text = "Use your strength now.";
            counttext.text = "Count down : 3";
        }
        else if (a == 4)
        {
            missiontext.text = "Shooting now.";
            counttext.text = "Count down : 5";
            hand.transform.Rotate(0.0f, -40.0f, 0.0f);
            throwNewBall();
            
            progressbar.value = ToSingle(0.33);
        }
        else if (a == 5)
        {
            missiontext.text = "Prepare to shoot! 2 of 3";
            counttext.text = "Count down : 2";
            hand.transform.Rotate(0.0f, 40.0f, 0.0f);

        }
        else if (a == 6)
        {
            missiontext.text = "Use your strength now.";
            counttext.text = "Count down : 3";
        }
        else if (a == 7)
        {
            missiontext.text = "Shooting now.";
            counttext.text = "Count down : 5";
            hand.transform.Rotate(0.0f, -40.0f, 0.0f);
            ThrowNewBallFail();
            progressbar.value = ToSingle(0.66);
        }
        else if (a == 8)
        {
            missiontext.text = "Prepare to shoot! 3 of 3";
            counttext.text = "Count down : 2";
            hand.transform.Rotate(0.0f, 40.0f, 0.0f);

        }
        else if (a == 9)
        {
            missiontext.text = "Use your strength now.";
            counttext.text = "Count down : 3";
        }
        else if (a == 10)
        {
            missiontext.text = "Shooting now.";
            counttext.text = "Count down : 5";
            hand.transform.Rotate(0.0f, -40.0f, 0.0f);
            throwNewBall();
            progressbar.value = ToSingle(1.0);
        }
        else
        {
            missiontext.text = "Congratulations!";
            hand.transform.Rotate(0.0f, 40.0f, 0.0f);
            counttext.text = "You have scored 2 of 3";
        }
        a = a + 1;
    }

    public static float ToSingle(double value)
    {
        return (float)value;
    }
    private void throwNewBall()
    {
        var ball = (Rigidbody)Instantiate(prefab, transform.position, transform.rotation);
        ball.velocity = (transform.forward + transform.up * 0.8f) * speed;
    }

    private void ThrowNewBallFail()
    {
        var ball = (Rigidbody)Instantiate(prefab, transform.position, transform.rotation);
        ball.velocity = (transform.forward + transform.up * 0.4f) * speed;
    }

    private void OnDestroy()
    {
        if (recognizer != null)
        {
            recognizer.TappedEvent -= Recognizer_TappedEvent;
        }
    }
}
