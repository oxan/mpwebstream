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
using System.Web;
using System.Collections.Generic;

namespace MPWebStream.MediaTranscoding {
    public class TranscodingStreamer {
        #region Properties
        public TranscoderProfile Transcoder {
            get;
            set;
        }

        public string Source {
            get;
            set;
        }

        public bool IsTranscoding {
            get { return Transcoder.UseTranscoding ? encoder.TranscoderRunning : true; }
            private set { }
        }
        #endregion

        private Transcoder encoder;

        public TranscodingStreamer(string source, TranscoderProfile transcoder) {
            this.Source = source;
            this.Transcoder = transcoder;
        }

        public void StartTranscoding() {
            // encoder configuration
            Log.Write("============================================");
            Log.Write("Using a transcoder named {0}", Transcoder.Name);
            Log.Write("Selected {0} as input for transcoding", Source);

            // create encoder
            encoder = new Transcoder(Transcoder, Source);
            Log.Write("Setting up transcoding...");
            encoder.StartTranscode();
            Log.Write("Transcoding started!");
        }

        public void StartWriteToStream(Stream output) {
            // stream it
            encoder.OutputStream = output;
            Log.Write("Writing to stream {0}", output);
            encoder.StartStreaming();
            Log.Write("Streaming started!");
        }

        public void WriteToClient(HttpResponse response) {
            // setup response
            response.Clear();
            response.Buffer = false;
            response.BufferOutput = false;
            response.AppendHeader("Connection", "Close");
            response.ContentType = "video/MP2T"; // FIXME
            response.StatusCode = 200;
            
            // start streaming it
            StartWriteToStream(response.OutputStream);
            encoder.OutputStream = response.OutputStream;
            Log.Write("Waiting for transcoder to end or client to disconnect now");
            while (true) {
                if (response.IsClientConnected && (Transcoder.UseTranscoding ? encoder.TranscoderRunning : true)) {
                    System.Threading.Thread.Sleep(5000);
                } else {
                    break;
                }
            }
        }

        public void StopTranscoding() {
            Log.Write("Finishing transcoding");
            try {
                if (encoder != null) encoder.StopTranscode();
            } catch (Exception e) {
                Log.Error("Stopping transcoder failed", e);
            }
        }

        /// <summary>
        /// Get all the transcoders that the user has configured in the TvServerPlugin of MPWebStream.
        /// 
        /// This list can be empty (for example when you integrate this in your own app and don't provide the MPWebStream plugins. It is intended for users to have single
        /// place to configure the transcoders for multiple pieces of software that use this library. 
        /// </summary>
        public static List<TranscoderProfile> GetConfiguredTranscoders() {
            Configuration config = new Configuration();
            return config.Transcoders;
        }
    }
}