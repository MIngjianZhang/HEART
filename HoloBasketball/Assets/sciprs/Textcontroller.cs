using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Textcontroller : MonoBehaviour {

    public Text counttext;
    public Text missiontext;
    public Text errortext;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        missiontext.text = "Finished! 30 of 30";
        counttext.text = "Congratulations! You scores 28 of 30.";
        errortext.text = "Connected!";
	}
}
