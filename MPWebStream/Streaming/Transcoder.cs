using System;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace MPWebStream.Streaming {
    public class Transcoder {
        private TranscoderProfile transcoder;
        private string input;

        private Stream inputStream = null;
        private Stream transcoderInputStream = null;
        private Stream transcoderOutputStream = null;
        private Stream outputStream = null;

        private Process transcoderApplication;

        private Thread inputCopyThread = null;
        private Thread outputCopyThread = null;

        public Transcoder(TranscoderProfile transcoder, string input) {
            this.transcoder = transcoder;
            this.input = input;
        }

        public void TranscodeTo(Stream output) {
            this.outputStream = output;
        }

        protected void SetupTranscoder() {
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

        public void StartTranscode(Stream output) {
            outputStream = output;

            // copy the inputStream to the transcoderInputStream, and simultaneously copy the transcoderOutputStream to the outputStream
            if (inputStream != null && transcoderInputStream != null)
                StreamCopy.AsyncStreamCopy(inputStream, transcoderInputStream);
            if (transcoderOutputStream != null && outputStream != null)
                StreamCopy.AsyncStreamCopy(transcoderOutputStream, outputStream);
        }

        public void StopTranscode() {
            transcoderApplication.Kill();
        }
    }
}