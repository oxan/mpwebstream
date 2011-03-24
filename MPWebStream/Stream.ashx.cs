﻿
﻿#region Copyright
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

using System;
using System.Linq;
using System.ServiceModel;
using System.Web;
using TV4Home.Server.TVEInteractionLibrary.Interfaces;

namespace MPWebStream.Site.Pages {
    public class Stream : IHttpHandler {
        public void ProcessRequest(HttpContext context) {
            if (!Authentication.authenticate(context, true))
                return;

            // get transcoder
            Configuration config = new Configuration();
            TranscoderProfile transcoder;
            int transcoderId;
            if (Int32.TryParse(context.Request.Params["transcoder"], out transcoderId) && config.GetTranscoder(transcoderId) != null) {
                transcoder = config.GetTranscoder(transcoderId);
            } else {
                context.Response.Write("Specify a valid transcoder");
                return;
            }

            // parse request parameters and start streamer
            Streamer streamer = null;
            try {
                ITVEInteraction tvServiceInterface = ChannelFactory<ITVEInteraction>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 },
                    new EndpointAddress("net.pipe://localhost/TV4Home.Server.CoreService/TVEInteractionService"));
                if (context.Request.Params["channelId"] != null) {
                    streamer = new Streamer(tvServiceInterface, tvServiceInterface.GetChannelBasicById(Int32.Parse(context.Request.Params["channelId"])), transcoder);
                } else if (context.Request.Params["recordingId"] != null) {
                    int recordingId = Int32.Parse(context.Request.Params["recordingId"]);
                    streamer = new Streamer(tvServiceInterface, tvServiceInterface.GetRecordings().Where(rec => rec.IdRecording == recordingId).First(), transcoder);
                } else {
                    context.Response.Write("Specify at least a channelId or recordingId parameter");
                    return;
                }

                // run
                try {
                    streamer.startTranscoding();
                    if (context.Response.IsClientConnected) // client could have disconnected in the meantime
                        streamer.streamToClient(context.Response);
                } catch (Exception e) {
                    Log.Error("Streaming to client failed", e);
                }
                streamer.finishTranscoding();
            } catch (Exception e) {
                Log.Error("Some error occured during the request body", e);
                // try really hard to always finish transcoding
                if (streamer != null)
                    streamer.finishTranscoding();
            }
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}
