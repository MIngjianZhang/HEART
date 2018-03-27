using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;


namespace DataReaderForUnity
{
   public class DataValues
   {
      public float copXvalue = 0;
      public float copYvalue = 0;
      public float cogXvalue = 0;
      public float cogYvalue = 0;
      public float fzValue = 0;
      public float mxValue = 0;
      public float myValue = 0;
      public float velocityl = 0;
      public float velocityr = 0;
      public long timestamp = 0;
   }

   public class DataReader
   {

      public delegate void ReaderConnectedHandler(string serverIP, int port);
      public event ReaderConnectedHandler ReaderConnected;

      public delegate void DataReceivedHandler(string serverIP, int port,DataValues data);
      public event DataReceivedHandler DataReceived;

      ///////////////////////////////////////////////////

      public string dataStreamDiscoveryUrl = "http://localhost:61710/$datastream";

      ///////////////////////////////////////////////////

      internal DataValues _dataValues = new DataValues();
      internal object _locker = new object();

      internal int numChans = 0;
      internal int port = 81;
      internal string channelsToGetCommand = "";
      internal string getDataCommand = "";
      internal string strmhost = "localhost";

      internal byte[] buffer = null;
      internal int[] headerBlock = null;
      internal float[] data = null;
      internal Socket socket = null;
      internal bool exitReader = false;

      ///////////////////////////////////////////////////


      public void SetServerAndPorts(string serverIP, string webDataUrl, int streamPort, string streamCmd)
      {
         port = streamPort;
         strmhost = serverIP;
         getDataCommand = streamCmd;
         dataStreamDiscoveryUrl = webDataUrl;
      }

      public IEnumerator StartViaWebDisco(string webDataUrl)
      {
         dataStreamDiscoveryUrl = webDataUrl;
         // seem to be getting problems here for no good reason.
         Debug.Log("starting the WWW BlockingReader for the data streamer @ " + dataStreamDiscoveryUrl);
         WWW www = new WWW(dataStreamDiscoveryUrl);
         Debug.Log("Waiting...");
         yield return www;
         Debug.Log("Got response: " + www.text);

         ParseWWWresponse(www.text);

         if (port > 0 && !String.IsNullOrEmpty(strmhost) && !String.IsNullOrEmpty(getDataCommand))
         {
            StartReader();
         }
         else
         {
            Debug.Log("Got empty/bad result from the data streamer web url");
         }

         yield return null;
      }


      internal void ParseWWWresponse(string xmltext)
      {
         // internal compiler errors when we try to use try-catches in an IEnumerator block; but it's happy to do it in a function call?
         try
         {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmltext);
            XmlElement root = xmlDoc.DocumentElement;
            strmhost = root.GetAttribute("serverip");
            port = Int32.Parse(root.GetAttribute("dataport"));
            getDataCommand = root.GetAttribute("datacmd");
         }
         catch (System.Exception ex)
         {
            Debug.Log("Got xml exception from the data streamer web url");
         }
      }


      public IEnumerator StartViaDirect(string serverIP, int streamPort, string streamCmd)
      {
         strmhost = serverIP;
         port = streamPort;
         getDataCommand = streamCmd;

         if (port > 0 && !String.IsNullOrEmpty(strmhost) && !String.IsNullOrEmpty(getDataCommand))
         {
            StartReader();
         }
         else
         {
            Debug.Log("Empty/bad parms passed to start direct");
         }

         yield return null;
      }

      internal void StartReader()
      {
         channelsToGetCommand = getDataCommand + "?copx,copy,cogx,cogy,fz,mx,my,velocityl,velocityr,timestamp";   // ask for only these channels
         numChans = 10;  // we expect only 10 channels

         buffer = new byte[numChans * sizeof(float) + (3 * sizeof(int))];   // 3 ints with the length, the row, and the # of chans (floats, also 4 bytes)
         headerBlock = new int[3];
         data = new float[numChans];

         Debug.Log("Connecting to stream server at " + strmhost + ":" + port + "...");
         socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
         IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(strmhost), port);
         IAsyncResult result = socket.BeginConnect(ipEndPoint, ReaderConnectedCallback, this);
      }

      internal void ReaderConnectedCallback(IAsyncResult ar)
      {
         Debug.Log("Connected to stream server at " + strmhost + ":" + port);
         try
         {
            if (ReaderConnected != null)
               ReaderConnected(strmhost, port);

            exitReader = false;
            socket.EndConnect(ar);
            socket.Send(Encoding.ASCII.GetBytes(channelsToGetCommand)); // tell the server to start sending data to us.
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, this);
         }
         catch (System.Exception ex)
         {
            string errorStr = ex.ToString();
         }
      }

      internal void ReceiveCallback(IAsyncResult ar)
      {
         int bytesRead = socket.EndReceive(ar);

         if (bytesRead == buffer.Length)
         {
            //if (header[0] == ((numChans * 8) + (2 * 4)))
            //if (header[2] == numChans)
            Buffer.BlockCopy(buffer, 0, headerBlock, 0, (3 * sizeof(int)));
            Buffer.BlockCopy(buffer, (3 * sizeof(int)), data, 0, numChans * sizeof(float));
            lock (_locker)
            {
               _dataValues.copXvalue = data[0];
               _dataValues.copYvalue = data[1];
               _dataValues.cogXvalue = data[2];
               _dataValues.cogYvalue = data[3];
               _dataValues.fzValue = data[4];
               _dataValues.mxValue = data[5];
               _dataValues.myValue = data[6];
               _dataValues.velocityl = data[7];
               _dataValues.velocityr = data[8];
               _dataValues.timestamp = (long)data[9];
            }

            if (DataReceived != null)
               DataReceived(strmhost, port, _dataValues);
         }

         bool socketStillOpen = true;
         try
         {
            socketStillOpen = !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
         }
         catch (SocketException) 
         {
            socketStillOpen = false; 
         }

         if (!socket.Connected)
            socketStillOpen = false;

         if (exitReader || !socketStillOpen)
         {
            socket.Close();
         }
         else
         {
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, this);
         }
      }

      public void StopReader()
      {
         exitReader = true;
         if (socket != null)
            socket.Close();
      }

    

      public DataValues Data
      {
         get { lock (_locker) { return _dataValues; } }
      }
   }
}
