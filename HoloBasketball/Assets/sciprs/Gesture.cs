using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gesture : MonoBehaviour {

    private UnityEngine.XR.WSA.Input.GestureRecognizer recognizer;
    public Rigidbody prefab;
    public float speed = 10.0f;

	// Use this for initialization
	void Start () {
        recognizer = new UnityEngine.XR.WSA.Input.GestureRecognizer();
        recognizer.SetRecognizableGestures(UnityEngine.XR.WSA.Input.GestureSettings.Tap); 
        recognizer.TappedEvent += Recognizer_TappedEvent;
        recognizer.StartCapturingGestures();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void Recognizer_TappedEvent(UnityEngine.XR.WSA.Input.InteractionSourceKind source, int tapCount, Ray headRay)
    {
        throwNewBall();
    }

    private void throwNewBall()
    {
        var ball = (Rigidbody)Instantiate(prefab, transform.position, transform.rotation);
        ball.velocity = (transform.forward + transform.up * 0.8f) * speed;
    }

    private void OnDestroy()
    {
        if (recognizer != null)
        {
            recognizer.TappedEvent -= Recognizer_TappedEvent;
        }
    }
}
