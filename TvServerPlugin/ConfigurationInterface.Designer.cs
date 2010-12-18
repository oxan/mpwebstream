namespace MPWebStream.TvServerPlugin {
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
            this.port = new System.Windows.Forms.NumericUpDown();
            this.labelPort = new System.Windows.Forms.Label();
            this.useWebserver = new System.Windows.Forms.CheckBox();
            this.labelUseWebserverExplain = new System.Windows.Forms.Label();
            this.labelUseWebserver = new System.Windows.Forms.Label();
            this.manageTV4Home = new System.Windows.Forms.CheckBox();
            this.labelManageTV4HomeExplain = new System.Windows.Forms.Label();
            this.labelManageTV4Home = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.userName = new System.Windows.Forms.TextBox();
            this.password = new System.Windows.Forms.TextBox();
            this.tabControl.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.port)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabGeneral);
            this.tabControl.Location = new System.Drawing.Point(3, 3);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(269, 247);
            this.tabControl.TabIndex = 0;
            // 
            // tabGeneral
            // 
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
            this.tabGeneral.Size = new System.Drawing.Size(261, 221);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 151);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Username:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 180);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Password:";
            // 
            // userName
            // 
            this.userName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.userName.Location = new System.Drawing.Point(183, 150);
            this.userName.Name = "userName";
            this.userName.Size = new System.Drawing.Size(72, 20);
            this.userName.TabIndex = 10;
            // 
            // password
            // 
            this.password.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.password.Location = new System.Drawing.Point(183, 177);
            this.password.Name = "password";
            this.password.PasswordChar = '*';
            this.password.Size = new System.Drawing.Size(72, 20);
            this.password.TabIndex = 11;
            // 
            // ConfigurationInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl);
            this.MinimumSize = new System.Drawing.Size(275, 250);
            this.Name = "ConfigurationInterface";
            this.Size = new System.Drawing.Size(275, 250);
            this.tabControl.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.port)).EndInit();
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
    }
}