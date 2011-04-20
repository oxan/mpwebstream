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

using MPWebStream.MediaTranscoding;
using System;
using System.Linq;
using System.ServiceModel;
using System.Web;
using TV4Home.Server.TVEInteractionLibrary.Interfaces;

namespace MPWebStream.Site {
    public class StreamPage : IHttpHandler {
        public bool IsReusable {
            get {
                return false;
            }
        }

        private enum StreamType {
            Unknown, 
            Recording,
            Channel
        }

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

            // initialize
            ITVEInteraction tvServiceInterface = ChannelFactory<ITVEInteraction>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 },
                new EndpointAddress("net.pipe://localhost/TV4Home.Server.CoreService/TVEInteractionService"));
            WebVirtualCard card;
            string username = "";
            bool dataSend = false;
            StreamType type = StreamType.Unknown;
            string source = "";

            // the actually streaming
            try {
                Log.Write("============================================");
                if (context.Request.Params["channelId"] != null) {
                    // live tv streaming
                    type = StreamType.Channel;
                    username = "mpwebstream-" + System.Guid.NewGuid().ToString("D"); // should be random enough
                    WebChannelBasic channel = tvServiceInterface.GetChannelBasicById(Int32.Parse(context.Request.Params["channelId"]));
                    Log.Write("Trying to switch to channel {0} with username {1}", channel.IdChannel, username);
                    card = tvServiceInterface.SwitchTVServerToChannelAndGetVirtualCard(username, channel.IdChannel);
                    source = transcoder.InputMethod == TransportMethod.Filename ? card.RTSPUrl : card.TimeShiftFileName; // FIXME
                    Log.Write("Channel switch succeeded");
                } else if (context.Request.Params["recordingId"] != null) {
                    // recording streaming
                    type = StreamType.Recording;
                    int recordingId = Int32.Parse(context.Request.Params["recordingId"]);
                    WebRecording recording = tvServiceInterface.GetRecordings().Where(rec => rec.IdRecording == recordingId).First();
                    Log.Write("Streaming recording with id {0}", recording.IdRecording);
                    source = recording.FileName;
                } else {
                    // user error
                    context.Response.Write("Specify at least a channelId or recordingId parameter");
                    return;
                }

                // run
                TranscodingStreamer streamer = new TranscodingStreamer(source, transcoder);
                if (!context.Response.IsClientConnected) {
                    Log.Write("Client has disconnected, not even bothering to start transcoding");
                } else {
                    streamer.StartTranscoding();
                    if (context.Response.IsClientConnected) { // client could have disconnected in the meantime
                        streamer.TranscodeToClient(context.Response);
                        dataSend = true;
                    }
                    streamer.StopTranscoding();
                }
            } catch (FaultException e) {
                // exception from TV4Home, so display message to user, probably not our fault. 
                Log.Error("Got error response from the TV Server", e);
                if (!dataSend)
                    WriteServerError(context, "Got error response from the TV Server: " + e.Message, e);
            } catch (Exception e) {
                Log.Error("Some error occured during the request body", e);
                if (!dataSend)
                    WriteServerError(context, "Some error occured during the streaming", e);  // only possible when no stream data send yet
            } finally {
                // stop timeshifting if needed
                if (type == StreamType.Channel && username != "") {
                    Log.Write("Finishing timeshifting");
                    try {
                        tvServiceInterface.CancelCurrentTimeShifting(username);
                    } catch (Exception e) {
                        Log.Error("Finishing timeshifting failed", e);
                    }
                }
            }
        }

        private void WriteServerError(HttpContext context, string message, Exception e) {
            context.Response.AddHeader("Content-Type", "text/plain");
            context.Response.Write("Failed to create the stream you requested.\r\n\r\n");
            context.Response.Write(message);
            context.Response.Write("\r\n\r\n");
            context.Response.Write(e.ToString());
            context.Response.End();
        }
    }
}
