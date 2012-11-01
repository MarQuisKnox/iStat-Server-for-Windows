namespace iStat_Server
{
    partial class Settings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            this.pinGroupLabel = new System.Windows.Forms.Label();
            this.pinText = new System.Windows.Forms.TextBox();
            this.instructionLabel = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.resetGroupLabel = new System.Windows.Forms.Label();
            this.resetAuthButton = new System.Windows.Forms.Button();
            this.portSaveButton = new System.Windows.Forms.Button();
            this.portInstructions = new System.Windows.Forms.Label();
            this.portText = new System.Windows.Forms.TextBox();
            this.portLabel = new System.Windows.Forms.Label();
            this.upnpSaveButton = new System.Windows.Forms.Button();
            this.upnpInstructions = new System.Windows.Forms.Label();
            this.upnpText = new System.Windows.Forms.TextBox();
            this.upnpCheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // pinGroupLabel
            // 
            this.pinGroupLabel.AutoSize = true;
            this.pinGroupLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pinGroupLabel.Location = new System.Drawing.Point(12, 9);
            this.pinGroupLabel.Name = "pinGroupLabel";
            this.pinGroupLabel.Size = new System.Drawing.Size(81, 21);
            this.pinGroupLabel.TabIndex = 1;
            this.pinGroupLabel.Text = "PIN CODE";
            // 
            // pinText
            // 
            this.pinText.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pinText.Location = new System.Drawing.Point(17, 52);
            this.pinText.Name = "pinText";
            this.pinText.Size = new System.Drawing.Size(146, 25);
            this.pinText.TabIndex = 2;
            this.pinText.TextChanged += new System.EventHandler(this.PinTextChanged);
            // 
            // instructionLabel
            // 
            this.instructionLabel.AutoSize = true;
            this.instructionLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.instructionLabel.Location = new System.Drawing.Point(14, 32);
            this.instructionLabel.Name = "instructionLabel";
            this.instructionLabel.Size = new System.Drawing.Size(123, 17);
            this.instructionLabel.TabIndex = 3;
            this.instructionLabel.Text = "Enter a 5 digit code";
            // 
            // saveButton
            // 
            this.saveButton.BackColor = System.Drawing.Color.Transparent;
            this.saveButton.Enabled = false;
            this.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveButton.Location = new System.Drawing.Point(179, 52);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 5;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = false;
            this.saveButton.Click += new System.EventHandler(this.SavePin);
            // 
            // resetGroupLabel
            // 
            this.resetGroupLabel.AutoSize = true;
            this.resetGroupLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resetGroupLabel.Location = new System.Drawing.Point(12, 249);
            this.resetGroupLabel.Name = "resetGroupLabel";
            this.resetGroupLabel.Size = new System.Drawing.Size(195, 21);
            this.resetGroupLabel.TabIndex = 6;
            this.resetGroupLabel.Text = "RESET AUTHORIZATIONS";
            // 
            // resetAuthButton
            // 
            this.resetAuthButton.BackColor = System.Drawing.Color.Transparent;
            this.resetAuthButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.resetAuthButton.Location = new System.Drawing.Point(179, 273);
            this.resetAuthButton.Name = "resetAuthButton";
            this.resetAuthButton.Size = new System.Drawing.Size(75, 23);
            this.resetAuthButton.TabIndex = 7;
            this.resetAuthButton.Text = "Reset";
            this.resetAuthButton.UseVisualStyleBackColor = false;
            this.resetAuthButton.Click += new System.EventHandler(this.ResetAuthButtonClick);
            // 
            // portSaveButton
            // 
            this.portSaveButton.BackColor = System.Drawing.Color.Transparent;
            this.portSaveButton.Enabled = false;
            this.portSaveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.portSaveButton.Location = new System.Drawing.Point(179, 132);
            this.portSaveButton.Name = "portSaveButton";
            this.portSaveButton.Size = new System.Drawing.Size(75, 23);
            this.portSaveButton.TabIndex = 12;
            this.portSaveButton.Text = "Save";
            this.portSaveButton.UseVisualStyleBackColor = false;
            this.portSaveButton.Click += new System.EventHandler(this.SavePort);
            // 
            // portInstructions
            // 
            this.portInstructions.AutoSize = true;
            this.portInstructions.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.portInstructions.Location = new System.Drawing.Point(14, 112);
            this.portInstructions.Name = "portInstructions";
            this.portInstructions.Size = new System.Drawing.Size(203, 17);
            this.portInstructions.TabIndex = 10;
            this.portInstructions.Text = "Enter a port between 1024-65535";
            this.portInstructions.Click += new System.EventHandler(this.label2_Click);
            // 
            // portText
            // 
            this.portText.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.portText.Location = new System.Drawing.Point(17, 132);
            this.portText.Name = "portText";
            this.portText.Size = new System.Drawing.Size(146, 25);
            this.portText.TabIndex = 9;
            this.portText.TextChanged += new System.EventHandler(this.PortTextChanged);
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.portLabel.Location = new System.Drawing.Point(12, 91);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(50, 21);
            this.portLabel.TabIndex = 8;
            this.portLabel.Text = "PORT";
            this.portLabel.Click += new System.EventHandler(this.label3_Click);
            // 
            // upnpSaveButton
            // 
            this.upnpSaveButton.BackColor = System.Drawing.Color.Transparent;
            this.upnpSaveButton.Enabled = false;
            this.upnpSaveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.upnpSaveButton.Location = new System.Drawing.Point(179, 209);
            this.upnpSaveButton.Name = "upnpSaveButton";
            this.upnpSaveButton.Size = new System.Drawing.Size(75, 23);
            this.upnpSaveButton.TabIndex = 16;
            this.upnpSaveButton.Text = "Save";
            this.upnpSaveButton.UseVisualStyleBackColor = false;
            this.upnpSaveButton.Click += new System.EventHandler(this.SaveUPNPPort);
            // 
            // upnpInstructions
            // 
            this.upnpInstructions.AutoSize = true;
            this.upnpInstructions.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.upnpInstructions.Location = new System.Drawing.Point(14, 189);
            this.upnpInstructions.Name = "upnpInstructions";
            this.upnpInstructions.Size = new System.Drawing.Size(203, 17);
            this.upnpInstructions.TabIndex = 15;
            this.upnpInstructions.Text = "Enter a port between 1024-65535";
            // 
            // upnpText
            // 
            this.upnpText.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.upnpText.Location = new System.Drawing.Point(17, 209);
            this.upnpText.Name = "upnpText";
            this.upnpText.Size = new System.Drawing.Size(146, 25);
            this.upnpText.TabIndex = 14;
            this.upnpText.TextChanged += new System.EventHandler(this.UPNPPortTextChanged);
            // 
            // upnpCheckbox
            // 
            this.upnpCheckbox.AutoSize = true;
            this.upnpCheckbox.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.upnpCheckbox.Location = new System.Drawing.Point(18, 166);
            this.upnpCheckbox.Name = "upnpCheckbox";
            this.upnpCheckbox.Size = new System.Drawing.Size(175, 25);
            this.upnpCheckbox.TabIndex = 17;
            this.upnpCheckbox.Text = "UPNP Port Mapping";
            this.upnpCheckbox.UseVisualStyleBackColor = true;
            this.upnpCheckbox.CheckStateChanged += new System.EventHandler(this.ToggleUPNP);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(284, 322);
            this.Controls.Add(this.upnpCheckbox);
            this.Controls.Add(this.upnpSaveButton);
            this.Controls.Add(this.upnpInstructions);
            this.Controls.Add(this.upnpText);
            this.Controls.Add(this.portSaveButton);
            this.Controls.Add(this.portInstructions);
            this.Controls.Add(this.portText);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.resetAuthButton);
            this.Controls.Add(this.resetGroupLabel);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.instructionLabel);
            this.Controls.Add(this.pinText);
            this.Controls.Add(this.pinGroupLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(290, 350);
            this.MinimumSize = new System.Drawing.Size(290, 350);
            this.Name = "Settings";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "iStat Server";
            this.Load += new System.EventHandler(this.Settings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label pinGroupLabel;
        private System.Windows.Forms.TextBox pinText;
        private System.Windows.Forms.Label instructionLabel;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Label resetGroupLabel;
        private System.Windows.Forms.Button resetAuthButton;
        private System.Windows.Forms.Button portSaveButton;
        private System.Windows.Forms.Label portInstructions;
        private System.Windows.Forms.TextBox portText;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.Button upnpSaveButton;
        private System.Windows.Forms.Label upnpInstructions;
        private System.Windows.Forms.TextBox upnpText;
        private System.Windows.Forms.CheckBox upnpCheckbox;
    }
}