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
            profile.UseTranscoding = true;
            profile.InputMethod = TransportMethod.NamedPipe;
            profile.OutputMethod = TransportMethod.NamedPipe;
            profile.Transcoder = @".\ffmpeg\bin\ffmpeg.exe";
            profile.Parameters = "-i {0} -sameq -vcodec libxvid -acodec aac -strict experimental -f mpegts {1}";

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
            System.Console.WriteLine("{0:yyyy-MM-dd HH:mm:ss.ffffff}: {1}", DateTime.Now, message);
        }
    }
}
