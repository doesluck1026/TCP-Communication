
namespace TCP_Server
{
    partial class Form1
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
            this.lbl_LED = new System.Windows.Forms.Label();
            this.btn_StartServer = new System.Windows.Forms.Button();
            this.txt_ServerIP = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lbl_ClientIP = new System.Windows.Forms.Label();
            this.txt_ClientMessage = new System.Windows.Forms.TextBox();
            this.trackbar_progress = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.trackbar_progress)).BeginInit();
            this.SuspendLayout();
            // 
            // lbl_LED
            // 
            this.lbl_LED.BackColor = System.Drawing.Color.Black;
            this.lbl_LED.Location = new System.Drawing.Point(12, 9);
            this.lbl_LED.Name = "lbl_LED";
            this.lbl_LED.Size = new System.Drawing.Size(100, 86);
            this.lbl_LED.TabIndex = 0;
            this.lbl_LED.Text = "label1";
            // 
            // btn_StartServer
            // 
            this.btn_StartServer.Location = new System.Drawing.Point(178, 146);
            this.btn_StartServer.Name = "btn_StartServer";
            this.btn_StartServer.Size = new System.Drawing.Size(97, 23);
            this.btn_StartServer.TabIndex = 1;
            this.btn_StartServer.Text = "Start Server";
            this.btn_StartServer.UseVisualStyleBackColor = true;
            this.btn_StartServer.Click += new System.EventHandler(this.btn_StartServer_Click);
            // 
            // txt_ServerIP
            // 
            this.txt_ServerIP.Location = new System.Drawing.Point(72, 148);
            this.txt_ServerIP.Name = "txt_ServerIP";
            this.txt_ServerIP.Size = new System.Drawing.Size(100, 20);
            this.txt_ServerIP.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 151);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Server IP:";
            // 
            // lbl_ClientIP
            // 
            this.lbl_ClientIP.AutoSize = true;
            this.lbl_ClientIP.Location = new System.Drawing.Point(281, 151);
            this.lbl_ClientIP.Name = "lbl_ClientIP";
            this.lbl_ClientIP.Size = new System.Drawing.Size(46, 13);
            this.lbl_ClientIP.TabIndex = 4;
            this.lbl_ClientIP.Text = "Client IP";
            // 
            // txt_ClientMessage
            // 
            this.txt_ClientMessage.Location = new System.Drawing.Point(134, 9);
            this.txt_ClientMessage.Multiline = true;
            this.txt_ClientMessage.Name = "txt_ClientMessage";
            this.txt_ClientMessage.ReadOnly = true;
            this.txt_ClientMessage.Size = new System.Drawing.Size(316, 86);
            this.txt_ClientMessage.TabIndex = 5;
            // 
            // trackbar_progress
            // 
            this.trackbar_progress.Location = new System.Drawing.Point(471, 9);
            this.trackbar_progress.Name = "trackbar_progress";
            this.trackbar_progress.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackbar_progress.RightToLeftLayout = true;
            this.trackbar_progress.Size = new System.Drawing.Size(45, 160);
            this.trackbar_progress.TabIndex = 6;
            this.trackbar_progress.TickStyle = System.Windows.Forms.TickStyle.Both;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(551, 203);
            this.Controls.Add(this.trackbar_progress);
            this.Controls.Add(this.txt_ClientMessage);
            this.Controls.Add(this.lbl_ClientIP);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_ServerIP);
            this.Controls.Add(this.btn_StartServer);
            this.Controls.Add(this.lbl_LED);
            this.Name = "Form1";
            this.Text = "TCP Server";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackbar_progress)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_LED;
        private System.Windows.Forms.Button btn_StartServer;
        private System.Windows.Forms.TextBox txt_ServerIP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbl_ClientIP;
        private System.Windows.Forms.TextBox txt_ClientMessage;
        private System.Windows.Forms.TrackBar trackbar_progress;
    }
}

