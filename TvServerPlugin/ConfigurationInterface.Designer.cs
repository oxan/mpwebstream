﻿namespace MPWebStream.TvServerPlugin {
    partial class ConfigurationInterface {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.requireAuthentication = new System.Windows.Forms.CheckBox();
            this.labelRequireAuthentication = new System.Windows.Forms.Label();
            this.password = new System.Windows.Forms.TextBox();
            this.userName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.port = new System.Windows.Forms.NumericUpDown();
            this.labelPort = new System.Windows.Forms.Label();
            this.useWebserver = new System.Windows.Forms.CheckBox();
            this.labelUseWebserverExplain = new System.Windows.Forms.Label();
            this.labelUseWebserver = new System.Windows.Forms.Label();
            this.manageTV4Home = new System.Windows.Forms.CheckBox();
            this.labelManageTV4HomeExplain = new System.Windows.Forms.Label();
            this.labelManageTV4Home = new System.Windows.Forms.Label();
            this.tabTranscoding = new System.Windows.Forms.TabPage();
            this.transcoders = new System.Windows.Forms.DataGridView();
            this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.transcode = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.inputMethod = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.outputMethod = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.transcoder = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.parameters = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabControl.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.port)).BeginInit();
            this.tabTranscoding.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.transcoders)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabGeneral);
            this.tabControl.Controls.Add(this.tabTranscoding);
            this.tabControl.Location = new System.Drawing.Point(3, 3);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(757, 404);
            this.tabControl.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.requireAuthentication);
            this.tabGeneral.Controls.Add(this.labelRequireAuthentication);
            this.tabGeneral.Controls.Add(this.password);
            this.tabGeneral.Controls.Add(this.userName);
            this.tabGeneral.Controls.Add(this.label2);
            this.tabGeneral.Controls.Add(this.label1);
            this.tabGeneral.Controls.Add(this.port);
            this.tabGeneral.Controls.Add(this.labelPort);
            this.tabGeneral.Controls.Add(this.useWebserver);
            this.tabGeneral.Controls.Add(this.labelUseWebserverExplain);
            this.tabGeneral.Controls.Add(this.labelUseWebserver);
            this.tabGeneral.Controls.Add(this.manageTV4Home);
            this.tabGeneral.Controls.Add(this.labelManageTV4HomeExplain);
            this.tabGeneral.Controls.Add(this.labelManageTV4Home);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(261, 241);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // requireAuthentication
            // 
            this.requireAuthentication.AutoSize = true;
            this.requireAuthentication.Checked = true;
            this.requireAuthentication.CheckState = System.Windows.Forms.CheckState.Checked;
            this.requireAuthentication.Location = new System.Drawing.Point(183, 155);
            this.requireAuthentication.Name = "requireAuthentication";
            this.requireAuthentication.Size = new System.Drawing.Size(15, 14);
            this.requireAuthentication.TabIndex = 13;
            this.requireAuthentication.UseVisualStyleBackColor = true;
            this.requireAuthentication.CheckedChanged += new System.EventHandler(this.requireAuthentication_CheckedChanged);
            // 
            // labelRequireAuthentication
            // 
            this.labelRequireAuthentication.AutoSize = true;
            this.labelRequireAuthentication.Location = new System.Drawing.Point(7, 156);
            this.labelRequireAuthentication.Name = "labelRequireAuthentication";
            this.labelRequireAuthentication.Size = new System.Drawing.Size(117, 13);
            this.labelRequireAuthentication.TabIndex = 12;
            this.labelRequireAuthentication.Text = "Require authentication:";
            // 
            // password
            // 
            this.password.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.password.Location = new System.Drawing.Point(183, 201);
            this.password.Name = "password";
            this.password.PasswordChar = '*';
            this.password.Size = new System.Drawing.Size(72, 20);
            this.password.TabIndex = 11;
            // 
            // userName
            // 
            this.userName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.userName.Location = new System.Drawing.Point(183, 175);
            this.userName.Name = "userName";
            this.userName.Size = new System.Drawing.Size(72, 20);
            this.userName.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 204);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Password:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 178);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Username:";
            // 
            // port
            // 
            this.port.Location = new System.Drawing.Point(183, 124);
            this.port.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.port.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(57, 20);
            this.port.TabIndex = 7;
            this.port.Value = new decimal(new int[] {
            8080,
            0,
            0,
            0});
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(7, 126);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(26, 13);
            this.labelPort.TabIndex = 6;
            this.labelPort.Text = "Port";
            // 
            // useWebserver
            // 
            this.useWebserver.AutoSize = true;
            this.useWebserver.Checked = true;
            this.useWebserver.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useWebserver.Location = new System.Drawing.Point(183, 58);
            this.useWebserver.Name = "useWebserver";
            this.useWebserver.Size = new System.Drawing.Size(15, 14);
            this.useWebserver.TabIndex = 5;
            this.useWebserver.UseVisualStyleBackColor = true;
            this.useWebserver.CheckedChanged += new System.EventHandler(this.useWebserver_CheckedChanged);
            // 
            // labelUseWebserverExplain
            // 
            this.labelUseWebserverExplain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelUseWebserverExplain.Location = new System.Drawing.Point(7, 75);
            this.labelUseWebserverExplain.Name = "labelUseWebserverExplain";
            this.labelUseWebserverExplain.Size = new System.Drawing.Size(248, 47);
            this.labelUseWebserverExplain.TabIndex = 4;
            this.labelUseWebserverExplain.Text = "Disable this if you use some other webserver (such as IIS) for hosting the websit" +
                "e (note: only for expert users, enable if you\'re not sure).";
            // 
            // labelUseWebserver
            // 
            this.labelUseWebserver.AutoSize = true;
            this.labelUseWebserver.Location = new System.Drawing.Point(7, 59);
            this.labelUseWebserver.Name = "labelUseWebserver";
            this.labelUseWebserver.Size = new System.Drawing.Size(131, 13);
            this.labelUseWebserver.TabIndex = 3;
            this.labelUseWebserver.Text = "Use integrated webserver:";
            // 
            // manageTV4Home
            // 
            this.manageTV4Home.AutoSize = true;
            this.manageTV4Home.Location = new System.Drawing.Point(183, 6);
            this.manageTV4Home.Name = "manageTV4Home";
            this.manageTV4Home.Size = new System.Drawing.Size(15, 14);
            this.manageTV4Home.TabIndex = 2;
            this.manageTV4Home.UseVisualStyleBackColor = true;
            // 
            // labelManageTV4HomeExplain
            // 
            this.labelManageTV4HomeExplain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelManageTV4HomeExplain.Location = new System.Drawing.Point(7, 24);
            this.labelManageTV4HomeExplain.Name = "labelManageTV4HomeExplain";
            this.labelManageTV4HomeExplain.Size = new System.Drawing.Size(248, 31);
            this.labelManageTV4HomeExplain.TabIndex = 1;
            this.labelManageTV4HomeExplain.Text = "Disable this if you want to keep the TV4Home Core Service running after the TV Se" +
                "rver stops.";
            // 
            // labelManageTV4Home
            // 
            this.labelManageTV4Home.AutoSize = true;
            this.labelManageTV4Home.Location = new System.Drawing.Point(7, 7);
            this.labelManageTV4Home.Name = "labelManageTV4Home";
            this.labelManageTV4Home.Size = new System.Drawing.Size(132, 13);
            this.labelManageTV4Home.TabIndex = 0;
            this.labelManageTV4Home.Text = "Manage TV4Home server:";
            // 
            // tabTranscoding
            // 
            this.tabTranscoding.Controls.Add(this.transcoders);
            this.tabTranscoding.Location = new System.Drawing.Point(4, 22);
            this.tabTranscoding.Name = "tabTranscoding";
            this.tabTranscoding.Padding = new System.Windows.Forms.Padding(3);
            this.tabTranscoding.Size = new System.Drawing.Size(749, 378);
            this.tabTranscoding.TabIndex = 1;
            this.tabTranscoding.Text = "Transcoding";
            this.tabTranscoding.UseVisualStyleBackColor = true;
            // 
            // transcoders
            // 
            this.transcoders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.transcoders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.transcoders.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.name,
            this.transcode,
            this.inputMethod,
            this.outputMethod,
            this.transcoder,
            this.parameters});
            this.transcoders.Location = new System.Drawing.Point(7, 7);
            this.transcoders.Name = "transcoders";
            this.transcoders.Size = new System.Drawing.Size(736, 365);
            this.transcoders.TabIndex = 0;
            // 
            // name
            // 
            this.name.HeaderText = "Name";
            this.name.Name = "name";
            // 
            // transcode
            // 
            this.transcode.HeaderText = "Transcode";
            this.transcode.Name = "transcode";
            this.transcode.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.transcode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // inputMethod
            // 
            this.inputMethod.HeaderText = "Input method";
            this.inputMethod.Items.AddRange(new object[] {
            "Filename",
            "NamedPipe",
            "StandardIn"});
            this.inputMethod.Name = "inputMethod";
            this.inputMethod.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.inputMethod.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // outputMethod
            // 
            this.outputMethod.HeaderText = "Output method";
            this.outputMethod.Items.AddRange(new object[] {
            "NamedPipe",
            "StandardOut"});
            this.outputMethod.Name = "outputMethod";
            this.outputMethod.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.outputMethod.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // transcoder
            // 
            this.transcoder.HeaderText = "Transcoder";
            this.transcoder.Name = "transcoder";
            this.transcoder.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // parameters
            // 
            this.parameters.HeaderText = "Parameters";
            this.parameters.Name = "parameters";
            // 
            // ConfigurationInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl);
            this.MinimumSize = new System.Drawing.Size(275, 270);
            this.Name = "ConfigurationInterface";
            this.Size = new System.Drawing.Size(763, 407);
            this.tabControl.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.port)).EndInit();
            this.tabTranscoding.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.transcoders)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.NumericUpDown port;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.CheckBox useWebserver;
        private System.Windows.Forms.Label labelUseWebserverExplain;
        private System.Windows.Forms.Label labelUseWebserver;
        private System.Windows.Forms.CheckBox manageTV4Home;
        private System.Windows.Forms.Label labelManageTV4HomeExplain;
        private System.Windows.Forms.Label labelManageTV4Home;
        private System.Windows.Forms.TextBox password;
        private System.Windows.Forms.TextBox userName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox requireAuthentication;
        private System.Windows.Forms.Label labelRequireAuthentication;
        private System.Windows.Forms.TabPage tabTranscoding;
        private System.Windows.Forms.DataGridView transcoders;
        private System.Windows.Forms.DataGridViewTextBoxColumn name;
        private System.Windows.Forms.DataGridViewCheckBoxColumn transcode;
        private System.Windows.Forms.DataGridViewComboBoxColumn inputMethod;
        private System.Windows.Forms.DataGridViewComboBoxColumn outputMethod;
        private System.Windows.Forms.DataGridViewTextBoxColumn transcoder;
        private System.Windows.Forms.DataGridViewTextBoxColumn parameters;
    }
}