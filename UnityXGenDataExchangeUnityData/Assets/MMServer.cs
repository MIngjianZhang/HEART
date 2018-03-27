using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Linq;
using System.Threading;
using System.Globalization;

/// <summary>
/// The MMServer is the handler for receiving and processing MM commands from Motion Monitor.
/// To use: instantiate a new MMServer object and set event notifications, then call Start().
/// When done, call Stop(), which will end the server and shut it down.
/// Inside of your main Update() loop call MMServer.Pulse() to service update loops and main thread functionality.
/// </summary>
class MMServer
{
   /// <summary>
   /// The server's port. The default and expected value by MM is 50008.
   /// </summary>
   public int ListenPort;

   ///////////////////////////////////////////////////

   /// <summary>
   /// Event signaling that a connection has been opened.
   /// Note that this is called in the context of this server's thread, not your main, so may require thread queuing.
   /// </summary>
   /// <param name="connectionID">The connection identifier corrisponding to the client.</param>
   /// <param name="ipaddress">The client's ip address.</param>
   public delegate void ConnectionOpenedHandler(int connectionID,string ipaddress);

   /// <summary>
   /// Event signaling that a connection has been opened.
   /// Note that this is called in the context of this server's thread, not your main, so may require thread queuing.
   /// </summary>
   public event ConnectionOpenedHandler ConnectionOpened;

   /// <summary>
   /// Event signaling that a connection has been closed.
   /// Note that this is called in the context of this server's thread, not your main, so may require thread queuing.
   /// </summary>
   /// <param name="connectionID">The connection identifier corrisponding to the client.</param>
   public delegate void ConnectionClosedHandler(int connectionID);

   /// <summary>
   /// Event signaling that a connection has been closed.
   /// Note that this is called in the context of this server's thread, not your main, so may require thread queuing.
   /// </summary>
   public event ConnectionClosedHandler ConnectionClosed;

   /// <summary>
   /// Event signaling that an xml command string has been received. This is called just prior to default parsing and processing. 
   /// Note that this is called in the context of this server's thread, not your main, so may require thread queuing.
   /// </summary>
   /// <param name="connectionID">The connection identifier corrisponding to the client connection this command came over.</param>
   /// <param name="tagName">The tag name ex: <foo></foo></param>
   /// <param name="keyValuePairs">The attributes for the tag. Key=value. You will need to cast or convert each value as needed.</param>
   /// <returns>Return TRUE to indicate your code has processed this command and that default processing should not take place.</returns>
   public delegate bool CommandReceivedHandler(int connectionID, string tagName, Hashtable keyValuePairs);

   /// <summary>
   /// Event signaling that an xml command string has been received. This is called just prior to default parsing and processing. 
   /// Note that this is called in the context of this server's thread, not your main, so may require thread queuing.
   /// </summary>
   public event CommandReceivedHandler CommandReceived;

   // Handlers for each of the supports MM commands. Each are passed the connection ID and a key-value hash table. If the callback returns TRUE then
   // the default processing is not handled. Note that these are each called in the context of this server's thread, not your main, so may require
   // thread queuing for gameobject handling (which the default processing does)

   /// <summary>
   /// Event signaling for the default processing of the commands. These are called just prior to the default processing of each.
   /// Note that these are called in the context of this server's thread, not your main, so may require thread queuing.
   /// </summary>
   /// <param name="connectionID">The connection identifier corrisponding to the client connection this command came over.</param>
   /// <param name="keyValuePairs">The attributes for the tag. Key=value. You will need to cast or convert each value as needed.</param>
   /// <returns>Return TRUE to indicate your code has processed this command and that default processing should not take place.</returns>
   public delegate bool CommandHandler(int connectionID, Hashtable keyValuePairs);

   /// <summary>
   /// Event signaling for the default processing of the "addobj" command.
   /// Note that this is called in the context of this server's thread, not your main, so may require thread queuing.
   /// </summary>
   public event CommandHandler AddObject;

   /// <summary>
   /// Event signaling for the default processing of the "updobj" command.
   /// Note that this is called in the context of this server's thread, not your main, so may require thread queuing.
   /// </summary>
   public event CommandHandler UpdateObject;

   /// <summary>
   /// Event signaling for the default processing of the "remobj" command.
   /// Note that this is called in the context of this server's thread, not your main, so may require thread queuing.
   /// </summary>
   public event CommandHandler RemoveObject;

   /// <summary>
   /// Event signaling for the default processing of the "updvar" command.
   /// See also VariableChangedNotificationHandler, AddVariableChangedNotification, RemoveVariableChangedNotification
   /// Note that this is called in the context of this server's thread, not your main, so may require thread queuing.
   /// </summary>
   public event CommandHandler UpdateVariable;

   /// <summary>
   /// Event signaling that the server is about to start sending <frame></frame> xml blocks back to the client. This is called once,
   /// not on every frame. The rate value is passed in the "rate" keyValuePairs parm. Return TRUE to indicate that your code has
   /// elected to handle the processing and that the default logic should not take place.
   /// </summary>
   public event CommandHandler StartedClientUpdateFrames;

   /// <summary>
   /// Event signaling has stopped sending <frame></frame> blocks to the client. This is called once, but only as a notification -
   /// the default processing will still stop the frames and clean up data.
   /// </summary>
   public event CommandHandler StoppedClientUpdateFrames;

   ///////////////////////////////////////////////////

   internal TcpListener server = null;

   internal Timer checkDeadSocketsTimer = null;

