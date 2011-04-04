using System;
using System.IO;
using MPWebStream.MediaTranscoding;

namespace MPWebStream.StandaloneTest {
    class Program {
        static void Main(string[] args) {
            MPWebStream.MediaTranscoding.Log.RegisterWriter(new Log.LogWrite(ConsoleLogger.Write));

            // configuration
            string source = @"T:\test-input.ts";
            string destination = @"T:\test-output.ts";
            TranscoderProfile profile = new TranscoderProfile();
            profile.Id = 0;
            profile.UseTranscoding = false;

            // do it
            TranscodingStreamer stream = new TranscodingStreamer(source, profile);
            stream.StartTranscoding();
            FileStream outstream = new FileStream(destination, FileMode.Create);
            stream.StartWriteToStream(outstream);
            while (stream.IsTranscoding)
                System.Threading.Thread.Sleep(1000);
            stream.StopTranscoding();
        }
    }

    static class ConsoleLogger {
        public static void Write(string message) {
            System.Console.WriteLine(message);
        }
    }
}
