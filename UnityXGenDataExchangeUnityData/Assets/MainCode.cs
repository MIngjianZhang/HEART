using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

public class MainCode : MonoBehaviour 
{
	public GUIText ScalarDisp;

   static MainCode pThis;
   public static MainCode Instance { get { return pThis; } }

   internal UDPDiscovery udp = new UDPDiscovery();
   internal DataReaderForUnity.DataReader readme = new DataReaderForUnity.DataReader();
   internal DataReaderForUnity.DataValues dataValues = new DataReaderForUnity.DataValues();

   internal MMServer mmServer = new MMServer();
   internal Dictionary<string, GameObject> mmObjects = new Dictionary<string, GameObject>();

   public GameObject cube;

   public MainCode()
   {
      pThis = this;
   }
   

   // Use this for initialization
   void Start ()
   {

      // Just a trivial notifier, does nothing
      mmServer.ConnectionOpened += MMGotNewClient;

      // The MMServer class runs in a seperate thread, which prevents Unity from handling object creation and manipulation from there.
      // Each AddVariableChangedNotification callback is instead scheduled to run on the main thread, which is handled by the Update
      // and FixedUpdate calls to mmServer.Pulse()

      // Some test variables for the treadmill and scalar data
      mmServer.AddVariableChangedNotification("Scalar", UpdateScalar);

      // These are from the demoDataExchangeWithTMM.py file. It only does the CubePosition value which sets the cube pos and returns the position back
      mmServer.AddVariableChangedNotification("CubePosition", UpdateCubePosition);

	  // Additional data types for the Cube to demonstrate Scalar Orientation data types
	  mmServer.AddVariableChangedNotification("CubeOri", UpdateCubeOri);

      
		mmServer.Start();
   }


   void OnApplicationQuit()
   {
      Debug.Log("OnApplicationQuit");
      udp.ServerDiscovered -= DataStreamServerDiscovered;
      udp.StopDiscovery();
      readme.StopReader();
      mmServer.Stop();
   }

   // Update is called once per frame and is tied to the framerate and the complexity of the scene
   void Update ()
   {
      mmServer.Pulse();

   }

   // FixedUpdate is called multiple times per frame, but can be affected by the physics engine.
   void FixedUpdate()
   {
      mmServer.Pulse();
   }

   internal void DataStreamServerDiscovered(string serverIP, string serverName, string webDataUrl, int webServerPort, int dataStreamServerPort, string dataStreamCmd)
   {
      string s = "Discovered server named " + serverName + " on ip address " + serverIP + " with server ports " + webServerPort + " for web and " + dataStreamServerPort + " for data";

      readme.SetServerAndPorts(serverIP, webDataUrl, dataStreamServerPort, dataStreamCmd);

//    StartCoroutine(readme.startViaWebDisco(webDataUrl));
      StartCoroutine(readme.StartViaDirect(serverIP, dataStreamServerPort, dataStreamCmd));
   }

   internal void MMGotNewClient(int connectionID,string ipadddress)
   {
      Debug.Log("Got a new client connection! " + ipadddress);
   }
		

	internal void UpdateScalar(string varname, object data)
	{
		mmServer.SetFrameVariable("ReturnedScalar", data);
		double ReturnedScalar = (double)data;
		/// Debug.Log("Scalar updated to " + Scalar);

		double Scalar = (double)data;
		if (ScalarDisp != null)
			ScalarDisp.text = Scalar.ToString("00.000");
	}

   internal void UpdateCubePosition(string varname,object data)
   {
      mmServer.SetFrameVariable("ReturnedCubePosition", data);
      Vector3 v3 = (Vector3)data;
      if (cube != null)
         cube.transform.localPosition = v3;
   }


	internal void UpdateCubeOri(string varname,object data)
	{
		mmServer.SetFrameVariable("ReturnedCubeOri", data);
			Quaternion qt = (Quaternion)data;
			if (cube != null)
				cube.transform.localRotation = qt;
	}

}
