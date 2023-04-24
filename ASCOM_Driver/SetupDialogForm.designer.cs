namespace ASCOM.DarkSkyGeek
{
    partial class SetupDialogForm
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
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.chkTrace = new System.Windows.Forms.CheckBox();
            this.DSGLogo = new System.Windows.Forms.PictureBox();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.pairedDeviceAddrLbl = new System.Windows.Forms.Label();
            this.pairedDeviceAddrValue = new System.Windows.Forms.Label();
            this.compatDevicesLbl = new System.Windows.Forms.Label();
            this.deviceSelectionBtn = new System.Windows.Forms.Button();
            this.devicesListBox = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.DSGLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Image = global::ASCOM.DarkSkyGeek.Properties.Resources.icon_ok_24;
            this.cmdOK.Location = new System.Drawing.Point(241, 327);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(59, 36);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Image = global::ASCOM.DarkSkyGeek.Properties.Resources.icon_cancel_24;
            this.cmdCancel.Location = new System.Drawing.Point(306, 327);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(59, 36);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // chkTrace
            // 
            this.chkTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkTrace.AutoSize = true;
            this.chkTrace.Location = new System.Drawing.Point(12, 346);
            this.chkTrace.Name = "chkTrace";
            this.chkTrace.Size = new System.Drawing.Size(69, 17);
            this.chkTrace.TabIndex = 6;
            this.chkTrace.Text = "Trace on";
            this.chkTrace.UseVisualStyleBackColor = true;
            // 
            // DSGLogo
            // 
            this.DSGLogo.Image = global::ASCOM.DarkSkyGeek.Properties.Resources.darkskygeek;
            this.DSGLogo.Location = new System.Drawing.Point(12, 12);
            this.DSGLogo.Name = "DSGLogo";
            this.DSGLogo.Size = new System.Drawing.Size(88, 88);
            this.DSGLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.DSGLogo.TabIndex = 7;
            this.DSGLogo.TabStop = false;
            this.DSGLogo.Click += new System.EventHandler(this.BrowseToHomepage);
            this.DSGLogo.DoubleClick += new System.EventHandler(this.BrowseToHomepage);
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionLabel.Location = new System.Drawing.Point(107, 12);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(258, 88);
            this.descriptionLabel.TabIndex = 8;
            this.descriptionLabel.Text = "This driver wirelessly sends commands to the flat panel, which is a Bluetooth© Lo" +
    "w Energy (BLE) device. Please, ensure that your device is powered up before atte" +
    "mpting to connect to it.";
            // 
            // pairedDeviceAddrLbl
            // 
            this.pairedDeviceAddrLbl.AutoSize = true;
            this.pairedDeviceAddrLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pairedDeviceAddrLbl.Location = new System.Drawing.Point(12, 121);
            this.pairedDeviceAddrLbl.Name = "pairedDeviceAddrLbl";
            this.pairedDeviceAddrLbl.Size = new System.Drawing.Size(158, 13);
            this.pairedDeviceAddrLbl.TabIndex = 10;
            this.pairedDeviceAddrLbl.Text = "Currently paired device address:";
            // 
            // pairedDeviceAddrValue
            // 
            this.pairedDeviceAddrValue.AutoSize = true;
            this.pairedDeviceAddrValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pairedDeviceAddrValue.ForeColor = System.Drawing.Color.Red;
            this.pairedDeviceAddrValue.Location = new System.Drawing.Point(176, 121);
            this.pairedDeviceAddrValue.Name = "pairedDeviceAddrValue";
            this.pairedDeviceAddrValue.Size = new System.Drawing.Size(104, 13);
            this.pairedDeviceAddrValue.TabIndex = 11;
            this.pairedDeviceAddrValue.Text = "No device paired";
            // 
            // compatDevicesLbl
            // 
            this.compatDevicesLbl.AutoSize = true;
            this.compatDevicesLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.compatDevicesLbl.Location = new System.Drawing.Point(12, 149);
            this.compatDevicesLbl.Name = "compatDevicesLbl";
            this.compatDevicesLbl.Size = new System.Drawing.Size(233, 13);
            this.compatDevicesLbl.TabIndex = 12;
            this.compatDevicesLbl.Text = "Bluetooth address of nearby wireless flat panels:";
            // 
            // deviceSelectionBtn
            // 
            this.deviceSelectionBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.deviceSelectionBtn.Enabled = false;
            this.deviceSelectionBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.deviceSelectionBtn.Location = new System.Drawing.Point(15, 280);
            this.deviceSelectionBtn.Name = "deviceSelectionBtn";
            this.deviceSelectionBtn.Size = new System.Drawing.Size(350, 30);
            this.deviceSelectionBtn.TabIndex = 14;
            this.deviceSelectionBtn.Text = "Pair with selected device";
            this.deviceSelectionBtn.UseVisualStyleBackColor = true;
            this.deviceSelectionBtn.Click += new System.EventHandler(this.deviceSelectionBtn_Click);
            // 
            // devicesListBox
            // 
            this.devicesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.devicesListBox.FormattingEnabled = true;
            this.devicesListBox.Location = new System.Drawing.Point(15, 175);
            this.devicesListBox.Name = "devicesListBox";
            this.devicesListBox.Size = new System.Drawing.Size(350, 95);
            this.devicesListBox.TabIndex = 15;
            this.devicesListBox.SelectedIndexChanged += new System.EventHandler(this.devicesListBox_SelectedIndexChanged);
            // 
            // SetupDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(375, 371);
            this.Controls.Add(this.devicesListBox);
            this.Controls.Add(this.deviceSelectionBtn);
            this.Controls.Add(this.compatDevicesLbl);
            this.Controls.Add(this.pairedDeviceAddrValue);
            this.Controls.Add(this.pairedDeviceAddrLbl);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.DSGLogo);
            this.Controls.Add(this.chkTrace);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupDialogForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DarkSkyGeek’s Wireless Flat Panel";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SetupDialogForm_FormClosed);
            this.Load += new System.EventHandler(this.SetupDialogForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.DSGLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.CheckBox chkTrace;
        private System.Windows.Forms.PictureBox DSGLogo;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.Label pairedDeviceAddrLbl;
        private System.Windows.Forms.Label pairedDeviceAddrValue;
        private System.Windows.Forms.Label compatDevicesLbl;
        private System.Windows.Forms.Button deviceSelectionBtn;
        private System.Windows.Forms.ListBox devicesListBox;
    }
}