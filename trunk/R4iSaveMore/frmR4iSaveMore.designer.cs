namespace R4iSaveMore
{
    partial class frmR4iSaveMore
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmR4iSaveMore));
            this.grpSandbox = new System.Windows.Forms.GroupBox();
            this.pbDevice = new System.Windows.Forms.ProgressBar();
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnFWVer = new System.Windows.Forms.Button();
            this.lblFw = new System.Windows.Forms.Label();
            this.lblSaveSize = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.btnGameInfo = new System.Windows.Forms.Button();
            this.cmbPre = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblPre = new System.Windows.Forms.Label();
            this.numRead = new System.Windows.Forms.NumericUpDown();
            this.lblRead = new System.Windows.Forms.Label();
            this.txtSend = new System.Windows.Forms.TextBox();
            this.btnGo = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.grpSandbox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRead)).BeginInit();
            this.SuspendLayout();
            // 
            // grpSandbox
            // 
            this.grpSandbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSandbox.Controls.Add(this.pbDevice);
            this.grpSandbox.Controls.Add(this.btnDownload);
            this.grpSandbox.Controls.Add(this.btnFWVer);
            this.grpSandbox.Controls.Add(this.lblFw);
            this.grpSandbox.Controls.Add(this.lblSaveSize);
            this.grpSandbox.Controls.Add(this.lblName);
            this.grpSandbox.Controls.Add(this.btnGameInfo);
            this.grpSandbox.Controls.Add(this.cmbPre);
            this.grpSandbox.Controls.Add(this.label1);
            this.grpSandbox.Controls.Add(this.lblPre);
            this.grpSandbox.Controls.Add(this.numRead);
            this.grpSandbox.Controls.Add(this.lblRead);
            this.grpSandbox.Controls.Add(this.txtSend);
            this.grpSandbox.Controls.Add(this.btnGo);
            this.grpSandbox.Controls.Add(this.txtOutput);
            this.grpSandbox.Enabled = false;
            this.grpSandbox.Location = new System.Drawing.Point(12, 12);
            this.grpSandbox.Name = "grpSandbox";
            this.grpSandbox.Size = new System.Drawing.Size(500, 238);
            this.grpSandbox.TabIndex = 1;
            this.grpSandbox.TabStop = false;
            this.grpSandbox.Text = "R4i SaveDongle";
            // 
            // pbDevice
            // 
            this.pbDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pbDevice.Location = new System.Drawing.Point(6, 184);
            this.pbDevice.Name = "pbDevice";
            this.pbDevice.Size = new System.Drawing.Size(487, 15);
            this.pbDevice.TabIndex = 11;
            // 
            // btnDownload
            // 
            this.btnDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownload.Location = new System.Drawing.Point(383, 205);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(111, 27);
            this.btnDownload.TabIndex = 10;
            this.btnDownload.Text = "Download from Cart";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // btnFWVer
            // 
            this.btnFWVer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnFWVer.Location = new System.Drawing.Point(157, 205);
            this.btnFWVer.Name = "btnFWVer";
            this.btnFWVer.Size = new System.Drawing.Size(76, 27);
            this.btnFWVer.TabIndex = 9;
            this.btnFWVer.Text = "FW Version";
            this.btnFWVer.UseVisualStyleBackColor = true;
            this.btnFWVer.Click += new System.EventHandler(this.btnFWVer_Click);
            // 
            // lblFw
            // 
            this.lblFw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblFw.AutoSize = true;
            this.lblFw.Location = new System.Drawing.Point(239, 205);
            this.lblFw.Name = "lblFw";
            this.lblFw.Size = new System.Drawing.Size(51, 13);
            this.lblFw.TabIndex = 8;
            this.lblFw.Text = "<fw_ver>";
            // 
            // lblSaveSize
            // 
            this.lblSaveSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSaveSize.AutoSize = true;
            this.lblSaveSize.Location = new System.Drawing.Point(88, 219);
            this.lblSaveSize.Name = "lblSaveSize";
            this.lblSaveSize.Size = new System.Drawing.Size(63, 13);
            this.lblSaveSize.TabIndex = 8;
            this.lblSaveSize.Text = "<save size>";
            // 
            // lblName
            // 
            this.lblName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(88, 205);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(45, 13);
            this.lblName.TabIndex = 8;
            this.lblName.Text = "<name>";
            // 
            // btnGameInfo
            // 
            this.btnGameInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGameInfo.Location = new System.Drawing.Point(6, 205);
            this.btnGameInfo.Name = "btnGameInfo";
            this.btnGameInfo.Size = new System.Drawing.Size(76, 27);
            this.btnGameInfo.TabIndex = 7;
            this.btnGameInfo.Text = "Game Info";
            this.btnGameInfo.UseVisualStyleBackColor = true;
            this.btnGameInfo.Click += new System.EventHandler(this.btnGameInfo_Click);
            // 
            // cmbPre
            // 
            this.cmbPre.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbPre.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPre.FormattingEnabled = true;
            this.cmbPre.Items.AddRange(new object[] {
            "Check Version (receives 0xC0)",
            "Get Rom Header (receives 0x400)",
            "Clear (doesn\'t receive)",
            "Start Save Download (doesn\'t receive)",
            "Get media From save at offset 0 (receives 0x200)",
            "Stop Download (doesn\'t receive)"});
            this.cmbPre.Location = new System.Drawing.Point(71, 27);
            this.cmbPre.Name = "cmbPre";
            this.cmbPre.Size = new System.Drawing.Size(423, 21);
            this.cmbPre.TabIndex = 6;
            this.cmbPre.SelectedIndexChanged += new System.EventHandler(this.cmbPre_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Command:";
            // 
            // lblPre
            // 
            this.lblPre.AutoSize = true;
            this.lblPre.Location = new System.Drawing.Point(6, 30);
            this.lblPre.Name = "lblPre";
            this.lblPre.Size = new System.Drawing.Size(61, 13);
            this.lblPre.TabIndex = 5;
            this.lblPre.Text = "Predefined:";
            // 
            // numRead
            // 
            this.numRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numRead.Location = new System.Drawing.Point(355, 59);
            this.numRead.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numRead.Name = "numRead";
            this.numRead.Size = new System.Drawing.Size(58, 20);
            this.numRead.TabIndex = 4;
            // 
            // lblRead
            // 
            this.lblRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRead.AutoSize = true;
            this.lblRead.Location = new System.Drawing.Point(286, 63);
            this.lblRead.Name = "lblRead";
            this.lblRead.Size = new System.Drawing.Size(63, 13);
            this.lblRead.TabIndex = 3;
            this.lblRead.Text = "Read back:";
            // 
            // txtSend
            // 
            this.txtSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSend.Location = new System.Drawing.Point(69, 60);
            this.txtSend.MaxLength = 128;
            this.txtSend.Name = "txtSend";
            this.txtSend.Size = new System.Drawing.Size(211, 20);
            this.txtSend.TabIndex = 2;
            // 
            // btnGo
            // 
            this.btnGo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGo.Location = new System.Drawing.Point(419, 59);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(75, 20);
            this.btnGo.TabIndex = 1;
            this.btnGo.Text = "Send";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // txtOutput
            // 
            this.txtOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutput.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.txtOutput.Location = new System.Drawing.Point(6, 86);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtOutput.Size = new System.Drawing.Size(488, 90);
            this.txtOutput.TabIndex = 0;
            // 
            // frmR4iSaveMore
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 262);
            this.Controls.Add(this.grpSandbox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(540, 300);
            this.Name = "frmR4iSaveMore";
            this.Text = "R4iSaveMore";
            this.grpSandbox.ResumeLayout(false);
            this.grpSandbox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRead)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpSandbox;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.TextBox txtSend;
        private System.Windows.Forms.NumericUpDown numRead;
        private System.Windows.Forms.Label lblRead;
        private System.Windows.Forms.Label lblPre;
        private System.Windows.Forms.ComboBox cmbPre;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Button btnGameInfo;
        private System.Windows.Forms.Button btnFWVer;
        private System.Windows.Forms.Label lblFw;
        private System.Windows.Forms.Label lblSaveSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar pbDevice;
        private System.Windows.Forms.Button btnDownload;
    }
}