#region Copyright
/* 
 *  Copyright (C) 2011 Heerfordt
 *  Copyright (C) 2011 Oxan
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

using System;
using System.Text;
using System.Web;
using MPWebStream.Site.Service;

namespace MPWebStream.Site {
    public partial class Playlist : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            if (!Authentication.authenticate(HttpContext.Current))
                return;

            Configuration config = new Configuration();
            MediaStream remoteControl = new MediaStream();
            int transcoderId = 0;
            Int32.TryParse(HttpContext.Current.Request.Params["transcoder"], out transcoderId);
            int channelId = 0;
            Int32.TryParse(HttpContext.Current.Request.Params["channel"], out channelId);
            int recordingId = 0;
            Int32.TryParse(HttpContext.Current.Request.Params["recording"], out recordingId);
            
            Response.ContentType = "audio/x-mpegurl";
            Response.Write("#EXTM3U\r\n");
            if(recordingId == 0) {
                Response.Write(GetChannelPlayList(remoteControl, config, transcoderId, channelId));
            } else {
                Response.Write(GetRecordingPlayList(remoteControl, config, transcoderId, recordingId));
            }
            Response.End();
        }

        private string GetChannelPlayList(MediaStream remoteControl, Configuration config, int transcoderId, int channelId) {
            StringBuilder builder = new StringBuilder();
            foreach (Channel channel in remoteControl.GetChannels()) {
                // show all channels, except when channelId is given
                if (channelId != 0 && channel.IdChannel != channelId)
                    continue;

                foreach (Transcoder transcoder in remoteControl.GetTranscoders()) {
                    // show all transcoders, except when transcoderId is given
                    if (transcoderId != 0 && transcoder.Id != transcoderId)
                        continue;
                    string name = channel.Name + (transcoderId == 0 ? " (" + transcoder.Name + ")" : "");
                    string streamurl = remoteControl.GetTranscodedTvStreamUrl(channel.IdChannel, config.Username, config.Password, transcoder.Id);
                    WriteEntry(builder, name, streamurl);
                }
            }

            return builder.ToString();
        }

        private string GetRecordingPlayList(MediaStream remoteControl, Configuration config, int transcoderId, int recordingId) {
            StringBuilder builder = new StringBuilder();
            foreach (Recording rec in remoteControl.GetRecordings()) {
                if (recordingId != 0 && rec.Id != recordingId)
                    continue;

                foreach (Transcoder transcoder in remoteControl.GetTranscoders()) {
                    // show all transcoders, except when transcoderId is given
                    if (transcoderId != 0 && transcoder.Id != transcoderId)
                        continue;
                    string name = rec.Title + " (" + rec.StartTime.ToShortDateString() + " " + rec.StartTime.ToShortTimeString() +
                        (transcoderId == 0 ? ", " + transcoder.Name + ")" : ")");
                    string streamurl = remoteControl.GetTranscodedRecordingStreamUrl(rec.Id, config.Username, config.Password, transcoder.Id);
                    WriteEntry(builder, name, streamurl);
                }
            }

            return builder.ToString();
        }

        private void WriteEntry(StringBuilder builder, string name, string url) {
            builder.Append("#EXTINF:-1," + name);
            builder.Append("\r\n");
            builder.Append(url);
            builder.Append("\r\n\r\n");
        }
    }
}