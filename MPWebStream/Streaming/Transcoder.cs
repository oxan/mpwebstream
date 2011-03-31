#region Copyright
/* 
 *  Copyright (C) 2011, Oxan
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
using System.Diagnostics;
using System.IO;

namespace MPWebStream.Streaming {
    public class Transcoder {
        private TranscoderProfile transcoder;
        private string input;
        public Stream OutputStream {
            get;
            set;
        }

        private Stream inputStream = null;
        private Stream transcoderInputStream = null;
        private Stream transcoderOutputStream = null;

        private Process transcoderApplication;

        public Transcoder(TranscoderProfile transcoder, string input) {
            this.transcoder = transcoder;
            this.input = input;
        }

        public void StartTranscode() {
            // without external process
            if (!transcoder.UseTranscoding) {
                transcoderOutputStream = new TsBuffer(this.input);
                return;
            }

            // sets up streams
            string input = "";
            string output = "";
            bool needsStdin = false;
            bool needsStdout = false;

            // input
            if (transcoder.InputMethod == TransportMethod.Filename) {
                input = this.input;
            } else if (transcoder.InputMethod == TransportMethod.NamedPipe) {
                transcoderInputStream = new NamedPipe();
                input = ((NamedPipe)transcoderInputStream).Url;
            } else if (transcoder.InputMethod == TransportMethod.StandardIn) {
                needsStdin = true;
            }

            // output stream
            if (transcoder.OutputMethod == TransportMethod.Filename) {
                // TODO
            } else if (transcoder.OutputMethod == TransportMethod.NamedPipe) {
                transcoderOutputStream = new NamedPipe();
                output = ((NamedPipe)transcoderOutputStream).Url;
            } else if(transcoder.OutputMethod == TransportMethod.StandardOut) {
                needsStdout = true;
            }

            // start transcoder
            SpawnTranscoder(input, output, needsStdin, needsStdout);

            // finish stream setup
            if (transcoder.InputMethod == TransportMethod.StandardIn)
                transcoderInputStream = transcoderApplication.StandardInput.BaseStream;
            if (transcoder.OutputMethod == TransportMethod.StandardOut)
                transcoderOutputStream = transcoderApplication.StandardOutput.BaseStream;
        }

        protected void SpawnTranscoder(string input, string output, bool needsStdin, bool needsStdout) {
            string args = String.Format(transcoder.Parameters, input, output);
            ProcessStartInfo start = new ProcessStartInfo(transcoder.Transcoder, args);
            start.UseShellExecute = false;
            start.RedirectStandardInput = needsStdin;
            start.RedirectStandardOutput = needsStdout;

            transcoderApplication = new Process();
            transcoderApplication.StartInfo = start;
            transcoderApplication.Start();
        }

        public void StartStreaming() {
            // copy the inputStream to the transcoderInputStream, and simultaneously copy the transcoderOutputStream to the outputStream
            if (inputStream != null && transcoderInputStream != null)
                StreamCopy.AsyncStreamCopy(inputStream, transcoderInputStream);
            if (transcoderOutputStream != null && OutputStream != null)
                StreamCopy.AsyncStreamCopy(transcoderOutputStream, OutputStream);
        }

        public void StopTranscode() {
            transcoderApplication.Kill();
        }

        // wait till it stopped
        public void WaitTillExit() {
            transcoderApplication.WaitForExit();
        }
    }
}