   //////////////////////////////////////////////////////////////////////////

   /// <summary>
   /// This is the named layer that all MM objects will be created in; all physics in this layer-to-layer interaction will
   /// be ignored (to mimic how WorldViz does it). The MM objects will still interact with other layers. Defaults to "mmLayer" and typically does not need changed.
   /// </summary>
   public string mmCreatedObjectsLayer = "mmLayer";

   internal int mmCreatedObjectsLayerNumber = 31;  // by default it will be the last one.
   
   ///////////////////////////////////////////////////

   internal class MMconnection
   {
      public MMServer pServer;

      public int id;
      public Socket socket=null;
      public int bufferSize = 8192;
      public byte[] buffer;

      public void Init(int i,Socket sock)
      {
         id = i;
         buffer = new byte[bufferSize];
         socket = sock;
         socket.NoDelay = true;
         socket.ReceiveTimeout = 150;  // 150ms should be more than enough (unlike sendtimeout, no floor limit)
         socket.SendTimeout = 10; //The time-out value, in milliseconds. If you set the property with a value between 1 and 499, the value will be changed to 500. The default value is 0, which indicates an infinite time-out period. Specifying -1 also indicates an infinite time-out period.
#if _SET_KEEPALIVE_
         int size = sizeof(UInt32);
         UInt32 on = 1;
         UInt32 keepAliveInterval = 10000; //Send a packet once every 10 seconds.
         UInt32 retryInterval = 1000; //If no response, resend every second.
         byte[] inArray = new byte[size * 3];
         Array.Copy(BitConverter.GetBytes(on), 0, inArray, 0, size);
         Array.Copy(BitConverter.GetBytes(keepAliveInterval), 0, inArray, size, size);
         Array.Copy(BitConverter.GetBytes(retryInterval), 0, inArray, size * 2, size);
         socket.IOControl(IOControlCode.KeepAliveValues, inArray, null);
#endif
      }

      public void Close()
      {
         if (socket != null)
         {
            socket.Close();
            if (pServer != null)
               pServer.SocketClosed(id);  // removes the begstr timers and calls the connectionclosed callback
         }
         socket = null;
      }

      public bool IsConnected()
      {
         if (socket!=null)
         {
            bool f = true;
            try
            {
               f = !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) 
            {
               f = false; 
            }

            if (!f)
            {
               Debug.Log("Socket dead, closing");
               Close();
            }

            return f;
         }
         return false;
      }
   }

   ///////////////////////////////////////////////////

   internal int nextSocketID = 1;
   internal Dictionary<int,MMconnection> sockets = new Dictionary<int,MMconnection>();

   ///////////////////////////////////////////////////

   // the game objects (models) created by addobj
   internal Dictionary<string, UnityEngine.GameObject> mmObjects = new Dictionary<string, UnityEngine.GameObject>();

   // the <frame> update timers keyed to a socket
   internal Dictionary<int, Timer> begStrTimers = new Dictionary<int, Timer>();   // keyed to the socket

   // Variable update listeners can register via AddVariableChangedNotification(name,eventhandler) and RemoveVariableChangedNotification(name,eventhandler)
   // These are all added to the list for each name and each are called in turn (but order is not guaranteed)

   /// <summary>
   /// Event signaling that a variable has been updated. See also AddVariableChangedNotification and RemoveVariableChangedNotification
   /// </summary>
   /// <param name="varname">The variable that was changed. Ex: "headpos". Duplicated here for the same handler for multiple variables.</param>
   /// <param name="value">The parameters sent from the client. You will need to typecast based on your needs. Either will be double, a Vector3, or a Quaternion.</param>
   public delegate void VariableChangedNotificationHandler(string varname, object value);

   internal Dictionary<string, VariableChangedNotificationHandler> varUpdatedCallbacks = new Dictionary<string, VariableChangedNotificationHandler>();

   internal readonly Queue<Action> executeOnMainThread = new Queue<Action>();

   // Anything set or added via SetFrameVariable will be put in here, and the next ClientFrameSendTimeEvent will send it out.
   // Note that this does not need to match the variables that are coming in from MM.
   internal Dictionary<string,object> frameVariables = new Dictionary<string, object>();

   ///////////////////////////////////////////////////

   /// <summary>
   /// Initializes a new instance of the MMServer class. 
   /// </summary>
   public MMServer()
   {
      ListenPort = 50008;
   }

   /// <summary>
   /// Starts the MM server. You should have set up your events and callbacks prior to this.
   /// </summary>
   public void Start()
   {
      Stop();

      int layNum = LayerMask.NameToLayer(mmCreatedObjectsLayer);
      if (layNum >= 0)
         mmCreatedObjectsLayerNumber = layNum;

      Physics.IgnoreLayerCollision(mmCreatedObjectsLayerNumber, mmCreatedObjectsLayerNumber, true);

      server = new TcpListener(IPAddress.Any, ListenPort);

      server.Start();
      Debug.Log("MMserver started");
      server.BeginAcceptSocket(AcceptSocketCallback, server);

      checkDeadSocketsTimer = new Timer(CheckForClosedSockets, null, 2000, 2000);
   }

