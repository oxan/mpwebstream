using System;
using System.Windows.Forms;
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

            // transcoders
            transcoders.Rows.Clear();
            foreach (TranscoderProfile profile in config.Transcoders) {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(transcoders);
                row.Cells["name"].Value = profile.Name;
                row.Cells["transcode"].Value = profile.UseTranscoding;
                row.Cells["inputMethod"].Value = profile.InputMethod;
                row.Cells["outputMethod"].Value = profile.OutputMethod;
                row.Cells["transcoder"].Value = profile.Transcoder;
                row.Cells["parameters"].Value = profile.Parameters;
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
                if(row.Cells["name"].Value != null) 
                    config.Transcoders.Add(new TranscoderProfile() {
                        Name = row.Cells["name"].Value != null ? row.Cells["name"].Value.ToString() : "",
                        UseTranscoding = row.Cells["transcode"].Value != null ? (bool)row.Cells["transcode"].Value : false,
                        InputMethod = row.Cells["inputMethod"].Value != null ? row.Cells["inputMethod"].Value.ToString() : "", 
                        OutputMethod = row.Cells["outputMethod"].Value != null ? row.Cells["outputMethod"].Value.ToString() : "",
                        Transcoder = row.Cells["transcoder"].Value != null ? row.Cells["transcoder"].Value.ToString() : "",
                        Parameters = row.Cells["parameters"].Value != null ? row.Cells["parameters"].Value.ToString() : ""
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
