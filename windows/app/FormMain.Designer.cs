namespace StylusForWindowsClient
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.buttonChangePort = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonUninstall = new System.Windows.Forms.Button();
            this.labelTPSlabel = new System.Windows.Forms.Label();
            this.labelTPS = new System.Windows.Forms.Label();
            this.labelSlowestlabel = new System.Windows.Forms.Label();
            this.labelSlowest = new System.Windows.Forms.Label();
            this.buttonReset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonChangePort
            // 
            this.buttonChangePort.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonChangePort.Location = new System.Drawing.Point(156, 5);
            this.buttonChangePort.Name = "buttonChangePort";
            this.buttonChangePort.Size = new System.Drawing.Size(123, 23);
            this.buttonChangePort.TabIndex = 2;
            this.buttonChangePort.Text = "Change port";
            this.buttonChangePort.UseVisualStyleBackColor = true;
            this.buttonChangePort.Click += new System.EventHandler(this.buttonChangePort_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(95, 7);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(55, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = "12333";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "UDP listen port";
            // 
            // buttonUninstall
            // 
            this.buttonUninstall.Location = new System.Drawing.Point(12, 33);
            this.buttonUninstall.Name = "buttonUninstall";
            this.buttonUninstall.Size = new System.Drawing.Size(267, 22);
            this.buttonUninstall.TabIndex = 3;
            this.buttonUninstall.Text = "Uninstall device and exit";
            this.buttonUninstall.UseVisualStyleBackColor = true;
            this.buttonUninstall.Click += new System.EventHandler(this.buttonUninstall_Click);
            // 
            // labelTPSlabel
            // 
            this.labelTPSlabel.AutoSize = true;
            this.labelTPSlabel.Location = new System.Drawing.Point(13, 61);
            this.labelTPSlabel.Name = "labelTPSlabel";
            this.labelTPSlabel.Size = new System.Drawing.Size(31, 13);
            this.labelTPSlabel.TabIndex = 4;
            this.labelTPSlabel.Text = "TPS:";
            // 
            // labelTPS
            // 
            this.labelTPS.AutoSize = true;
            this.labelTPS.Location = new System.Drawing.Point(66, 61);
            this.labelTPS.Name = "labelTPS";
            this.labelTPS.Size = new System.Drawing.Size(13, 13);
            this.labelTPS.TabIndex = 5;
            this.labelTPS.Text = "0";
            // 
            // labelSlowestlabel
            // 
            this.labelSlowestlabel.AutoSize = true;
            this.labelSlowestlabel.Location = new System.Drawing.Point(13, 74);
            this.labelSlowestlabel.Name = "labelSlowestlabel";
            this.labelSlowestlabel.Size = new System.Drawing.Size(47, 13);
            this.labelSlowestlabel.TabIndex = 6;
            this.labelSlowestlabel.Text = "Slowest:";
            // 
            // labelSlowest
            // 
            this.labelSlowest.AutoSize = true;
            this.labelSlowest.Location = new System.Drawing.Point(66, 74);
            this.labelSlowest.Name = "labelSlowest";
            this.labelSlowest.Size = new System.Drawing.Size(13, 13);
            this.labelSlowest.TabIndex = 7;
            this.labelSlowest.Text = "0";
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(156, 61);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(123, 26);
            this.buttonReset.TabIndex = 8;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 92);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.labelSlowest);
            this.Controls.Add(this.labelSlowestlabel);
            this.Controls.Add(this.labelTPS);
            this.Controls.Add(this.labelTPSlabel);
            this.Controls.Add(this.buttonUninstall);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.buttonChangePort);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "S-Pen Digitizer (beta)";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonChangePort;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonUninstall;
        private System.Windows.Forms.Label labelTPSlabel;
        private System.Windows.Forms.Label labelTPS;
        private System.Windows.Forms.Label labelSlowestlabel;
        private System.Windows.Forms.Label labelSlowest;
        private System.Windows.Forms.Button buttonReset;

    }
}