   /// <summary>
   /// Stops the MM server. Closes all connections, stops all timed processes.
   /// </summary>
   public void Stop()
   {
      ClearQueuedActions();
      if (checkDeadSocketsTimer != null)
      {
         checkDeadSocketsTimer.Change(Timeout.Infinite, Timeout.Infinite);
         checkDeadSocketsTimer.Dispose();
         checkDeadSocketsTimer = null;
      }

      if (server != null)
         server.Stop();
      server = null;

      lock(sockets)
      {
         foreach (KeyValuePair<int, MMconnection> entry in sockets)
         {
            entry.Value.Close();
         }

         sockets.Clear();
      }
   }

   /// <summary>
   /// Adds the passed Action object to the queue to execute later on the main thread.
   /// Make sure your main Update() loop calls MMServer.ProcessQueuedActions()
   /// </summary>
   /// <param name="act">The action to perform. Ex: () => { CommandAddObject(connectionID, keyValuePairs); }</param>
   public void AddAction(Action act)
   {
      lock (executeOnMainThread)
      {
         executeOnMainThread.Enqueue(act);
      }
   }

   /// <summary>
   /// Processes the Action queue and executes them. This is called from the Pulse loop and should only be called from your main thread function.
   /// </summary>
   public void ProcessQueuedActions()
   {
      lock (executeOnMainThread)
      {
         while (executeOnMainThread.Count > 0)
         {
            executeOnMainThread.Dequeue().Invoke();
         }
      }
   }

   /// <summary>
   /// This should be called from your main Update() loop to service any main thread functions that MMServer needs to handle, such as
   /// dispatching queued actions. 
   /// </summary>
   public void Pulse()
   {
      ProcessQueuedActions();
   }

   /// <summary>
   /// Removes all queue actions from the main thread execute queue.
   /// </summary>
   public void ClearQueuedActions()
   {
      lock (executeOnMainThread)
      {
         executeOnMainThread.Clear();
      }
   }

   // This is called via the checkDeadSocketsTimer every 2 seconds to check for stale dead sockets that send/recv missed.
   internal void CheckForClosedSockets(object _connectionID)
   {
      lock(sockets)
      {
         var toRemove = sockets.Where(pair => !pair.Value.IsConnected())
                            .Select(pair => pair.Key)
                            .ToList();

         foreach (var key in toRemove)
         {
            sockets.Remove(key);
         }
      }
   }

   /// <summary>
   /// Forcibly close the connection instead of letting the client do it.
   /// </summary>
   /// <param name="connectionID">The connection ID corrisponding to the client connection. Same as what is sent on most events and ConnectionOpened.</param>
   public void CloseConnection(int connectionID)
   {
      lock(sockets)
      {
         if (sockets.ContainsKey(connectionID))
         {
            sockets[connectionID].Close();
            sockets.Remove(connectionID);
         }
      }
   }

   internal void SocketClosed(int connectionID)
   {
      CommandStopClientFrameSend(connectionID, new Hashtable());

      if (ConnectionClosed != null)
         ConnectionClosed(connectionID);
   }

   internal void AcceptSocketCallback(IAsyncResult ar)
   {
      Debug.Log("MMserver AcceptSocketCallback");

      MMconnection con = new MMconnection();
      con.pServer = this;
      con.Init(nextSocketID, server.EndAcceptSocket(ar));  // accept the connection and start a new one

      string ip = ((IPEndPoint)con.socket.RemoteEndPoint).Address.ToString();

      server.BeginAcceptSocket(AcceptSocketCallback, server);

      lock(sockets)
      {
         sockets.Add(nextSocketID, con);
      }

      if (ConnectionOpened != null)
         ConnectionOpened(nextSocketID,ip);

      ++nextSocketID;

      con.socket.BeginReceive(con.buffer, 0, con.bufferSize, 0, ReceiveCallback, con);
   }

   internal void ReceiveCallback(IAsyncResult ar)
   {
      Debug.Log("MMserver ReceiveCallback for socket");

      MMconnection con = (MMconnection)ar.AsyncState;
      Socket clientSocket = con.socket;

      int bytesRead = -1;

      // Read data from the remote device into con.buffer
      try
      {
         bytesRead = clientSocket.EndReceive(ar);
      }
      catch (System.Exception ex)
      {
         // If EndReceive throws, there has been an abnormal termination of the connection (process killed, network cable cut, power lost, etc).
         Debug.Log("Exception " + ex.ToString() + " in ReceiveCallback; assume conneciton dropped and closing socket.");
         con.Close();
         return;
      }

      if (bytesRead<=0)
      {
         // If EndReceive returns zero, the remote host has closed its end of the connection.  
         Debug.Log("EndReceive returned zero bytes. Connection closed.");
         con.Close();
         return;
      }

#if _CHECK_CONNECTED_OFTEN_
      if (!clientSocket.Connected || !con.IsConnected())
      {
         Debug.Log("client socket or connection indicated no longer connected.");
         con.Close();
         return; // we're done, no reason to keep going
      }
#endif

      if (bytesRead > 5)   // should be at least this amount
      {
         String xmlText = Encoding.ASCII.GetString(con.buffer, 0, bytesRead);
         Debug.Log(xmlText);

         try
         {
            xmlText = "<bogus>" + xmlText + "</bogus>";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlText);

            XmlNodeList nodelist = xmlDoc.DocumentElement.ChildNodes; // get all nodes

            foreach (XmlNode node in nodelist) // for each node
            {
               String tagName = node.Name;

               XmlAttributeCollection tribs = node.Attributes;
               Hashtable keyPairs = new Hashtable();
               for (int i = 0; i < tribs.Count; ++i)
               {
                  String tribName = tribs[i].Name;
                  String tribValue = tribs[i].Value;
                  keyPairs.Add(tribName, tribValue);
               }

               bool doDefaultProc = true;
               if (CommandReceived != null)
                  doDefaultProc = !CommandReceived(con.id, tagName, keyPairs);

               if (doDefaultProc)
                  ProcessMMCommand(con.id, tagName, keyPairs);
            }
         }
         catch (System.Exception ex)
         {
            Debug.Log("XML parsing error: " + ex.ToString());
         }
      }

