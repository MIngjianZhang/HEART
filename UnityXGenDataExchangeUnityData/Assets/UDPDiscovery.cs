using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Threading;


public class UDPDiscovery
{
   public delegate void ServerDiscoveredHandler(string serverIP,string serverName,string webDataUrl, int webServerPort,int dataStreamServerPort, string dataStreamCmd);
   public event ServerDiscoveredHandler ServerDiscovered;

   ///////////////////////////////////////////////////

   internal UdpClient sock = null;

   internal bool foundUDPServer = false;

   internal Timer checkTimer = null;
   
   ///////////////////////////////////////////////////

   public void StartDiscovery()
   {
      StopDiscovery();
      sock = new UdpClient();
      sock.ExclusiveAddressUse = false;
      sock.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
      
      IPEndPoint iep = new IPEndPoint(IPAddress.Broadcast, 61712);

      byte[] data = Encoding.ASCII.GetBytes("Hello UDP Server!!!");

      sock.Send(data, data.Length, iep);

      //UnityEngine.Debug.Log("starting threaded receive for udp disco");
      sock.BeginReceive(ReceiveCallback, this);

      // start the watchdog timeout from the .net thread pool (avoids using main thread enquing and this claas from using monobehave)
      checkTimer = new Timer(CheckIfConnectedAndRetry);
      checkTimer.Change(2000, 0);
   }

   public void StopDiscovery()
   {
      if (checkTimer != null)
      {
         checkTimer.Dispose();
         checkTimer = null;
      }

      if (sock != null)
      {
         sock.Close();
         sock = null;
      }
   }

   // This is called after two seconds (above, checkTimer.Change(2000, 0)) if the udp hasn't discovered a server yet. If the foundUDPServer is not set
   // and sock is not null, then we restart the process
   internal void CheckIfConnectedAndRetry(object _checkTimer)
   {
      // The _checkTimer object is the Timer object. We don't care, we just directly kill our own
      if (checkTimer != null)
      {
         checkTimer.Dispose();
         checkTimer = null;
      }

      if (!foundUDPServer && sock != null)  // if StopDiscovery has been called, then sock will be null
         StartDiscovery();
   }

   internal void ReceiveCallback(IAsyncResult ar)
   {
      IPEndPoint iep2 = new IPEndPoint(IPAddress.Any, 0);

      String rcvData = System.Text.Encoding.UTF8.GetString(sock.EndReceive(ar, ref iep2));

      UnityEngine.Debug.Log("receive data " + rcvData);
      UnityEngine.Debug.Log("receive data from " + iep2.ToString());

      sock.Close();
      sock = null;

      //"<response serverip="192.168.2.82" name="TODDWILSON" webport="61710" dataport="61713">Hello UDP Server!!!</response>" 
      try
      {
         XmlDocument xmlDoc = new XmlDocument();
         xmlDoc.LoadXml(rcvData);
         XmlElement root = xmlDoc.DocumentElement;
         string serverIP = root.GetAttribute("serverip");
         string serverName = root.GetAttribute("name");
         string weburl = root.GetAttribute("webdataurl");
         string webdatacmd = root.GetAttribute("webdatacmd");
         int webServerPort = Int32.Parse(root.GetAttribute("webport"));
         int dataStreamServerPort = Int32.Parse(root.GetAttribute("dataport"));
         string datacmd = root.GetAttribute("datacmd");

         foundUDPServer = true;  // set the flag and kill our watchdog timer while we're at it.
         if (checkTimer != null)
         {
            checkTimer.Dispose();
            checkTimer = null;
         }

         // we have to use this sort of BS because the DataStreamServerDiscovered code is called inside the context of another thread, and just being able to do the Qt magic
         // of "Invoke" across a thread boundary doesn't work.
         if (ServerDiscovered != null)
            ExecuteOnMainThreadQueue.AddAction(() => { ServerDiscovered(serverIP, serverName, weburl, webServerPort, dataStreamServerPort, datacmd); });
      }
      catch (System.Exception ex)
      {
         // ignore all errors here
         UnityEngine.Debug.Log("got exception during udp disco xml processing");
      }
   }
}
