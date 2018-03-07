using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testingshoot : MonoBehaviour {
    public Rigidbody prefab;
    public float speed = 12.0f;
    public float fireRate = 10.0F;
    private float nextFire = 0.0F;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.UpArrow) && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            ThrowNewBall();
            
        }
    }

    private void ThrowNewBall()
    {
        var ball = (Rigidbody)Instantiate(prefab, transform.position, transform.rotation);
        ball.velocity = (transform.forward + transform.up * 0.8f) * speed;
    }
}
