#region Copyright
/* 
 *  Copyright (C) 2009, 2010 Gemx
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
using System.IO;
using System.ServiceModel;
using System.Web;
using System.Linq;
using MPWebStream;
using MPWebStream.Streaming;
using TV4Home.Server.TVEInteractionLibrary.Interfaces;

namespace MPWebStream.Site {
    public enum StreamSource {
        Channel,
        Recording
    }

    public class Streamer {
        #region Properties
        public ITVEInteraction Server {
            get;
            set;
        }

        public StreamSource StreamType {
            get;
            set;
        }

        public int SourceId {
            get;
            set;
        }

        public int BufferSize { 
            get; 
            set; 
        }

        public TranscoderProfile Transcoder {
            get;
            set;
        }

        public string Username {
            get;
            protected set;
        }
        #endregion

        private Transcoder encoder;
        private Stream source;
        private Stream outStream;

        public Streamer(ITVEInteraction server, StreamSource streamType, int sourceId, TranscoderProfile transcoder) {
            this.Server = server;
            this.StreamType = streamType;
            this.SourceId = sourceId;
            this.BufferSize = 524288;
            this.Transcoder = transcoder;
        }

        public Streamer(ITVEInteraction server, WebChannelBasic channel, TranscoderProfile transcoder) 
            : this(server, StreamSource.Channel, channel.IdChannel, transcoder) {
        }

        public Streamer(ITVEInteraction server, WebRecording recording, TranscoderProfile transcoder) 
            : this(server, StreamSource.Recording, recording.IdRecording, transcoder) {
        }

        public void startTranscoding() {
            // encoder configuration
            Log.Write("============================================");
            Log.Write("Using a transcoder named {0}", Transcoder.Name);

            // get the path to the source
            string path = "";
            if (StreamType == StreamSource.Channel) {
                Username = "mpwebstream-" + System.Guid.NewGuid().ToString("D"); // should be random enough
                Log.Write("Trying to switch to channel {0} with username {1}", SourceId, Username);
                WebVirtualCard card = Server.SwitchTVServerToChannelAndGetVirtualCard(Username, SourceId);
                Log.Write("Switching channel succeeded");
                path = Transcoder.InputMethod == TransportMethod.Filename ? card.RTSPUrl : card.TimeShiftFileName; // FIXME
                Log.Write("Selected {0} as input URL", path);
            } else if(StreamType == StreamSource.Recording) {
                WebRecording recording = Server.GetRecordings().Where(rec => rec.IdRecording == SourceId).First();
                path = recording.FileName;
                Log.Write("Selected {0} as input URL for recording {1}", path, SourceId);
            }

            // create encoder
            encoder = new Transcoder(Transcoder, path);
            Log.Write("Setting up transcoding...");
            encoder.StartTranscode();
            Log.Write("Transcoding started!");
        }

        public void streamToClient(HttpResponse response) {
            // setup response
            response.Clear();
            response.Buffer = false;
            response.BufferOutput = false;
            response.AppendHeader("Connection", "Close");
            response.ContentType = "video/MP2T"; // FIXME
            response.StatusCode = 200;
            
            // start streaming it
            try {
                encoder.OutputStream = response.OutputStream;
                Log.Write("Starting streaming to the client now...");
                encoder.StartStreaming();
                Log.Write("Streaming to the client started, waiting for transcoder to end now");
                encoder.WaitTillExit();
                Log.Write("Streaming done!");
            } catch (Exception ex) {
                Log.Error("Exception while streaming data", ex);
                System.Console.WriteLine("Exception while streaming data");
                System.Console.WriteLine(ex.ToString());
            }
        }

        public void finishTranscoding() {
            // close and finish
            Log.Write("Finishing request");
            try {
                if (outStream != null) outStream.Close();
                if (source != null) source.Close();
                if (encoder != null) encoder.StopTranscode();
            } catch (Exception e) {
                Log.Error("Closing streams and stopping encoder failed", e);
            }
            Server.CancelCurrentTimeShifting(Username);
        }
    }
}
