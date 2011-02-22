﻿using System;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Collections.Generic;
using TvLibrary.Log;

namespace MPWebStream.TvServerPlugin {
    public partial class ConfigurationInterface : SetupTv.SectionSettings {
        private LoggingConfiguration config;

        public ConfigurationInterface() {
            InitializeComponent();
            config = new LoggingConfiguration();
        }

        public override void OnSectionActivated() {
            // load settings
            port.Value = config.Port;
            useWebserver.Checked = config.UseWebserver;
            manageTV4Home.Checked = config.ManageTV4Home;
            requireAuthentication.Checked = config.EnableAuthentication;
            userName.Text = config.Username;
            password.Text = config.Password;
            siteroot.Text = config.SiteRoot;
            streamType.SelectedIndex = config.StreamType == Configuration.StreamlinkType.Direct ? 0 : 1; // TODO: can be done nicer I guess

            // show warning when TV4Home is not installed
            ServiceController controller = new ServiceController("TV4HomeCoreService");
            try {
                ServiceControllerStatus tmp = controller.Status;
                labelTV4HomeInstalled.Visible = false;
            } catch (InvalidOperationException) {
                labelTV4HomeInstalled.Visible = true;
            }

            // transcoders
            transcoders.Rows.Clear();
            foreach (TranscoderProfile profile in config.Transcoders) {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(transcoders);
                // doing it with string indexes doesn't work, don't know why
                row.Cells[0].Value = profile.Name;
                row.Cells[1].Value = profile.UseTranscoding;
                row.Cells[2].Value = profile.InputMethod;
                row.Cells[3].Value = profile.OutputMethod;
                row.Cells[4].Value = profile.Transcoder;
                row.Cells[5].Value = profile.Parameters;
                transcoders.Rows.Add(row);
            }

            base.OnSectionActivated();
        }

        public override void OnSectionDeActivated() {
            // save settings
            config.Port = (int)port.Value;
            config.UseWebserver = useWebserver.Checked;
            config.ManageTV4Home = manageTV4Home.Checked;
            config.Username = userName.Text;
            config.Password = password.Text;
            config.EnableAuthentication = requireAuthentication.Checked;
            config.SiteRoot = siteroot.Text;
            config.StreamType = streamType.SelectedIndex == 0 ? Configuration.StreamlinkType.Direct : Configuration.StreamlinkType.VLC;

            // transcoders
            config.Transcoders = new List<TranscoderProfile>();
            foreach (DataGridViewRow row in transcoders.Rows) {
                if(row.Cells[0].Value != null) 
                    config.Transcoders.Add(new TranscoderProfile() {
                        Name = row.Cells[0].Value != null ? row.Cells[0].Value.ToString() : "",
                        UseTranscoding = row.Cells[1].Value != null ? (bool)row.Cells[1].Value : false,
                        InputMethod = row.Cells[2].Value != null ? row.Cells[2].Value.ToString() : "", 
                        OutputMethod = row.Cells[3].Value != null ? row.Cells[3].Value.ToString() : "",
                        Transcoder = row.Cells[4].Value != null ? row.Cells[4].Value.ToString() : "",
                        Parameters = row.Cells[5].Value != null ? row.Cells[5].Value.ToString() : ""
                    });
            }

            config.Write();
            base.OnSectionDeActivated();
        }

        private void useWebserver_CheckedChanged(object sender, EventArgs e) {
            if (useWebserver.Checked) {
                port.Enabled = labelPort.Enabled = true;
            } else {
                port.Enabled = labelPort.Enabled = false;
            }
        }

        private void requireAuthentication_CheckedChanged(object sender, EventArgs e)
        {
            if (requireAuthentication.Checked) {
                userName.Enabled = true;
                password.Enabled = true;
            } else {
                userName.Enabled = false;
                password.Enabled = false;
            }
        }
    }
}
