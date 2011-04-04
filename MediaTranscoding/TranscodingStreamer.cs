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

using System;
using System.IO;
using System.Web;
using System.Collections.Generic;

namespace MPWebStream.MediaTranscoding {
    public class TranscodingStreamer {
        private enum State {
            Initialized,
            TranscodingStarted,
            Streaming,
            StreamingFinished,
            TranscodingStopped
        }

        public TranscoderProfile Transcoder {
            get;
            set;
        }

        public string Source {
            get;
            set;
        }

        public bool IsTranscoding {
            get { 
                bool running = Transcoder.UseTranscoding ? encoder.TranscoderRunning : true; // FIXME
                if (!running && currentState == State.Streaming)
                    currentState = State.StreamingFinished;
                return running;
            }
            private set { }
        }

        private State currentState;
        private Transcoder encoder;

        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="source">The source to transcode, most commonly path to a file.</param>
        /// <param name="transcoder">The transcoder to use.</param>
        public TranscodingStreamer(string source, TranscoderProfile transcoder) {
            this.Source = source;
            this.Transcoder = transcoder;
            currentState = State.Initialized;
        }

        /// <summary>
        /// Convenience method to use when streaming live TV.
        /// 
        /// Based on the transcoder configuration, we need to pass an RTSP url, a path to the TS buffer file to the transcoder, or we need to read the TS
        /// buffer file ourself. This method abstracts that choose from your application.
        /// </summary>
        /// <param name="RTSPurl">URL to the RTSP</param>
        /// <param name="tsbuffer">Path</param>
        /// <param name="transcoder"></param>
        public TranscodingStreamer(string RTSPurl, string tsbuffer, TranscoderProfile transcoder) :
            this(transcoder.InputMethod == TransportMethod.Filename ? RTSPurl : tsbuffer, transcoder) {
        }

        /// <summary>
        /// Transcode and send the output to an HTTP client, waiting till we're at the end of the stream or the client disconnects.
        /// 
        /// This method abstracts the starting/stopping of transcoding, handling of HTTP specific-things and waiting till the client disconnects from you. You
        /// will probably use this method instead of the other three when you do something with HTTP.
        /// </summary>
        public void TranscodeToClient(HttpResponse response) {
            // start the transcoding
            if (currentState != State.TranscodingStarted)
                StartTranscoding();

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
            currentState = State.StreamingFinished;

            // stop transcoding
            StopTranscoding();
        }

        /// <summary>
        /// Start the transcoding.
        /// </summary>
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
            currentState = State.TranscodingStarted;
        }

        /// <summary>
        /// Starting writing the output of the transcoder to a stream.
        /// </summary>
        /// <param name="output">The stream to write to</param>
        public void StartWriteToStream(Stream output) {
            // stream it
            encoder.OutputStream = output;
            Log.Write("Writing to stream {0}", output);
            encoder.StartStreaming();
            currentState = State.Streaming;
            Log.Write("Streaming started!");
        }

        /// <summary>
        /// Stop the transcoding.
        /// </summary>
        public void StopTranscoding() {
            Log.Write("Finishing transcoding");
            try {
                if (encoder != null) encoder.StopTranscode();
            } catch (Exception e) {
                Log.Error("Stopping transcoder failed", e);
            }
            currentState = State.TranscodingStopped;
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