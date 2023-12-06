namespace Paster
{
    partial class MultiDetector
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnConnect = new Button();
            txtIPAddr = new TextBox();
            label1 = new Label();
            label2 = new Label();
            txtPort = new TextBox();
            btnPreAcq = new Button();
            listStatus = new ListBox();
            btnServer = new Button();
            listBox1 = new ListBox();
            SuspendLayout();
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(8, 120);
            btnConnect.Margin = new Padding(2);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(293, 72);
            btnConnect.TabIndex = 0;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // txtIPAddr
            // 
            txtIPAddr.Location = new Point(66, 18);
            txtIPAddr.Margin = new Padding(2);
            txtIPAddr.Name = "txtIPAddr";
            txtIPAddr.Size = new Size(143, 23);
            txtIPAddr.TabIndex = 1;
            txtIPAddr.Text = "192.168.0.";
            txtIPAddr.TextChanged += txtIPAddr_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(8, 20);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(50, 15);
            label1.TabIndex = 2;
            label1.Text = "IP Addr:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(212, 20);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(32, 15);
            label2.TabIndex = 3;
            label2.Text = "Port:";
            // 
            // txtPort
            // 
            txtPort.Location = new Point(251, 20);
            txtPort.Margin = new Padding(2);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(52, 23);
            txtPort.TabIndex = 4;
            txtPort.Text = "5556";
            // 
            // btnPreAcq
            // 
            btnPreAcq.Location = new Point(7, 196);
            btnPreAcq.Margin = new Padding(2);
            btnPreAcq.Name = "btnPreAcq";
            btnPreAcq.Size = new Size(293, 72);
            btnPreAcq.TabIndex = 5;
            btnPreAcq.Text = "Prepare & Acquire";
            btnPreAcq.UseVisualStyleBackColor = true;
            btnPreAcq.Click += btnPreAcq_Click;
            // 
            // listStatus
            // 
            listStatus.FormattingEnabled = true;
            listStatus.ItemHeight = 15;
            listStatus.Location = new Point(7, 272);
            listStatus.Margin = new Padding(2);
            listStatus.Name = "listStatus";
            listStatus.Size = new Size(294, 139);
            listStatus.TabIndex = 6;
            // 
            // btnServer
            // 
            btnServer.Location = new Point(8, 47);
            btnServer.Margin = new Padding(2);
            btnServer.Name = "btnServer";
            btnServer.Size = new Size(293, 72);
            btnServer.TabIndex = 7;
            btnServer.Text = "Send Parm_Config";
            btnServer.UseVisualStyleBackColor = true;
            btnServer.Click += btnServer_Click;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(314, 53);
            listBox1.Margin = new Padding(2);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(564, 349);
            listBox1.TabIndex = 8;
            // 
            // MultiDetector
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(897, 492);
            Controls.Add(listBox1);
            Controls.Add(btnServer);
            Controls.Add(listStatus);
            Controls.Add(btnPreAcq);
            Controls.Add(txtPort);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtIPAddr);
            Controls.Add(btnConnect);
            Margin = new Padding(2);
            Name = "MultiDetector";
            Text = "Form1";
            FormClosing += MultiDetector_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnConnect;
        private TextBox txtIPAddr;
        private Label label1;
        private Label label2;
        private TextBox txtPort;
        private Button btnPreAcq;
        private ListBox listStatus;
        private Button btnServer;
        private ListBox listBox1;
    }
}