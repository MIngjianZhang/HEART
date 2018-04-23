/* Trigno SDK Windows Example.  Copyright (C) 2011 Delsys, Inc.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, and distribute the Software, 
 * and to permit persons to whom the Software is furnished to do so, subject to 
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
*/

using System;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace TrignoSDKExample
{
    public partial class MainForm : Form
    {
        //The following are used for TCP/IP connections
        private TcpClient commandSocket;
        private TcpClient emgSocket;
        private TcpClient accSocket;
        private const int commandPort = 50040;  //server command port
        private const int emgPort = 50041;  //port for EMG data
        private const int accPort = 50042;  //port for acc data

        //The following are streams and readers/writers for communication
        private NetworkStream commandStream;
        private NetworkStream emgStream;
        private NetworkStream accStream;
        private StreamReader commandReader;
        private StreamWriter commandWriter;

        //The following are storage for acquired data
        private float[] emgData = new float[16];
        private float[] accXData = new float[16];
        private float[] accYData = new float[16];
        private float[] accZData = new float[16];

        private bool connected = false; //true if connected to server
        private bool running = false;   //true when acquiring data

        //Server commands
        private const string COMMAND_QUIT = "QUIT";
        private const string COMMAND_GETTRIGGERS = "TRIGGER?";
        private const string COMMAND_SETSTARTTRIGGER = "TRIGGER START";
        private const string COMMAND_SETSTOPTRIGGER = "TRIGGER STOP";
        private const string COMMAND_START = "START";
        private const string COMMAND_STOP = "STOP";

        //Threads for acquiring emg and acc data
        private Thread emgThread;
        private Thread accThread;

        //The constructor for our form
        public MainForm()
        {
            InitializeComponent();
        }

        //Connect button handler
        private void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                //Establish TCP/IP connection to server using URL entered
                commandSocket = new TcpClient(serverURL.Text, commandPort);

                //Set up communication streams
                commandStream = commandSocket.GetStream();
                commandReader = new StreamReader(commandStream, Encoding.ASCII);
                commandWriter = new StreamWriter(commandStream, Encoding.ASCII);

                //Get initial response from server and display
                responseLine.Text = commandReader.ReadLine();
                commandReader.ReadLine();   //get extra line terminator
                connected = true;   //iindicate that we are connected
                connectButton.Enabled = false;  //disable connect button
                quitButton.Enabled = true;  //enable quit button
            }
            catch (Exception connectException)
            {
                //connection failed, display error message
                MessageBox.Show("Could not connect.\n" + connectException.Message);
            }
        }

        //Quit button handler
        private void quitButton_Click(object sender, EventArgs e)
        {
            //Check if running and display error message if not
            if (running)
            {
                MessageBox.Show("Can't quit while acquiring data!");
                return;
            }

            //send QUIT command
            SendCommand(COMMAND_QUIT);

            connected = false;  //no longer connected
            connectButton.Enabled = true;   //enable connect button
            quitButton.Enabled = false; //disable quit button

            //Close all streams and connections
            commandReader.Close();
            commandWriter.Close();
            commandStream.Close();
            commandSocket.Close();
            emgStream.Close();
            emgSocket.Close();
            accStream.Close();
            accSocket.Close();
        }

        //Send a command to the server and get the response
        private string SendCommand(string command)
        {
            string response = "";

            //Check if connected
            if (connected)
            {
                //Send the command
                commandLine.Text = command;
                commandWriter.WriteLine(command);
                commandWriter.WriteLine();  //terminate command
                commandWriter.Flush();  //make sure command is sent immediately

                //Read the response line and display    
                response = commandReader.ReadLine();
                commandReader.ReadLine();   //get extra line terminator
                responseLine.Text = response;
            }
            else
                MessageBox.Show("Not connected.");
            return response;    //return the response we got
        }

        //Set start trigger button handler
        private void setStartTriggerButton_Click(object sender, EventArgs e)
        {
            //Get the state of the check box and send command
            string state;
            if (startTrigger.Checked)
                state = " ON";
            else
                state = " OFF";
            string response = SendCommand(COMMAND_SETSTARTTRIGGER + state);
        }

        //Set stop trigger button handler
        private void setStopTriggerButton_Click(object sender, EventArgs e)
        {
            //Get the state of the check box and send command
            string state;
            if (stopTrigger.Checked)
                state = " ON";
            else
                state = " OFF";
            string response = SendCommand(COMMAND_SETSTOPTRIGGER + state);
        }

        //Get triggers button handler
        private void getTriggers_Click(object sender, EventArgs e)
        {
            //Send command to get current trigger states
            string response = SendCommand(COMMAND_GETTRIGGERS);
            if (response == "")
                return;

            //Parse the response if not blank and set the state of the check boxes
            string[] words = response.Split(new char[] { ' ' });
            if (words[1] == "ON")
                startTrigger.Checked = true;
            else
                startTrigger.Checked = false;
            if (words[3] == "ON")
                stopTrigger.Checked = true;
            else
                stopTrigger.Checked = false;
        }

        //Start button handler
        private void startButton_Click(object sender, EventArgs e)
        {
            if (!connected)
            {
                MessageBox.Show("Not connected.");
                return;
            }

            //Clear stale data
            emgData = new float[16];
            accXData = new float[16];
            accYData = new float[16];
            accZData = new float[16];

            //Establish data connections and creat streams
            emgSocket = new TcpClient(serverURL.Text, emgPort);
            accSocket = new TcpClient(serverURL.Text, accPort);
            emgStream = emgSocket.GetStream();
            accStream = accSocket.GetStream();

            //Create data acquisition threads
            emgThread = new Thread(emgWorker);
            emgThread.IsBackground = true;
            accThread = new Thread(accWorker);
            accThread.IsBackground = true;

            //Indicate we are running and start up the acquisition threads
            running = true;
            emgThread.Start();
            accThread.Start();

            //Send start command to server to stream data
            string response = SendCommand(COMMAND_START);

            //check response
            if (response.StartsWith("OK"))
            {
                //Enable stop button and disable start button
                startButton.Enabled = false;
                stopButton.Enabled = true;

                //Start the UI update timer
                timer1.Start();
            }
            else
                running = false;    //stop threads
        }

        //Stop button handler
        private void stopButton_Click(object sender, EventArgs e)
        {
            running = false;    //no longer running
            //Wait for threads to terminate
            emgThread.Join();
            accThread.Join();

            //Send stop command to server
            string response = SendCommand(COMMAND_STOP);
            if (!response.StartsWith("OK"))
                MessageBox.Show("Server failed to stop. Further actions may fail.");

            //Enable start button and disable stop button
            startButton.Enabled = true;
            stopButton.Enabled = false;
            timer1.Stop();  //stop the UI update timer
        }

        //Thread for emg data acquisition
        private void emgWorker()
        {
            emgStream.ReadTimeout = 100;    //set timeout

            //Create a binary reader to read the data
            BinaryReader reader = new BinaryReader(emgStream);

            while (running)
            {
                try
                {
                    //Demultiplex the data and save for UI display
                    for (int sn = 0; sn < 16; ++sn)
                    {
                        emgData[sn] = reader.ReadSingle();
                    }
                }
                catch
                {
                    //ignore timeouts, but force a check of the running flag
                }
            }

            reader.Close(); //close the reader. This also disconnects
        }

        //Thread for acc data acquisition
        private void accWorker()
        {
            accStream.ReadTimeout = 100;    //set timeout

            //Create a binary reader to read the data
            BinaryReader reader = new BinaryReader(accStream);

            while (running)
            {
                try
                {
                    //Demultiplex the data and save for UI display
                    for (int sn = 0; sn < 16; ++sn)
                    {
                        accXData[sn] = reader.ReadSingle();
                        accYData[sn] = reader.ReadSingle();
                        accZData[sn] = reader.ReadSingle();
                    }
                }
                catch
                {
                    //ignore timeouts, but force a check of the running flag
                }
            }

            reader.Close(); //close the reader. This also disconnects
        }

        //UI update timer handler
        private void timer1_Tick(object sender, EventArgs e)
        {
            //Here we display the last values saved by the acquisition threads.
            //It should be noted that it is possible that the three acc samples may not
            //  correspond to the same point in time since no locking between threads is employed.
            //This sould be considered in a real application.

            emgDataDisplay.Text = emgData[(int)(sensorNumber.Value - 1)].ToString();
            accXDisplay.Text = accXData[(int)(sensorNumber.Value - 1)].ToString();
            accYDisplay.Text = accYData[(int)(sensorNumber.Value - 1)].ToString();
            accZDisplay.Text = accZData[(int)(sensorNumber.Value - 1)].ToString();

            //look for STOP from server
            if (commandStream.DataAvailable)
            {
                if ((responseLine.Text = commandReader.ReadLine()).StartsWith("STOPPED"))
                {
                    //A stop was received from te server.
                    commandReader.ReadLine();   //read extra line terminator

                    //Stop threads and wait for termination
                    running = false;
                    emgThread.Join();
                    accThread.Join();

                    //Enable start button and disable stop button
                    startButton.Enabled = true;
                    stopButton.Enabled = false;
                    commandLine.Text = "";  //clear command line display
                    timer1.Stop();  //stop the UI display update timer
                }
            }
        }
    }
}
