namespace TrignoSDKExample
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.serverURL = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.commandLine = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.responseLine = new System.Windows.Forms.TextBox();
            this.quitButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.setStopTriggerButton = new System.Windows.Forms.Button();
            this.setStartTriggerButton = new System.Windows.Forms.Button();
            this.getTriggers = new System.Windows.Forms.Button();
            this.stopTrigger = new System.Windows.Forms.CheckBox();
            this.startTrigger = new System.Windows.Forms.CheckBox();
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.emgDataDisplay = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.sensorNumber = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.accXDisplay = new System.Windows.Forms.TextBox();
            this.accYDisplay = new System.Windows.Forms.TextBox();
            this.accZDisplay = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sensorNumber)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server URL:";
            // 
            // serverURL
            // 
            this.serverURL.Location = new System.Drawing.Point(85, 18);
            this.serverURL.Name = "serverURL";
            this.serverURL.Size = new System.Drawing.Size(122, 20);
            this.serverURL.TabIndex = 1;
            this.serverURL.Text = "localhost";
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(229, 17);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 2;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // commandLine
            // 
            this.commandLine.Location = new System.Drawing.Point(85, 376);
            this.commandLine.Name = "commandLine";
            this.commandLine.ReadOnly = true;
            this.commandLine.Size = new System.Drawing.Size(309, 20);
            this.commandLine.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 380);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Command:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 418);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Response:";
            // 
            // responseLine
            // 
            this.responseLine.Location = new System.Drawing.Point(85, 414);
            this.responseLine.Name = "responseLine";
            this.responseLine.ReadOnly = true;
            this.responseLine.Size = new System.Drawing.Size(309, 20);
            this.responseLine.TabIndex = 6;
            // 
            // quitButton
            // 
            this.quitButton.Enabled = false;
            this.quitButton.Location = new System.Drawing.Point(331, 17);
            this.quitButton.Name = "quitButton";
            this.quitButton.Size = new System.Drawing.Size(75, 23);
            this.quitButton.TabIndex = 7;
            this.quitButton.Text = "Quit";
            this.quitButton.UseVisualStyleBackColor = true;
            this.quitButton.Click += new System.EventHandler(this.quitButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.setStopTriggerButton);
            this.groupBox1.Controls.Add(this.setStartTriggerButton);
            this.groupBox1.Controls.Add(this.getTriggers);
            this.groupBox1.Controls.Add(this.stopTrigger);
            this.groupBox1.Controls.Add(this.startTrigger);
            this.groupBox1.Location = new System.Drawing.Point(19, 66);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(313, 109);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Triggers";
            // 
            // setStopTriggerButton
            // 
            this.setStopTriggerButton.Location = new System.Drawing.Point(210, 61);
            this.setStopTriggerButton.Name = "setStopTriggerButton";
            this.setStopTriggerButton.Size = new System.Drawing.Size(75, 23);
            this.setStopTriggerButton.TabIndex = 2;
            this.setStopTriggerButton.Text = "Set";
            this.setStopTriggerButton.UseVisualStyleBackColor = true;
            this.setStopTriggerButton.Click += new System.EventHandler(this.setStopTriggerButton_Click);
            // 
            // setStartTriggerButton
            // 
            this.setStartTriggerButton.Location = new System.Drawing.Point(210, 27);
            this.setStartTriggerButton.Name = "setStartTriggerButton";
            this.setStartTriggerButton.Size = new System.Drawing.Size(75, 23);
            this.setStartTriggerButton.TabIndex = 2;
            this.setStartTriggerButton.Text = "Set";
            this.setStartTriggerButton.UseVisualStyleBackColor = true;
            this.setStartTriggerButton.Click += new System.EventHandler(this.setStartTriggerButton_Click);
            // 
            // getTriggers
            // 
            this.getTriggers.Location = new System.Drawing.Point(111, 27);
            this.getTriggers.Name = "getTriggers";
            this.getTriggers.Size = new System.Drawing.Size(75, 23);
            this.getTriggers.TabIndex = 1;
            this.getTriggers.Text = "Get Triggers";
            this.getTriggers.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.getTriggers.UseVisualStyleBackColor = true;
            this.getTriggers.Click += new System.EventHandler(this.getTriggers_Click);
            // 
            // stopTrigger
            // 
            this.stopTrigger.AutoSize = true;
            this.stopTrigger.Location = new System.Drawing.Point(21, 64);
            this.stopTrigger.Name = "stopTrigger";
            this.stopTrigger.Size = new System.Drawing.Size(84, 17);
            this.stopTrigger.TabIndex = 0;
            this.stopTrigger.Text = "Stop Trigger";
            this.stopTrigger.UseVisualStyleBackColor = true;
            // 
            // startTrigger
            // 
            this.startTrigger.AutoSize = true;
            this.startTrigger.Location = new System.Drawing.Point(21, 30);
            this.startTrigger.Name = "startTrigger";
            this.startTrigger.Size = new System.Drawing.Size(84, 17);
            this.startTrigger.TabIndex = 0;
            this.startTrigger.Text = "Start Trigger";
            this.startTrigger.UseVisualStyleBackColor = true;
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(19, 182);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 9;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Enabled = false;
            this.stopButton.Location = new System.Drawing.Point(117, 182);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(75, 23);
            this.stopButton.TabIndex = 10;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // emgDataDisplay
            // 
            this.emgDataDisplay.Location = new System.Drawing.Point(73, 263);
            this.emgDataDisplay.Name = "emgDataDisplay";
            this.emgDataDisplay.ReadOnly = true;
            this.emgDataDisplay.Size = new System.Drawing.Size(135, 20);
            this.emgDataDisplay.TabIndex = 11;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // sensorNumber
            // 
            this.sensorNumber.Location = new System.Drawing.Point(109, 231);
            this.sensorNumber.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.sensorNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.sensorNumber.Name = "sensorNumber";
            this.sensorNumber.Size = new System.Drawing.Size(47, 20);
            this.sensorNumber.TabIndex = 12;
            this.sensorNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 235);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Sensor Number";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 267);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "EMG:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(23, 293);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "ACC X:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(23, 317);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "ACC Y:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(23, 343);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "ACC Z:";
            // 
            // accXDisplay
            // 
            this.accXDisplay.Location = new System.Drawing.Point(70, 289);
            this.accXDisplay.Name = "accXDisplay";
            this.accXDisplay.ReadOnly = true;
            this.accXDisplay.Size = new System.Drawing.Size(135, 20);
            this.accXDisplay.TabIndex = 11;
            // 
            // accYDisplay
            // 
            this.accYDisplay.Location = new System.Drawing.Point(70, 313);
            this.accYDisplay.Name = "accYDisplay";
            this.accYDisplay.ReadOnly = true;
            this.accYDisplay.Size = new System.Drawing.Size(135, 20);
            this.accYDisplay.TabIndex = 11;
            // 
            // accZDisplay
            // 
            this.accZDisplay.Location = new System.Drawing.Point(70, 339);
            this.accZDisplay.Name = "accZDisplay";
            this.accZDisplay.ReadOnly = true;
            this.accZDisplay.Size = new System.Drawing.Size(138, 20);
            this.accZDisplay.TabIndex = 11;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(522, 559);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.sensorNumber);
            this.Controls.Add(this.accZDisplay);
            this.Controls.Add(this.accYDisplay);
            this.Controls.Add(this.accXDisplay);
            this.Controls.Add(this.emgDataDisplay);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.quitButton);
            this.Controls.Add(this.responseLine);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.commandLine);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.serverURL);
            this.Controls.Add(this.label1);
            this.Name = "MainForm";
            this.Text = "Trigno SDK Example";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sensorNumber)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox serverURL;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.TextBox commandLine;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox responseLine;
        private System.Windows.Forms.Button quitButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button getTriggers;
        private System.Windows.Forms.CheckBox startTrigger;
        private System.Windows.Forms.Button setStartTriggerButton;
        private System.Windows.Forms.Button setStopTriggerButton;
        private System.Windows.Forms.CheckBox stopTrigger;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.TextBox emgDataDisplay;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.NumericUpDown sensorNumber;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox accXDisplay;
        private System.Windows.Forms.TextBox accYDisplay;
        private System.Windows.Forms.TextBox accZDisplay;
    }
}

