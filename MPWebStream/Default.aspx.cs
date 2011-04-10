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

using MPWebStream.Site.Service;
using System;
using System.Web;
using System.Web.UI.WebControls;
using System.Collections.Specialized;

namespace MPWebStream.Site {
    public partial class Default : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            // this whole page probably qualifies for The Daily WTF, but I'm not a ASP.NET coder and I don't have enough time to learn it. Suggestions and patches welcome.
            if (!Authentication.authenticate(HttpContext.Current))
                return;

            Configuration config = new Configuration();
            MediaStream remoteControl = new MediaStream();
            SetupStreamTable(remoteControl, config);
            SetupRecordingTable(remoteControl, config);
        }

        private void SetupRecordingTable(MediaStream remoteControl, Configuration config) {
            // header
            TableHeaderRow hrow = new TableHeaderRow();
            TableHeaderCell dcell = new TableHeaderCell();
            dcell.Text = "Date";
            hrow.Cells.Add(dcell);
            TableHeaderCell tcell = new TableHeaderCell();
            tcell.Text = "Title";
            hrow.Cells.Add(tcell);
            foreach (Transcoder transcoder in remoteControl.GetTranscoders()) {
                TableHeaderCell ccell = new TableHeaderCell();
                ccell.Text = transcoder.Name;
                hrow.Cells.Add(ccell);
            }
            RecordingTable.Rows.Add(hrow);

            foreach (Recording rec in remoteControl.GetRecordings()) {
                // base cells
                TableRow row = new TableRow();
                TableCell date = new TableCell();
                date.Text = rec.StartTime.ToShortDateString() + " " + rec.StartTime.ToShortTimeString();
                row.Cells.Add(date);
                TableCell title = new TableCell();
                title.Text = rec.Title;
                row.Cells.Add(title);

                // all transcoders
                foreach (Transcoder transcoder in remoteControl.GetTranscoders()) {
                    TableCell item = new TableCell();

                    HyperLink direct = new HyperLink();
                    direct.NavigateUrl = remoteControl.GetTranscodedRecordingStreamUrl(rec.Id, config.Username, config.Password, transcoder.Id);
                    direct.Text = "Direct";
                    item.Controls.Add(direct);

                    // really 3SLOC for a simple dash? 
                    Label label = new Label();
                    label.Text = " - ";
                    item.Controls.Add(label);

                    HyperLink vlc = new HyperLink();
                    NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
                    queryString["recordingId"] = rec.Id.ToString();
                    queryString["transcoder"] = transcoder.Id.ToString();
                    vlc.NavigateUrl = "VLC.aspx?" + queryString.ToString();
                    vlc.Text = "VLC";
                    item.Controls.Add(vlc);

                    row.Cells.Add(item);
                }
                RecordingTable.Rows.Add(row);
            }
        }

        private void SetupStreamTable(MediaStream remoteControl, Configuration config) {
            // header row
            TableHeaderRow hrow = new TableHeaderRow();
            TableHeaderCell cell = new TableHeaderCell();
            cell.Text = "Channel";
            hrow.Cells.Add(cell);
            foreach (Transcoder transcoder in remoteControl.GetTranscoders()) {
                TableHeaderCell tcell = new TableHeaderCell();
                tcell.Text = transcoder.Name;
                hrow.Cells.Add(tcell);
            }
            StreamTable.Rows.Add(hrow);

            foreach (Channel channel in remoteControl.GetChannels()) {
                // channel name header cell
                TableRow row = new TableRow();
                TableCell title = new TableCell();
                title.Text = channel.DisplayName;
                row.Cells.Add(title);

                // all transcoders
                foreach (Transcoder transcoder in remoteControl.GetTranscoders()) {
                    TableCell item = new TableCell();

                    HyperLink direct = new HyperLink();
                    direct.NavigateUrl = remoteControl.GetTranscodedTvStreamUrl(channel.IdChannel, config.Username, config.Password, transcoder.Id);
                    direct.Text = "Direct";
                    item.Controls.Add(direct);

                    // really 3SLOC for a simple dash? 
                    Label label = new Label();
                    label.Text = " - ";
                    item.Controls.Add(label);

                    HyperLink vlc = new HyperLink();
                    NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
                    queryString["channel"] = channel.IdChannel.ToString();
                    queryString["transcoder"] = transcoder.Id.ToString();
                    vlc.NavigateUrl = "VLC.aspx?" + queryString.ToString();
                    vlc.Text = "VLC";
                    item.Controls.Add(vlc);

                    row.Cells.Add(item);
                }
                StreamTable.Rows.Add(row);
            }
        }
    }
}