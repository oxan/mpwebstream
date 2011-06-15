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
using System.IO;
using System.Web;
using System.Collections.Generic;

namespace MPWebStream.MediaTranscoding {
    public class TranscodingStreamer {
        /// <summary>
        /// All states of the streamer
        /// </summary>
        private enum State {
            Initialized,
            TranscodingStarted,
            Streaming,
            StreamingFinished,
            TranscodingStopped
        }

        /// <summary>
        /// The transcoder that is used for streaming
        /// </summary>
        public TranscoderProfile Transcoder {
            get;
            set;
        }

        /// <summary>
        /// The source media file that is streamed
        /// </summary>
        public string Source {
            get;
            set;
        }

        /// <summary>
        /// The logfile to which the output of the transcoder (on stderr) is written
        /// </summary>
        public string TranscoderLog {
            get;
            set;
        }

        /// <summary>
        /// Is the transcoder currently running? A false return indicates that we're at the end of the input stream
        /// </summary>
        public bool IsTranscoding {
            get { 
                bool running = Transcoder.UseTranscoding ? encoder.TranscoderRunning : true; // FIXME
                if (!running && currentState == State.Streaming)
                    currentState = State.StreamingFinished;
                return running;
            }
            private set { }
        }

        /// <summary>
        /// The current state of the streamer
        /// </summary>
        private State currentState;
        /// <summary>
        /// The actual encoder which does the work
        /// </summary>
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
        /// buffer file ourself. This method abstracts that choice from your application.
        /// </summary>
        /// <param name="RTSPurl">URL to the RTSP stream.</param>
        /// <param name="tsbuffer">Path to the TsBuffer file.</param>
        /// <param name="transcoder">The transcoder to use.</param>
        public TranscodingStreamer(string RTSPurl, string tsbuffer, TranscoderProfile transcoder) :
            this(transcoder.UseTranscoding && transcoder.InputMethod == TransportMethod.Filename ? RTSPurl : tsbuffer, transcoder) {
        }


        /// <summary>
        /// Stop transcoding to prevent leaving transcoder processes around
        /// </summary>
        ~TranscodingStreamer() {
            StopTranscoding();
        }

        #region HTTP client
        /// <summary>
        /// Transcode and send the output to an HTTP client, waiting till we're at the end of the stream or the client disconnects.
        /// 
        /// This method abstracts the starting/stopping of transcoding, handling of HTTP specific-things and waiting till the client disconnects from you. You
        /// will probably use this method instead of the other three when you do something with HTTP.
        /// </summary>
        /// <param name="response">The response to write to</param>
        public void TranscodeToClient(HttpResponseBase response) {
            TranscodeToClientImplementation(response);
        }

        /// <summary>
        /// Transcode and send the output to an HTTP client, waiting till we're at the end of the stream or the client disconnects.
        /// 
        /// This method abstracts the starting/stopping of transcoding, handling of HTTP specific-things and waiting till the client disconnects from you. You
        /// will probably use this method instead of the other three when you do something with HTTP.
        /// </summary>
        /// <param name="response">The response to write to</param>
        public void TranscodeToClient(HttpResponse response) {
            TranscodeToClientImplementation(response);
        }

        /// <summary>
        /// Implementation of the TranscodeToClient() methods
        /// </summary>
        /// <param name="response">The response. This is guaranteed to be an HttpResponseBase or a HttpResponse object, which are API compatible.</param>
        private void TranscodeToClientImplementation(dynamic response) {
            // start the transcoding
            if (currentState != State.TranscodingStarted)
                StartTranscoding();

            // setup response
            response.Clear();
            response.Buffer = false;
            response.BufferOutput = false;
            response.AppendHeader("Connection", "Close");
            response.ContentType = Transcoder.MIME == null || Transcoder.MIME == string.Empty ? "video/MP2T" : Transcoder.MIME;
            response.StatusCode = 200;

            // start streaming it
            StartWriteToStream(response.OutputStream);
            encoder.VideoOutputStream = response.OutputStream;
            Log.Write("Waiting for transcoder to end or client to disconnect now");
            try {
                while (true) {
                    if (response.IsClientConnected && (Transcoder.UseTranscoding ? encoder.TranscoderRunning : true)) {
                        System.Threading.Thread.Sleep(5000);
                    } else {
                        break;
                    }
                }
            } catch (HttpException ex) {
                Log.Write("HttpException in TranscodeToClient, usually client disconnect, message {0}", ex.Message);
            }
            currentState = State.StreamingFinished;

            // stop transcoding
            StopTranscoding();
        }
        #endregion

        #region Start streaming
        /// <summary>
        /// Start the transcoding.
        /// </summary>
        public void StartTranscoding() {
            // encoder configuration
            Log.Write("Starting transcoding of {0} with transcoder named {1}", Source, Transcoder.Name);

            // create encoder
            encoder = new Transcoder(Transcoder, Source);
            encoder.StartTranscode();
#if !DEBUG
            if (Transcoder.UseTranscoding && TranscoderLog != null) {
                Log.Write("Copying stderr of transcoder into {0}", TranscoderLog);
                FileStream logstream = new FileStream(TranscoderLog, FileMode.Create, FileAccess.ReadWrite);
                StreamCopy.AsyncStreamCopy(encoder.TranscoderInfoOutputStream, logstream, "translog");
            }
#endif
            Log.Write("Transcoding started!");
            currentState = State.TranscodingStarted;
        }

        /// <summary>
        /// Starting writing the output of the transcoder to a stream.
        /// </summary>
        /// <param name="output">The stream to write to</param>
        public void StartWriteToStream(Stream output) {
            // start transcoding
            if (currentState != State.TranscodingStarted)
                StartTranscoding();

            // stream it
            encoder.VideoOutputStream = output;
            encoder.StartStreaming();
            currentState = State.Streaming;
            Log.Write("Streaming started!");
        }

        /// <summary>
        /// Start the output stream of the transcoder and return it
        /// </summary>
        /// <returns>Output data stream of transcoder</returns>
        public Stream StartStream() {
            // start transcoding
            if (currentState != State.TranscodingStarted)
                StartTranscoding();

            encoder.DoOutputCopy = false;
            encoder.StartStreaming();
            currentState = State.Streaming;
            Log.Write("Streaming started!");
            return encoder.VideoOutputStream;
        }
        #endregion

        /// <summary>
        /// Stop the transcoding.
        /// </summary>
        public void StopTranscoding() {
            if (currentState == State.TranscodingStopped || currentState == State.Initialized)
                return;
            Log.Write("Finishing transcoding");
            try {
                if (encoder != null) encoder.StopTranscode();
            } catch (Exception e) {
                Log.Error("Stopping transcoder failed", e);
            }
            currentState = State.TranscodingStopped;
        }

        #region Data methods
        /// <summary>
        /// Get the stderr stream of the transcoder
        /// </summary>
        /// <returns>Stream</returns>
        public Stream GetTranscoderOutputStream() {
            if (currentState != State.TranscodingStarted && currentState != State.Streaming)
                return encoder.TranscoderInfoOutputStream;
            return null;
        }
        #endregion

        /// <summary>
        /// Get all the transcoders that the user has configured in the TvServerPlugin of MPWebStream.
        /// 
        /// This list can be empty (for example when you integrate this in your own app and don't provide the MPWebStream plugins. It is intended for users to have single
        /// place to configure the transcoders for multiple pieces of software that use this library. 
        /// </summary>
        public static List<TranscoderProfile> GetConfiguredTranscoders() {
            Configuration config = new Configuration();
            return (List<TranscoderProfile>)config.Transcoders.Cast<TranscoderProfile>();
        }
    }
}