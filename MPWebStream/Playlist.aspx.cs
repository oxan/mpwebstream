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
using System.Text;

namespace MPWebStream.Site {
    public partial class Playlist : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            // this whole page probably qualifies for The Daily WTF, but I'm not a ASP.NET coder and I don't have enough time to learn it. Suggestions and patches welcome.
            if (!Authentication.authenticate(HttpContext.Current))
                return;

            Configuration config = new Configuration();
            MediaStream remoteControl = new MediaStream();
            
            Response.ContentType = "audio/x-mpegurl";
            Response.Write("#EXTM3U\r\n");
            Response.Write(GetPlayList(remoteControl, config));

        }

        private String GetPlayList(MediaStream remoteControl, Configuration config) {

            StringBuilder builder = new StringBuilder();
            int pos = 0;
            foreach (Channel channel in remoteControl.GetChannels()) {
                // all transcoders
                foreach (Transcoder transcoder in remoteControl.GetTranscoders()) {
                        String name = "#EXTINF:"+(++pos)+","+channel.Name;
                        String streamurl = remoteControl.GetTranscodedTvStreamUrl(channel.IdChannel, config.Username, config.Password, transcoder.Id);
                        builder.Append(name);
                        builder.Append("\r\n");
                        builder.Append(streamurl);
                        builder.Append("\r\n");
                        builder.Append("\r\n");
                        break;

                }
            }


            return builder.ToString();
        }
    }
}