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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace MPWebStream.MediaTranscoding {
    class Encoder {
        #region Properties
        public TranscoderProfile Profile {
            get;
            set;
        }
        public string Input {
            get;
            set;
        }
        public bool WantTranscoderInfo {
            get;
            set;
        }

        public bool TranscoderRunning {
            get { return transcoderApplication != null && !transcoderApplication.HasExited; }
        }
        #endregion

        #region Streams
        public Stream InputStream { get; set; }
        public Stream TranscoderInputStream { get; set; }
        public Stream TranscoderOutputStream { get; set; }
        public Stream TranscoderInfoOutputStream { get; set; }
        #endregion

        private Process transcoderApplication;

        public Encoder(TranscoderProfile transcoder, string input) {
            this.Profile = transcoder;
            this.Input = input;
            this.WantTranscoderInfo = false;
        }

        public void StartTranscode() {
            bool isTsBuffer = this.Input.IndexOf(".ts.tsbuffer") != -1; 

            // resolve the Path transport method if needed
            if (Profile.InputMethod == TransportMethod.Path)
                Profile.InputMethod = isTsBuffer ? TransportMethod.NamedPipe : TransportMethod.Filename;
            if (Profile.OutputMethod == TransportMethod.Path)
                Profile.OutputMethod = TransportMethod.NamedPipe; // keeping it in memory is probably faster

            // setup the TsBuffer if needed
            Stream readInputStream = null;
            if (!Profile.UseTranscoding || Profile.InputMethod != TransportMethod.Filename) {
                readInputStream = isTsBuffer ? (Stream)new TsBuffer(this.Input) : (Stream)new FileStream(this.Input, FileMode.Open);
                Log.Write("Input: type {0}", readInputStream.GetType() == typeof(TsBuffer) ? "TsBuffer" : "file");
            }

            // without external process
            if (!Profile.UseTranscoding) {
                Log.Write("Output: direct copy of input stream");
                TranscoderOutputStream = readInputStream;
                TranscoderInfoOutputStream = null;
                return;
            }

            // sets up streams
            string input = "";
            string output = "";
            bool needsStdin = false;
            bool needsStdout = false;

            // input
            if (Profile.InputMethod == TransportMethod.Filename) {
                input = this.Input;
            } else if (Profile.InputMethod == TransportMethod.NamedPipe) {
                TranscoderInputStream = new NamedPipe();
                input = ((NamedPipe)TranscoderInputStream).Url;
                Log.Write("Input: starting named pipe {0}", input);
                ((NamedPipe)TranscoderInputStream).Start(false);
                InputStream = readInputStream;
            } else if (Profile.InputMethod == TransportMethod.StandardIn) {
                needsStdin = true;
                InputStream = readInputStream;
            } else if (Profile.InputMethod == TransportMethod.External) {
                // we don't have to do any setup here
            }

            // output stream
            if (Profile.OutputMethod == TransportMethod.Filename) {
                // TODO: this doesn't work yet (and I bet nobody wants to use it)
                output = Path.GetTempFileName();
            } else if (Profile.OutputMethod == TransportMethod.NamedPipe) {
                TranscoderOutputStream = new NamedPipe();
                output = ((NamedPipe)TranscoderOutputStream).Url;
            } else if(Profile.OutputMethod == TransportMethod.StandardOut) {
                needsStdout = true;
            }

            // start transcoder
            Log.Write("Transcoder configuration dump");
            Log.Write("  input {0}, output {1}", input, output);
            Log.Write("  needsStdin {0}, needsStdout {1}", needsStdin, needsStdout);
            Log.Write("  path {0}", Profile.Transcoder);
            Log.Write("  arguments {0}", Profile.Parameters);
            SpawnTranscoder(input, output, needsStdin, needsStdout);

            // finish stream setup
            if (Profile.InputMethod == TransportMethod.StandardIn)
                TranscoderInputStream = transcoderApplication.StandardInput.BaseStream;
            if (Profile.OutputMethod == TransportMethod.StandardOut)
                TranscoderOutputStream = transcoderApplication.StandardOutput.BaseStream;
            if (Profile.OutputMethod == TransportMethod.Filename)
                TranscoderOutputStream = new FileStream(output, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (Profile.OutputMethod == TransportMethod.NamedPipe) {
                Log.Write("Output: starting named pipe {0}", output);
                ((NamedPipe)TranscoderOutputStream).Start(false);
            }

            // setup stderr forwarding
            if (WantTranscoderInfo)
                TranscoderInfoOutputStream = transcoderApplication.StandardError.BaseStream;
        }

        protected void SpawnTranscoder(string input, string output, bool needsStdin, bool needsStdout) {
            string args = String.Format(Profile.Parameters, input, output);
            ProcessStartInfo start = new ProcessStartInfo(Profile.Transcoder, args);
            start.UseShellExecute = false;
            start.RedirectStandardInput = needsStdin;
            start.RedirectStandardOutput = needsStdout;
            start.RedirectStandardError = this.WantTranscoderInfo;
#if DEBUG
            start.WindowStyle = ProcessWindowStyle.Normal;
            start.CreateNoWindow = false;
#endif

            try {
                transcoderApplication = new Process();
                transcoderApplication.StartInfo = start;
                transcoderApplication.Start();
            } catch (Win32Exception e) {
                Log.Error("Failed to start transcoder", e);
                Log.Write("ERROR: Transcoder probably doesn't exists");
                throw new TranscodingFailedException("Transcoder does not exists");
            }
        }

        public void StartStreaming() {
            // copy the inputStream to the transcoderInputStream
            if (InputStream != null && TranscoderInputStream != null) {
                Log.Write("Copy input stream of type {0} into transcoder input stream of type {1}", InputStream.ToString(), TranscoderInputStream.ToString());
                if (TranscoderInputStream is NamedPipe)
                    ((NamedPipe)TranscoderInputStream).WaitTillReady();
                StreamCopy.AsyncStreamCopy(InputStream, TranscoderInputStream, "transinput");
            }

            // make sure that the transcoder is still running
            if (Profile.UseTranscoding && !TranscoderRunning) {
                Log.Write("ERROR: The transcoder isn't running anymore, refusing to copy the output stream");
                StopTranscode();
                throw new TranscodingFailedException();
            }
        }

        public void StopTranscode() {
            // close streams
            CloseStream(InputStream, "input");
            CloseStream(TranscoderInputStream, "transcoder input");
            CloseStream(TranscoderOutputStream, "transcoder output");

            if (transcoderApplication != null && !transcoderApplication.HasExited) {
                Log.Write("Killing transcoder");
                try {
                    transcoderApplication.Kill();
                } catch (Exception e) {
                    Log.Error("Failed to kill transcoder", e);
                }
            }
        }

        private void CloseStream(Stream stream, string logName) {
            try {
                if (stream != null) stream.Close();
            } catch (Exception e) {
                Log.Write("Failed to close {0} stream: {1}", logName, e.Message);
            }
        }    
    }
}