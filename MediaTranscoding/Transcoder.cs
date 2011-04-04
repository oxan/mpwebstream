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

namespace MPWebStream.MediaTranscoding {
    class Transcoder {
        #region Properties
        public TranscoderProfile transcoder {
            get;
            set;
        }
        public string input {
            get;
            set;
        }
        public Stream OutputStream {
            get;
            set;
        }
        public Boolean TranscoderRunning {
            get { return transcoderApplication != null && !transcoderApplication.HasExited; }
        }
        #endregion
        #region Private variables
        private Stream inputStream = null;
        private Stream transcoderInputStream = null;
        private Stream transcoderOutputStream = null;

        private Process transcoderApplication;
        #endregion

        public Transcoder(TranscoderProfile transcoder, string input) {
            this.transcoder = transcoder;
            this.input = input;
        }

        public void StartTranscode() {
            // setup the TsBuffer if needed
            Stream readInputStream = null;
            // weird logic to ignore the InputMethod when no transcoding is ued
            if ((transcoder.InputMethod != TransportMethod.Filename && transcoder.UseTranscoding) || !transcoder.UseTranscoding) {
                readInputStream = this.input.IndexOf(".ts.tsbuffer") != -1 ? (Stream)new TsBuffer(this.input) : (Stream)new FileStream(this.input, FileMode.Open);
                Log.Write("Using a {0} as input stream", readInputStream.GetType().ToString() == "MPWebStream.MediaTranscoding.TsBuffer" ? "TsBuffer" : "file");
            }


            // without external process
            if (!transcoder.UseTranscoding) {
                Log.Write("Not using transcoding, just streaming");
                transcoderOutputStream = readInputStream;
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
                Log.Write("Starting named pipe {0}, being input stream", input);
                ((NamedPipe)transcoderInputStream).Start(false);
                inputStream = readInputStream;
            } else if (transcoder.InputMethod == TransportMethod.StandardIn) {
                needsStdin = true;
                inputStream = readInputStream;
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
            Log.Write("Transcoder configuration dump; input {0}, output {1}, needsStdin {2}, needsStdout {3}", input, output, needsStdin, needsStdout);
            SpawnTranscoder(input, output, needsStdin, needsStdout);

            // finish stream setup
            if (transcoder.InputMethod == TransportMethod.StandardIn)
                transcoderInputStream = transcoderApplication.StandardInput.BaseStream;
            if (transcoder.OutputMethod == TransportMethod.StandardOut)
                transcoderOutputStream = transcoderApplication.StandardOutput.BaseStream;
            if (transcoder.OutputMethod == TransportMethod.NamedPipe) {
                Log.Write("Starting named pipe {0}, being output stream", output);
                ((NamedPipe)transcoderOutputStream).Start(false);
            }
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
            Log.Write("Start copying all the streams");
            if (inputStream != null && transcoderInputStream != null) {
                Log.Write("Copy input stream into transcoder input stream");
                if (transcoderInputStream is NamedPipe)
                    WaitTillReady((NamedPipe)transcoderInputStream);
                StreamCopy.AsyncStreamCopy(inputStream, transcoderInputStream, "transinput");
            }
            if (transcoderOutputStream != null && OutputStream != null) {
                Log.Write("Copy transcoder output stream into output stream");
                if (transcoderOutputStream is NamedPipe)
                    WaitTillReady((NamedPipe)transcoderOutputStream);
                StreamCopy.AsyncStreamCopy(transcoderOutputStream, OutputStream, "transoutput");
            }
        }

        public void StopTranscode() {
            Log.Write("Killing transcoder");
            if (transcoderApplication != null && !transcoderApplication.HasExited) {
                try {
                    transcoderApplication.Kill();
                } catch (Exception e) {
                    Log.Error("Failed to kill transcoder", transcoderApplication);
                }
            }
        }

        private void WaitTillReady(NamedPipe pipe) {
            while(!pipe.IsReady)
                System.Threading.Thread.Sleep(100);
        }
    }
}