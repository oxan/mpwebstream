#region Copyright
/* 
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
using System.IO;
using System.Web;

namespace MPWebStream.MediaTranscoding {
    public class HttpOutputProcessingUnit : IProcessingUnit, IBlockingProcessingUnit {
        public Stream InputStream { get; set; }
        public Stream DataOutputStream { get; private set; }
        public Stream LogOutputStream { get; private set; }
        public bool IsInputStreamConnected { get; set; }
        public bool IsDataStreamConnected { get; set; }
        public bool IsLogStreamConnected { get; set; }

        private Pipeline pipeline;
        private dynamic response; // HttpResponse or HttpResponseBase
        private string mime;

        public HttpOutputProcessingUnit(Pipeline pipeline, string mime, HttpResponse response) {
            this.pipeline = pipeline;
            this.mime = mime;
            this.response = response;
        }

        public HttpOutputProcessingUnit(Pipeline pipeline, string mime, HttpResponseBase response) {
            this.pipeline = pipeline;
            this.mime = mime;
            this.response = response;
        }

        public bool Setup() {
            return true;
        }

        public bool Start() {
            // setup response
            response.Clear();
            response.Buffer = false;
            response.BufferOutput = false;
            response.AppendHeader("Connection", "Close");
            response.ContentType = this.mime == null || this.mime == string.Empty ? "video/MP2T" : this.mime;
            response.StatusCode = 200;

            // start the streaming
            Log.Write("HttpOutput: Copying from {0} to {1}", InputStream.ToString(), response.OutputStream.ToString());
            StreamCopy.AsyncStreamCopy(InputStream, response.OutputStream);

            return true;
        }

        public bool RunBlocking() {
            Log.Write("HttpOutput: Waiting for transcoder to end or client to disconnect now");
            try {
                while (true) {
                    if (response.IsClientConnected) { /* TODO: check for running transcoder */
                        System.Threading.Thread.Sleep(5000);
                    } else {
                        break;
                    }
                }
            } catch (HttpException ex) {
                Log.Write("HttpOutput: HttpException in TranscodeToClient, usually client disconnect, message {0}", ex.Message);
            }

            pipeline.Stop();

            return true;
        }

        public bool Stop() {
            return true;
        }
    }
}
