﻿#region Copyright
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
        public string Input {
            get;
            set;
        }
        public bool DoOutputCopy {
            get;
            set;
        }

        public bool TranscoderRunning {
            get { return transcoderApplication != null && !transcoderApplication.HasExited; }
        }
        #endregion

        #region Streams
        public Stream VideoOutputStream {
            get;
            set;
        }

        public Stream TranscoderInfoOutputStream {
            get;
            private set;
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
            this.Input = input;
            this.DoOutputCopy = true;
        }

        public void StartTranscode() {
            bool isTsBuffer = this.Input.IndexOf(".ts.tsbuffer") != -1; 

            // resolve the Path transport method if needed
            if (transcoder.InputMethod == TransportMethod.Path)
                transcoder.InputMethod = isTsBuffer ? TransportMethod.NamedPipe : TransportMethod.Filename;
            if (transcoder.OutputMethod == TransportMethod.Path)
                transcoder.OutputMethod = TransportMethod.NamedPipe; // keeping it in memory is probably faster

            // setup the TsBuffer if needed
            Stream readInputStream = null;
            if (!transcoder.UseTranscoding || transcoder.InputMethod != TransportMethod.Filename) {
                readInputStream = isTsBuffer ? (Stream)new TsBuffer(this.Input) : (Stream)new FileStream(this.Input, FileMode.Open);
                Log.Write("Input: type {0}", readInputStream.GetType() == typeof(TsBuffer) ? "TsBuffer" : "file");
            }

            // without external process
            if (!transcoder.UseTranscoding) {
                Log.Write("Output: direct copy of input stream");
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
                input = this.Input;
            } else if (transcoder.InputMethod == TransportMethod.NamedPipe) {
                transcoderInputStream = new NamedPipe();
                input = ((NamedPipe)transcoderInputStream).Url;
                Log.Write("Input: starting named pipe {0}", input);
                ((NamedPipe)transcoderInputStream).Start(false);
                inputStream = readInputStream;
            } else if (transcoder.InputMethod == TransportMethod.StandardIn) {
                needsStdin = true;
                inputStream = readInputStream;
            }

            // output stream
            if (transcoder.OutputMethod == TransportMethod.Filename) {
                output = Path.GetTempFileName(); // this doesn't work yet
            } else if (transcoder.OutputMethod == TransportMethod.NamedPipe) {
                transcoderOutputStream = new NamedPipe();
                output = ((NamedPipe)transcoderOutputStream).Url;
            } else if(transcoder.OutputMethod == TransportMethod.StandardOut) {
                needsStdout = true;
            }

            // start transcoder
            Log.Write("Transcoder configuration dump");
            Log.Write("  input {0}, output {1}", input, output);
            Log.Write("  needsStdin {0}, needsStdout {1}", needsStdin, needsStdout);
            Log.Write("  path {0}", transcoder.Transcoder);
            Log.Write("  arguments {0}", transcoder.Parameters);
            SpawnTranscoder(input, output, needsStdin, needsStdout);

            // finish stream setup
            if (transcoder.InputMethod == TransportMethod.StandardIn)
                transcoderInputStream = transcoderApplication.StandardInput.BaseStream;
            if (transcoder.OutputMethod == TransportMethod.StandardOut)
                transcoderOutputStream = transcoderApplication.StandardOutput.BaseStream;
            if (transcoder.OutputMethod == TransportMethod.Filename)
                transcoderOutputStream = new FileStream(output, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (transcoder.OutputMethod == TransportMethod.NamedPipe) {
                Log.Write("Output: starting named pipe {0}", output);
                ((NamedPipe)transcoderOutputStream).Start(false);
            }
        }

        protected void SpawnTranscoder(string input, string output, bool needsStdin, bool needsStdout) {
            string args = String.Format(transcoder.Parameters, input, output);
            ProcessStartInfo start = new ProcessStartInfo(transcoder.Transcoder, args);
            start.UseShellExecute = false;
            start.RedirectStandardInput = needsStdin;
            start.RedirectStandardOutput = needsStdout;
            start.RedirectStandardError = true;
#if DEBUG
            start.WindowStyle = ProcessWindowStyle.Normal;
            start.CreateNoWindow = false;
#endif

            transcoderApplication = new Process();
            transcoderApplication.StartInfo = start;
            transcoderApplication.Start();

            // copy stderr of the transcoder to a logfile if needed
#if !DEBUG
            if (transcoder.UseTranscoding && TranscoderLog != null) {
                Log.Write("Copying stderr of transcoder into {0}", TranscoderLog);
                FileStream logstream = new FileStream(TranscoderLog, FileMode.Create, FileAccess.ReadWrite);
                StreamCopy.AsyncStreamCopy(transcoderApplication.StandardError.BaseStream, logstream, "translog");
            }
#endif
        }

        public void StartStreaming() {
            // copy the inputStream to the transcoderInputStream
            if (inputStream != null && transcoderInputStream != null) {
                Log.Write("Copy input stream of type {0} into transcoder input stream of type {1}", inputStream.ToString(), transcoderInputStream.ToString());
                if (transcoderInputStream is NamedPipe)
                    WaitTillReady((NamedPipe)transcoderInputStream);
                StreamCopy.AsyncStreamCopy(inputStream, transcoderInputStream, "transinput");
            }

            // but make sure that the transcoder is running now
            if (transcoder.UseTranscoding && !TranscoderRunning) {
                Log.Write("ERROR: The transcoder isn't running anymore, refusing to copy the output stream");
                return;
            }

            // when retrieve original stream is set, just point OutputStream to transcoderOutputStream 
            if (!DoOutputCopy) {
                VideoOutputStream = transcoderOutputStream;
            } else if (transcoderOutputStream != null && VideoOutputStream != null) {
                // else copy the transcoder output stream to output stream
                Log.Write("Copy transcoder output stream of type {0} into output stream of type {1}", transcoderOutputStream.ToString(), VideoOutputStream.ToString());
                if (transcoderOutputStream is NamedPipe)
                    WaitTillReady((NamedPipe)transcoderOutputStream);
                StreamCopy.AsyncStreamCopy(transcoderOutputStream, VideoOutputStream, "transoutput");
            }
        }

        public void StopTranscode() {
            // close streams
            CloseStream(inputStream, "input");
            CloseStream(transcoderInputStream, "transcoder input");
            CloseStream(transcoderOutputStream, "transcoder output");
            CloseStream(VideoOutputStream, "output");

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

        private void WaitTillReady(NamedPipe pipe) {
            while(!pipe.IsReady)
                System.Threading.Thread.Sleep(100);
        }
    }
}