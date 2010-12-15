using System;
using System.IO;
using System.ServiceModel;
using System.Web;
using MPWebStream.Streaming;
using MPWebStream.TVEInteraction;

namespace MPWebStream.Site {
    public class Streamer {
        public HttpContext Context {
            get;
            set;
        }

        public TVEInteractionClient Server {
            get;
            set;
        }

        public WebChannel Channel {
            get;
            set;
        }

        public int BufferSize { 
            get; 
            set; 
        }

        public Streamer(HttpContext context, TVEInteractionClient server, WebChannel channel) {
            this.Context = context;
            this.Server = server;
            this.Channel = channel;
            this.BufferSize = 524288;
        }

        public Streamer(HttpContext context, TVEInteractionClient server, WebChannel channel, int bufferSize) : this(context, server, channel) {
            this.BufferSize = BufferSize;
        }

        public void stream() {
            // the real work
            // setup response
            Context.Response.Clear();
            Context.Response.Buffer = false;
            Context.Response.BufferOutput = false;
            Context.Response.AppendHeader("Connection", "Close");
            Context.Response.ContentType = "video/x-ms-video"; // FIXME
            Context.Response.StatusCode = 200;

            // setup encoder and variables
            Stream sourceStream = null;
            Stream outStream  = null;
            EncoderWrapper encoder = null;
            string sourceFilename;
            EncoderConfig config = new EncoderConfig("BLA", false, "", "", TransportMethod.NamedPipe, TransportMethod.NamedPipe);
            if (config.inputMethod == TransportMethod.Filename) {
                sourceFilename = Server.SwitchTVServerToChannelAndGetStreamingUrl(Channel.IdChannel);
                encoder = new EncoderWrapper(sourceFilename, config);
            } else {
                sourceFilename = Server.SwitchTVServerToChannelAndGetTimeshiftFilename(Channel.IdChannel);
                sourceStream = new TsBuffer(sourceFilename);
                encoder = new EncoderWrapper(sourceStream, config);
            }
            if (config.useTranscoding) {
                outStream = encoder;
            } else {
                outStream = sourceStream;
            }

            // stream
            byte[] buffer = new byte[BufferSize];
            int read;
            try {
                while ((read = outStream.Read(buffer, 0, buffer.Length)) > 0) {
                    Context.Response.OutputStream.Write(buffer, 0, read);
                }
            } catch (Exception ex) {
                // FIXME: handle
            }

            // close and finish
            if (outStream != null) outStream.Close();
            if (sourceStream != null) sourceStream.Close();
            if (encoder != null) encoder.StopProcess();
            Server.CancelCurrentTimeShifting();
            Context.Response.End();
        }

        public static void run(HttpContext context) {
            // connect to TV4Home service
            // FIXME: make hostname configurable
            EndpointAddress address = new EndpointAddress(String.Format("http://{0}:4321/TV4Home.Server.CoreService/TVEInteractionService", "mediastreamer.lan"));
            TVEInteractionClient tvWebClient = new TVEInteractionClient("BasicHttpBinding_ITVEInteraction", address);

            // FIXME: make this dynamic
            WebChannel ch = tvWebClient.GetChannelById(301);

            // run
            Streamer s = new Streamer(context, tvWebClient, ch);
            s.stream();
        }
    }
}