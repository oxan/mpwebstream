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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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

            LogPath.Text = Log.LogPath;
            Config.Text = config.ConfigPath;
            TranscoderLogPath.Text = System.IO.Path.Combine(config.BasePath, "transcoderlogs");
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
                ccell.Text = transcoder.Name.Replace(",", ",<br />");
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
                    item.Text = createCellContents(remoteControl.GetTranscodedRecordingStreamUrl(rec.Id, config.Username, config.Password, transcoder.Id), "recording", rec.Id.ToString(), transcoder);
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
                tcell.Text = transcoder.Name.Replace(",", ",<br />");
                hrow.Cells.Add(tcell);
            }
            StreamTable.Rows.Add(hrow);

            // link to the playlist
            TableRow prow = new TableRow();
            prow.Cells.Add(new TableCell());
            foreach (Transcoder transcoder in remoteControl.GetTranscoders()) {
                TableCell pcell = new TableCell();
                HyperLink link = new HyperLink();
                link.NavigateUrl = "Playlist.aspx?transcoder=" + transcoder.Id.ToString();
                link.Text = "Playlist";
                pcell.Controls.Add(link);
                prow.Cells.Add(pcell);
            }
            StreamTable.Rows.Add(prow);

            foreach (Channel channel in remoteControl.GetChannels()) {
                // channel name header cell
                TableRow row = new TableRow();
                TableCell title = new TableCell();
                title.Text = channel.DisplayName;
                row.Cells.Add(title);

                // all transcoders
                foreach (Transcoder transcoder in remoteControl.GetTranscoders()) {
                    TableCell item = new TableCell();
                    item.Text = createCellContents(remoteControl.GetTranscodedTvStreamUrl(channel.IdChannel, config.Username, config.Password, transcoder.Id), "channel", channel.IdChannel.ToString(), transcoder);
                    row.Cells.Add(item);
                }
                StreamTable.Rows.Add(row);
            }
        }

        private string createCellContents(string directURL, string queryStringKey, string queryStringValue, Transcoder transcoder) {
            StringBuilder output = new StringBuilder();
            HtmlTextWriter writer = new HtmlTextWriter(new System.IO.StringWriter(output));

            // direct
            HyperLink direct = new HyperLink();
            direct.NavigateUrl = directURL;
            direct.Text = "Direct";
            direct.RenderControl(writer);

            // dynamic stream types
            Dictionary<string, string> pages = new Dictionary<string, string>();
            pages["VLC"] = "VLC.aspx";
            pages["M3U"] = "Playlist.aspx";
            pages["HTML5"] = "HTML5.aspx";

            // and render them
            int i = 1;
            foreach (KeyValuePair<string, string> page in pages) {
                HyperLink link = new HyperLink();
                NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString["transcoder"] = transcoder.Id.ToString();
                queryString[queryStringKey] = queryStringValue;
                link.NavigateUrl = page.Value + "?" + queryString.ToString();
                link.Text = page.Key;
                output.Append(i % 2 == 1 ? " - " : "<br />");
                link.RenderControl(writer);
                i++;
            }

            return output.ToString();
        }
    }
}