      // continue to receive data on another thread
      clientSocket.BeginReceive(con.buffer, 0, con.bufferSize, 0, ReceiveCallback, con);
   }

      // this is run in the main thread's loop
   internal void ProcessMMCommand(int connectionID, string tagName, Hashtable keyValuePairs)
   {
      {  // debug logging
         string text = "";
         IDictionaryEnumerator e = keyValuePairs.GetEnumerator();
         while (e.MoveNext())
         {
            text += e.Key + "=" + e.Value + " , ";
         }
         Debug.Log("ProcessMMCommand " + connectionID + " cmd: " + tagName + " keyValuePairs " + text);
      }

      switch (tagName.ToLower())
      {
         case "addobj": // add an object
            if (AddObject != null)
               if (AddObject(connectionID, keyValuePairs))
                  return;
            AddAction(() => { CommandAddObject(connectionID, keyValuePairs); });
            break;
         case "updobj": // update an existing object
            if (UpdateObject != null)
               if (UpdateObject(connectionID, keyValuePairs))
                  return;
            AddAction(() => { CommandUpdateObject(connectionID, keyValuePairs); });
            break;
         case "remobj": // remove all or an existing object
            if (RemoveObject != null)
               if (RemoveObject(connectionID, keyValuePairs))
                  return;
            AddAction(() => { CommandRemoveObject(connectionID, keyValuePairs); });
            break;
         case "updvar": // update a variable
            if (UpdateVariable != null)
               if (UpdateVariable(connectionID, keyValuePairs))
                  return;
            AddAction(() => { CommandUpdateVariable(connectionID, keyValuePairs); });
            break;
         case "begstr": // start sending info back to the mm client. Note that we don't have to schedule handling like we do with the above.
            CommandStartClientFrameSend(connectionID, keyValuePairs);
            break;
         case "endstr": // stop sending info back to the mm client
            CommandStopClientFrameSend(connectionID, keyValuePairs);
            break;
      }
   }


   /// <summary>
   /// Send data directly to the client, in a blocking fashion. Not recommended for general use.
   /// </summary>
   /// <param name="connectionID">The connection id to send data to. If the connection id does not exist or is closed, nothing happens.</param>
   /// <param name="data">The data to send. To send a string, use Encoding.ASCII.GetBytes</param>
   /// <returns></returns>
   public bool SendToConnection_Direct(int connectionID, byte[] data)
   {
      lock(sockets)
      {
         if (sockets.ContainsKey(connectionID))
         {
            MMconnection con = sockets[connectionID];
            try
            {
               SocketError errorCode;
               con.socket.Send(data, 0, data.Length, 0, out errorCode);
               if (errorCode == SocketError.Success)
                  return true;
               Debug.Log("SendToConnection failed with error code " + errorCode.ToString());
            }
            catch (System.Exception ex)
            {
               Debug.Log("SendToConnection threw exception " + ex.ToString());
            }

#if _CHECK_CONNECTED_OFTEN_
            if (!con.IsConnected())
            {
               con.socket.Send(data, 0, data.Length, 0);
               return true;
            }
#endif
         }
      }
      Debug.Log("SendToConnection failed");

      return false;
   }

   /// <summary>
   /// Send data directly to the client, in a non-blocking queued fashion.
   /// </summary>
   /// <param name="connectionID">The connection id to send data to. If the connection id does not exist or is closed, nothing happens.</param>
   /// <param name="data">The data to send. To send a string, use Encoding.ASCII.GetBytes</param>
   /// <returns></returns>
   public bool SendToConnection(int connectionID, byte[] data)
   {
      lock(sockets)
      {
         if (sockets.ContainsKey(connectionID))
         {
            MMconnection con = sockets[connectionID];
            try
            {
               SocketError errorCode;
               con.socket.BeginSend(data, 0, data.Length, 0, out errorCode, SendToConnection_Complete, con);
               if (errorCode==SocketError.Success)
                  return true;
               Debug.Log("SendToConnection failed with error code " + errorCode.ToString());
            }
            catch (System.Exception ex)
            {
               Debug.Log("SendToConnection threw exception " + ex.ToString());
            }
#if _CHECK_CONNECTED_OFTEN_
            if (con.IsConnected())
            {
               //byte[] buff = Encoding.ASCII.GetBytes(s);
               con.socket.BeginSend(data, 0, data.Length, 0, SendToConnection_Complete, con);
               return true;
            }
#endif
         }
      }
      Debug.Log("SendToConnection failed");

      return false;
   }

   internal void SendToConnection_Complete(IAsyncResult ar)
   {
      MMconnection con = (MMconnection)ar.AsyncState;
      Socket clientSocket = con.socket;

      // finish sending the data - this will block
      SocketError errorCode;
      int bytesSent = clientSocket.EndSend(ar, out errorCode);

      if (errorCode != SocketError.Success)
      {
         Debug.Log("SendToConnection failed with error code " + errorCode.ToString());
         con.Close();   // may already be closed, and if so,this does nothing
         return;
      }

      if (bytesSent<1)
      {
         Debug.Log("SendToConnection_Complete returned " + bytesSent + ", assuming fault and closing connection.");
         con.Close();   // may already be closed, and if so,this does nothing
         return;
      }

#if _CHECK_CONNECTED_OFTEN_
      if (!clientSocket.Connected || !con.IsConnected())
      {
         con.Close();   // may already be closed, and if so,this does nothing
      }
#endif
   }

   ///////////////////////////////////////////////////////////////////////////
   // Handlers for the <begstr> stuff

   // The "begstr" handler: what this does is build the <frame> string and sends it back on a timer.
   internal void CommandStartClientFrameSend(int connectionID, Hashtable keyValuePairs)
   {
      CommandStopClientFrameSend(connectionID, keyValuePairs);  // will invoke a callback if we have an existing begstr on this connection running

      if (StartedClientUpdateFrames != null)
         if (StartedClientUpdateFrames(connectionID, keyValuePairs))
            return;  // the callback handler handled it for us.

      Single rate = 100.0f;
      if (keyValuePairs.Contains("rate"))
      {
         rate = parseSingle(keyValuePairs["rate"]);
         if (Single.IsNaN(rate) || rate < 1.0f)
            return;
      }

      int msRate = (int)(1000.0f / rate);

      Debug.Log("begstr rate(ms)=" + msRate);

      lock(begStrTimers)
      {
         begStrTimers[connectionID] = new Timer(ClientFrameSendTimeEvent, connectionID, msRate, msRate);
      }
   }

   // called from the socket's thread, but removing the dict entry should be thread safe
   internal void CommandStopClientFrameSend(int connectionID, Hashtable keyValuePairs)
   {
      Debug.Log("MM_endstr ending with " + connectionID);

      bool hasBegStr = false;
      lock(begStrTimers)
      {
         Timer tm = null;
         hasBegStr = begStrTimers.TryGetValue(connectionID, out tm);
         begStrTimers.Remove(connectionID);  // remove it from the list always
         if (tm != null)
         {
            tm.Change(Timeout.Infinite, Timeout.Infinite);  // this will kill the timer's ClientFrameSendTimeEvent callback 
            tm.Dispose();
            tm = null;
         }
      }

      if (hasBegStr)
      {
         if (StoppedClientUpdateFrames != null)
            StoppedClientUpdateFrames(connectionID, keyValuePairs);  // we don't care about the return value here
      }
   }

   // This is called from the timer thread on the rate set by the rate attribute. As long as the Timer object hasn't been disposed, it will be called.
   // Checking the begStrTimers table for the matching connection id will do nothing for us. 
   internal void ClientFrameSendTimeEvent(object _connectionID)
   {
      int connectionID = (int)parseSingle(_connectionID); // avoid exceptions when passed 1.2

      lock(begStrTimers)
      {
         Timer tm = null;
         if (!begStrTimers.TryGetValue(connectionID, out tm))
         {
            Debug.Log("Still getting events even if connection dropped.");
            return;
         }
      }

      // <frame>
      // <var name="SCALAR_name" val="VALUE"></var>
      // <var name="VECTOR_name" x="X" y="Y" z="Z"></var>
      // <var name="QUATRONION_name" qx="QX" qy="QY" qz="QZ" qw="QW"></var>
      // </frame>
      string varData = "";


      foreach (KeyValuePair<string, object> entry in frameVariables)
      {
         string varname = entry.Key;
         object value = entry.Value;

         string data = "";

         if (value is Vector3)
         {
            Vector3 v3 = (Vector3)value;
            data = string.Format("x=\"{0}\" y=\"{1}\" z=\"{2}\"", v3.x, v3.y, v3.z);
         }
         else if (value is Quaternion)
         {
            Quaternion q = (Quaternion)value;
            data = string.Format("qw=\"{0}\" qx=\"{1}\" qy=\"{2}\" qz=\"{3}\"", q.w, q.x, q.y, q.z);
         }
         else // probably a scaler or a string, so default to the base which does it for us
         {
            data = string.Format("value=\"{0}\"", value.ToString());
         }

         varData += string.Format("<var name=\"{0}\" {1}></var>\n", varname, data);
      }

      decimal milliseconds = DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond;

      string s = string.Format("<frame connection=\"{0}\" time=\"{1}\">{2}</frame>\n", connectionID, milliseconds, varData);

      Debug.Log(s);

      if (!SendToConnection(connectionID, Encoding.ASCII.GetBytes(s)))
      {
         // we failed to send the frame, which typically means we've dropped the connection. Go and remove the event from our list (same as endstr)
         Hashtable empty = new Hashtable();
         CommandStopClientFrameSend(connectionID, empty);
      }
   }


   //this will add an object to the visual world. 
   //The object model must already be known/accessible to the runtime engine (in project or on disk? Right now we limit to assets in the project's Resources folder).
   //All models are “static” and cannot be moved by other collision objects or the ui’s physics engine.
   // i: string or number, required. This the index key into the object table that this object should be using. If an object already exists at that key it will be removed and replaced with this one.
   // model: String, required. The prefab name as set in the Unity Editor. Ex: sphere.obj. The actual material texture used is/can be defined later.
   // image: String, optional. The material texture to be used with this model. If set, then red/green/blue values are ignored.
   //    If not set, then the model’s own material (if any) is used.
   // red,green,blue: Real, optional. The color/tint for the object. Only used if the ‘image’ parm is not set. The default tint is black [0,0,0].
   // o: Real, optional. The opacity level of the object. If not set the default/current opacity of the object (or its material) is used. A value of 0.0 hides the object.
   // x,y,z: Real, optional. The position of the object in the world. If not set, a value of [0, 1000, -1000] is used.
   // qw,qx,qy,qz: Real, optional. Quaternion (rotational) component of the object. If not set, no angle is set for the object (which assumes a zero-angle but this is not explicitly declared in the code).
   // sx,sy,sz: Real, optional. Scaling factor of the object. If not set, a value of [1.0,1.0,1.0] is used.
   // vx,vy,vz: Real, optional. The object's velocity. If not set, a value of [0.0,0.0,0.0] is used (object is static).
   // avx,avy,avz: Real, optional. The object's angular (rotational) velocity. If not set, a value of [0.0,0.0,0.0] is used (the object does not rotate by itself).
   internal void CommandAddObject(int connectionID, Hashtable keyValuePairs)
   {
      string tableKey = keyValuePairs["i"].ToString();
      string modelName = keyValuePairs["model"].ToString();

      if (tableKey == "" || modelName == "")
         return;

      UnityEngine.Object resource = Resources.Load(modelName);
      if (resource == null)
         return;

      GameObject go = UnityEngine.Object.Instantiate(resource) as GameObject;

      if (go == null)
         return;

      if (mmCreatedObjectsLayerNumber>=0)
         go.layer = mmCreatedObjectsLayerNumber;

      go.name = go.name.Replace("(Clone)","-" + tableKey);

      Rigidbody rb = go.GetComponent<Rigidbody>();
      if (rb == null)
      {
         rb = go.AddComponent<Rigidbody>();
         rb.useGravity = false;  // so it doesn't fall into the floor
         rb.mass = 10;           // this is the suggested Unity limit so that it whacks stuff in the scene but doesn't get affected by it.
         rb.drag = 0;            // so the veloc doesn't slow down
         rb.angularDrag = 0;     // so the rotation doesn't slow down
      }

      UpdateGameObject(go, keyValuePairs);

      // remove what we have before adding the new one to the table.
      RemoveObjectCommon(tableKey);

      mmObjects[tableKey] = go;
   }


   //this updates the settings of an existing object in the world. The object must have been previously set via the <addobj> tag.
   // i: string or number, required. This the index key into the object table that this object should be using. If an object already exists at that key it will be removed and replaced with this one.
   // red,green,blue: Real, optional. The color/tint for the object. Only used if the ‘image’ parm is not set. The default tint is black [0,0,0].
   // o: Real, optional. The opacity level of the object. If not set the default/current opacity of the object (or its material) is used. A value of 0.0 hides the object.
   // x,y,z: Real, optional. The position of the object in the world. If not set, a value of [0, 1000, -1000] is used.
   // qw,qx,qy,qz: Real, optional. Quaternion (rotational) component of the object. If not set, no angle is set for the object (which assumes a zero-angle but this is not explicitly declared in the code).
   // sx,sy,sz: Real, optional. Scaling factor of the object. If not set, a value of [1.0,1.0,1.0] is used.
   // vx,vy,vz: Real, optional. The object's velocity. If not set, a value of [0.0,0.0,0.0] is used (object is static).
   // avx,avy,avz: Real, optional. The object's angular (rotational) velocity. If not set, a value of [0.0,0.0,0.0] is used (the object does not rotate by itself).
   internal void CommandUpdateObject(int connectionID, Hashtable keyValuePairs)
   {
      string tableKey = keyValuePairs["i"].ToString();

      if (tableKey == "")
         return;

      if (mmObjects.ContainsKey(tableKey))
      {
         GameObject go = mmObjects[tableKey];
         if (go != null)
            UpdateGameObject(go, keyValuePairs);
      }
   }

   // called when we remove an object by name
   internal void RemoveObjectCommon(string tableKey)
   {
      if (mmObjects.ContainsKey(tableKey))
      {
         // remove what we have
         RemoveObjectCommon(mmObjects[tableKey]);
         mmObjects.Remove(tableKey);
      }
   }

   // called to actually remove the object (either by name or when removing all).
   // This will set the object active, hide it, and then schedule it for deleting in 0.1 seconds - this is done so that
   // OnDestroy() will get called (because otherwise it will not)
   internal void RemoveObjectCommon(GameObject go)
   {
      if (go != null)
      {
         go.SetActive(true); // so that OnDestroy will get called

         // visually hide it
         if (go.GetComponentInChildren<Renderer>() != null)
            go.GetComponentInChildren<Renderer>().enabled = false;

         // We need to also turn off the collider if it's there; this solves a problem if we re-create the same object over top of the existing one;
         // it causes the new object to be ejected since it's invisibly collided with itself before it's previous version was zapped.
         if (go.GetComponent<Collider>() != null)
            go.GetComponent<Collider>().enabled = false;

         UnityEngine.Object.Destroy(go, 0.01f); // give the engine time to process the changes to Active before actually destroying it.
      }
   }

   //remove an existing object from the object list and delete it from the scene. 
   //If the "i" parm is not passed, all added objects are removed.
   internal void CommandRemoveObject(int connectionID, Hashtable keyValuePairs)
   {
      if (keyValuePairs.Contains("i"))
      {
         RemoveObjectCommon(keyValuePairs["i"].ToString());
      }
      else // remove all objects
      {

         foreach (KeyValuePair<string, GameObject> entry in mmObjects)
         {
            RemoveObjectCommon(entry.Value);
         }

         mmObjects.Clear();
      }
   }


   // common handler for addobj and updobj. This handles all of the things we can set.
   internal void UpdateGameObject(GameObject go, Hashtable keyValuePairs)
   {
      if (keyValuePairs.Contains("x") && keyValuePairs.Contains("y") && keyValuePairs.Contains("z"))
      {
         go.transform.localPosition = new Vector3(parseSingle(keyValuePairs["x"]), parseSingle(keyValuePairs["y"]), parseSingle(keyValuePairs["z"]));
      }

      // Bertec extension: pass a direct angle instead of a quaternion
      if (keyValuePairs.Contains("rx") && keyValuePairs.Contains("ry") && keyValuePairs.Contains("rz"))
      {
         go.transform.localEulerAngles = new Vector3(parseSingle(keyValuePairs["rx"]), parseSingle(keyValuePairs["ry"]), parseSingle(keyValuePairs["rz"]));
      }

      if (keyValuePairs.Contains("qx") && keyValuePairs.Contains("qy") && keyValuePairs.Contains("qz") && keyValuePairs.Contains("qw"))
      {
         go.transform.localRotation = new Quaternion(parseSingle(keyValuePairs["qx"]), parseSingle(keyValuePairs["qy"]), parseSingle(keyValuePairs["qz"]), parseSingle(keyValuePairs["qw"]));
      }

      if (keyValuePairs.Contains("sx") && keyValuePairs.Contains("sy") && keyValuePairs.Contains("sz"))
      {
         go.transform.localScale = new Vector3(parseSingle(keyValuePairs["sx"]), parseSingle(keyValuePairs["sy"]), parseSingle(keyValuePairs["sz"]));
      }

      if (keyValuePairs.Contains("vx") && keyValuePairs.Contains("vy") && keyValuePairs.Contains("vz"))
      {
         Rigidbody rb = go.GetComponent<Rigidbody>();
         rb.velocity = new Vector3(parseSingle(keyValuePairs["vx"]), parseSingle(keyValuePairs["vy"]), parseSingle(keyValuePairs["vz"]));
      }

      if (keyValuePairs.Contains("avx") && keyValuePairs.Contains("avy") && keyValuePairs.Contains("avz"))
      {
         Rigidbody rb = go.GetComponent<Rigidbody>();
         rb.angularVelocity = new Vector3(parseSingle(keyValuePairs["avx"]), parseSingle(keyValuePairs["avy"]), parseSingle(keyValuePairs["avz"]));
      }

      // set the transparency / opacity of the object. This only works if the material that is assigned has the proper shader (ex: transparent/diffuse not just diffuse) set.
      // NOTE: Unity uses a value of 0.0 -> 1.0 (hidden/fully shown); we do not know at this time what MM and WorldViz uses for their scaling. We may need to convert from 0->100 down to 0->1 etc
      if (keyValuePairs.Contains("o"))
      {
         Single opac = parseSingle(keyValuePairs["o"]);

         if (!Single.IsNaN(opac))
         {
            Renderer rend = go.GetComponentInChildren<Renderer>();

            if (rend != null)
            {
               if (opac < 0.1f)
               {
                  rend.enabled = false;
               }
               else
               {
                  rend.enabled = true;
               }

               if (rend.material != null)
               {
                  Color c = rend.material.color;
                  c.a = opac;
                  rend.material.color = c;
               }
            }
         }
      }

      //set the color of the object. Unlike WorldViz, Unity allows you to tint the object anyways, even if there is an image or material on it.
      if (keyValuePairs.Contains("red") && keyValuePairs.Contains("green") && keyValuePairs.Contains("blue"))
      {
         Renderer rend = go.GetComponentInChildren<Renderer>();

         if (rend != null)
         {
            if (rend.material != null)
            {
               Color c = rend.material.color;

               // keep the opacity/alpha value
               c.r = parseSingle(keyValuePairs["red"]);
               c.g = parseSingle(keyValuePairs["green"]);
               c.b = parseSingle(keyValuePairs["blue"]);

               rend.material.color = c;
            }
         }
      }

      //Bertec extension: set the color of the object via rgb or rgba hex value
      if (keyValuePairs.Contains("rgb") || keyValuePairs.Contains("rgba"))
      {
         Renderer rend = go.GetComponentInChildren<Renderer>();

         if (rend != null)
         {
            if (rend.material != null)
            {
               Color c = rend.material.color;

               string hex;
               if (keyValuePairs.Contains("rgb"))
                  hex = keyValuePairs["rgb"].ToString();
               else
                  hex = keyValuePairs["rgba"].ToString();
               hex = hex.Replace("0x", "").Replace("#", "");  // if the string is formatted 0xFFFFFF or #FFFFFF

               if (hex.Length>=6)
               {
                  c.r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                  c.g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                  c.b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                  if (hex.Length == 8)
                     c.a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

                  rend.material.color = c;
               }
            }
         }
      }

      // change the material of the object to use a different texture
      // we have extended this so that you can also change out the material via name instead of just the texture, and can change the material via updobj instead of just addobj
      if (keyValuePairs.Contains("image"))
      {
         Renderer rend = go.GetComponentInChildren<Renderer>();

         if (rend != null)
         {
            if (rend.material != null)
            {
               string newMatOrTex = keyValuePairs["image"].ToString();
               UnityEngine.Object resource = Resources.Load(newMatOrTex);
               if (resource == null)
                  return;

               if (resource is Material)
               {
                  rend.material = resource as Material;
               }
               else if (resource is Texture)
               {
                  if (rend.material != null)
                     rend.material.mainTexture = resource as Texture;
               }
            }
         }
      }
   }


   /////////////////////////////////////////////////////

   /// <summary>
   /// Add a notification handler for a given variable name via the "updvar" command. There can be multiple listeners for a given variable.
   /// The update will be queued and handled by your main thread Update() handler when it calls MMServer.Pulse().
   /// See VariableChangedNotificationHandler for the handler delegate.
   /// </summary>
   /// <param name="varname">The variable name to be notified for. Case sensistive.</param>
   /// <param name="handler">The VariableChangedNotificationHandler that should be called when the variable is updated.</param>
   public void AddVariableChangedNotification(string varname, VariableChangedNotificationHandler handler)
   {
      lock (varUpdatedCallbacks)
      {
         VariableChangedNotificationHandler d;
         if (varUpdatedCallbacks.TryGetValue(varname, out d))
            varUpdatedCallbacks[varname] = d += handler;
         else
            varUpdatedCallbacks[varname] = handler;  // none yet, so assign the first one
      }
   }

   /// <summary>
   /// Remove a notification handler for a given variable name.
   /// See VariableChangedNotificationHandler for the handler delegate.
   /// </summary>
   /// <param name="varname">The variable name to no longer be notified about. Case sensistive.</param>
   /// <param name="handler">The VariableChangedNotificationHandler delegate. It must be the same as what was passed to AddVariableChangedNotification.</param>
   public void RemoveVariableChangedNotification(string varname, VariableChangedNotificationHandler handler)
   {
      lock (varUpdatedCallbacks)
      {
         VariableChangedNotificationHandler d;
         if (varUpdatedCallbacks.TryGetValue(varname, out d))
         {
            d -= handler;

            if (d != null)
               varUpdatedCallbacks[varname] = d;      // put the new chain back into the dictionary
            else
               varUpdatedCallbacks.Remove(varname);   // all removed, we're done
         }
      }
   }

   /// <summary>
   /// Remove ALL notification handlers for a given variable name.
   /// </summary>
   /// <param name="varname">The variable name to no longer be notified about. Case sensistive. All handlers attached to this variable name are removed.</param>
   public void RemoveAllVariableChangedNotification(string varname)
   {
      lock (varUpdatedCallbacks)
      {
         varUpdatedCallbacks.Remove(varname);
      }
   }

   /// <summary>
   /// Sets or adds a frame variable that subsequent ClientFrameSendTimeEvent calls will send out. Note that just adding this will not
   /// trigger said event; this is controlled by begstr/endstr.
   /// </summary>
   /// <param name="varname">The variable name to be set. Case sensistive. Does not necessarily need to be the same variable name as what is being sent from MM.</param>
   /// <param name="value">The object value. Proper casting and encoding is done when the value is sent.</param>

   public void SetFrameVariable(string varname,object value)
   {
      lock (frameVariables)
      {
         frameVariables[varname] = value;
      }
   }
   /// <summary>
   /// Remove a frame variable by name name.
   /// </summary>
   /// <param name="varname">The variable name to be removed. Case sensistive.</param>

   public void RemoveFrameVariable(string varname)
   {
      lock (frameVariables)
      {
         frameVariables.Remove(varname);
      }
   }

   internal double parseDouble(object o)
   {
      try
      {
         return double.Parse(o.ToString(), CultureInfo.InvariantCulture);
      }
      catch (System.Exception ex)
      {
         return double.NaN;      	
      }
   }

   internal Single parseSingle(object o)
   {
      try
      {
         return Single.Parse(o.ToString(), CultureInfo.InvariantCulture);
      }
      catch (System.Exception ex)
      {
         return Single.NaN;
      }
   }


   internal void CommandUpdateVariable(int connectionID, Hashtable keyValuePairs)
   {
      string varname = keyValuePairs["name"].ToString();
      if (varname == "")
         return;

      // Certain other special pre-defined values should be handled here, such as eyeheight (a val), headpos (a vector), headori (a quat), etc.

      object value = null;
 
      if (keyValuePairs.Contains("val"))
      {
         double val = parseDouble(keyValuePairs["val"]);
         value = val;
      }

      if (keyValuePairs.Contains("x") && keyValuePairs.Contains("y") && keyValuePairs.Contains("z"))
      {
         Vector3 vec = new Vector3(parseSingle(keyValuePairs["x"]), parseSingle(keyValuePairs["y"]), parseSingle(keyValuePairs["z"]));
         value = vec;
      }

      if (keyValuePairs.Contains("qx") && keyValuePairs.Contains("qy") && keyValuePairs.Contains("qz") && keyValuePairs.Contains("qw"))
      {
         Quaternion quat = new Quaternion(parseSingle(keyValuePairs["qx"]), parseSingle(keyValuePairs["qy"]), parseSingle(keyValuePairs["qz"]), parseSingle(keyValuePairs["qw"]));
         value = quat;
      }

      VariableChangedNotificationHandler d;
      lock (varUpdatedCallbacks)
      {
         varUpdatedCallbacks.TryGetValue(varname, out d);
      }

      try
      {
         if (d != null)
            d.Invoke(varname, value);  // this will call all of the callbacks that are set to this variable
      }
      catch (System.Exception ex)
      {
         Debug.Log("Got an exception during a variable update notification: " + ex.ToString());
         // ignore exceptions.      	
      }
   }
}

