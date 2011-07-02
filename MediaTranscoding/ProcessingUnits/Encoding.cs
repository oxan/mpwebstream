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
using System.Diagnostics;

namespace MPWebStream.MediaTranscoding {
    public class EncodingProcessingUnit : IProcessingUnit {
        public Stream InputStream { get; set; }
        public Stream DataOutputStream { get; private set; }
        public Stream LogOutputStream { get; private set; }
        public bool IsInputStreamConnected { get; set; }
        public bool IsDataStreamConnected { get; set; }
        public bool IsLogStreamConnected { get; set; }

        private string transcoderPath;
        private string arguments;
        private TransportMethod inputMethod;
        private TransportMethod outputMethod;
        private Process transcoderApplication;
        private string source;
        private Stream transcoderInputStream;
        private bool doInputCopy;

        public EncodingProcessingUnit(string transcoder, string arguments, TransportMethod inputMethod, TransportMethod outputMethod) {
            this.transcoderPath = transcoder;
            this.arguments = arguments;
            this.inputMethod = inputMethod;
            this.outputMethod = outputMethod;
        }

        public EncodingProcessingUnit(string transcoder, string arguments, TransportMethod inputMethod, TransportMethod outputMethod, string source) :
            this(transcoder, arguments, inputMethod, outputMethod) {
            this.source = source;
        }

        public bool Setup() {
            // sets up streams
            string input = "";
            string output = "";
            bool needsStdin = false;
            bool needsStdout = false;

            // input (StandardOut not supported and External needs no processing)
            if (inputMethod == TransportMethod.Filename) {
                input = this.source;
            } else if (inputMethod == TransportMethod.NamedPipe) {
                transcoderInputStream = new NamedPipe();
                input = ((NamedPipe)transcoderInputStream).Url;
                Log.Write("Input: starting named pipe {0}", input);
                ((NamedPipe)transcoderInputStream).Start(false);
                doInputCopy = true;
            } else if (inputMethod == TransportMethod.StandardIn) {
                needsStdin = true;
                doInputCopy = true;
            } 

            // output stream (StandardIn not supported and External needs no processing)
            if (outputMethod == TransportMethod.Filename) {
                output = Path.GetTempFileName();
            } else if (outputMethod == TransportMethod.NamedPipe) {
                DataOutputStream = new NamedPipe();
                output = ((NamedPipe)DataOutputStream).Url;
            } else if (outputMethod == TransportMethod.StandardOut) {
                needsStdout = true;
            }

            // start transcoder
            Log.Write("Transcoder configuration dump");
            Log.Write("  input {0}, output {1}", input, output);
            Log.Write("  needsStdin {0}, needsStdout {1}", needsStdin, needsStdout);
            Log.Write("  path {0}", transcoderPath);
            Log.Write("  arguments {0}", arguments);
            SpawnTranscoder(input, output, needsStdin, needsStdout);

            // finish stream setup
            if (inputMethod == TransportMethod.StandardIn)
                transcoderInputStream = transcoderApplication.StandardInput.BaseStream;
            if (outputMethod == TransportMethod.StandardOut)
                DataOutputStream = transcoderApplication.StandardOutput.BaseStream;
            if (outputMethod == TransportMethod.Filename)
                DataOutputStream = new FileStream(output, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); // doesn't work yet
            if (outputMethod == TransportMethod.NamedPipe) {
                Log.Write("Output: starting named pipe {0}", output);
                ((NamedPipe)DataOutputStream).Start(false);
            }

            // setup stderr forwarding
            if (IsLogStreamConnected)
                LogOutputStream = transcoderApplication.StandardError.BaseStream;

            return true;
        }

        private void SpawnTranscoder(string input, string output, bool needsStdin, bool needsStdout) {
            string args = String.Format(arguments, input, output);
            ProcessStartInfo start = new ProcessStartInfo(transcoderPath, args);
            start.UseShellExecute = false;
            start.RedirectStandardInput = needsStdin;
            start.RedirectStandardOutput = needsStdout;
            start.RedirectStandardError = IsDataStreamConnected;
/*
#if DEBUG
            start.WindowStyle = ProcessWindowStyle.Normal;
            start.CreateNoWindow = false;
#endif
*/

            //try {
                transcoderApplication = new Process();
                transcoderApplication.StartInfo = start;
                transcoderApplication.Start();
            /*
            } catch (Win32Exception e) {
                Log.Error("Failed to start transcoder", e);
                Log.Write("ERROR: Transcoder probably doesn't exists");
                throw new TranscodingFailedException("Transcoder does not exists");
            }
             */
        }

        public bool Start() {
            // copy the inputStream to the transcoderInputStream
            if(doInputCopy) {
                Log.Write("Copy input stream of type {0} into transcoder input stream of type {1}", InputStream.ToString(), transcoderInputStream.ToString());
                if (transcoderInputStream is NamedPipe)
                    ((NamedPipe)transcoderInputStream).WaitTillReady();
                StreamCopy.AsyncStreamCopy(InputStream, transcoderInputStream, "transinput");
            }

            return true;
        }

        public bool Stop() {
            // close streams
            CloseStream(InputStream, "input");
            CloseStream(transcoderInputStream, "transcoder input");
            CloseStream(DataOutputStream, "transcoder output");

            if (transcoderApplication != null && !transcoderApplication.HasExited) {
                Log.Write("Killing transcoder");
                try {
                    transcoderApplication.Kill();
                } catch (Exception e) {
                    Log.Error("Failed to kill transcoder", e);
                }
            }

            return true;
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
