#region Copyright
/* 
 *  Copyright (C) 2010, 2011 Oxan
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion

using MPWebStream.MediaTranscoding;
using System;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Collections.Generic;
using TvLibrary.Log;

namespace MPWebStream.TvServerPlugin {
    public partial class ConfigurationInterface : SetupTv.SectionSettings {
        private LoggingConfiguration config;
        private int lastTranscoderId = 1;

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
            logTranscoder.Checked = config.TranscoderLog;

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
                row.Tag = profile.Id;
                lastTranscoderId = Math.Max(profile.Id, lastTranscoderId);
                // doing it with string indexes doesn't work, don't know why
                row.Cells[0].Value = profile.Name;
                row.Cells[1].Value = profile.UseTranscoding;
                row.Cells[2].Value = Enum.GetName(typeof(TransportMethod), profile.InputMethod);
                row.Cells[3].Value = Enum.GetName(typeof(TransportMethod), profile.OutputMethod);
                row.Cells[4].Value = profile.Transcoder;
                row.Cells[5].Value = profile.Parameters;
                row.Cells[6].Value = profile.MIME == null || profile.MIME == string.Empty ? "video/MP2T" : profile.MIME;
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
            config.TranscoderLog = logTranscoder.Checked;

            // transcoders
            config.Transcoders = new List<TranscoderProfile>();
            foreach (DataGridViewRow row in transcoders.Rows) {
                if(row.Cells[0].Value != null) {
                    if (row.Tag == null)
                        row.Tag = ++lastTranscoderId;
                    config.Transcoders.Add(new TranscoderProfile() {
                        Name = row.Cells[0].Value != null ? row.Cells[0].Value.ToString() : "",
                        UseTranscoding = row.Cells[1].Value != null ? (bool)row.Cells[1].Value : false,
                        InputMethod = row.Cells[2].Value != null ? (TransportMethod)Enum.Parse(typeof(TransportMethod), row.Cells[2].Value.ToString(), true) : TransportMethod.NamedPipe,
                        OutputMethod = row.Cells[3].Value != null ? (TransportMethod)Enum.Parse(typeof(TransportMethod), row.Cells[3].Value.ToString(), true) : TransportMethod.NamedPipe,
                        Transcoder = row.Cells[4].Value != null ? row.Cells[4].Value.ToString() : "",
                        Parameters = row.Cells[5].Value != null ? row.Cells[5].Value.ToString() : "",
                        Id = (int)row.Tag,
                        MIME = row.Cells[6].Value != null ? row.Cells[6].Value.ToString() : "",
                    });
                }
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
