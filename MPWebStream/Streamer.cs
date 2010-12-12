using System;
using System.Web;
using System.ServiceModel;
using System.IO;
using MPWebStream.TVEInteraction;
using MPWebStream.Streaming;

namespace MPWebStream {
    public class Streamer {
        public HttpResponse Response {
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

        public Streamer(HttpResponse response, TVEInteractionClient server, WebChannel channel) {
            this.Response = response;
            this.Server = server;
            this.Channel = channel;
            this.BufferSize = 524288;
        }

        public Streamer(HttpResponse response, TVEInteractionClient server, WebChannel channel, int bufferSize) : this(response, server, channel) {
            this.BufferSize = BufferSize;
        }

        public void stream() {
            // the real work
            // setup response
            Response.Clear();
            Response.Buffer = false;
            Response.BufferOutput = false;
            Response.AppendHeader("Connection", "Close");
            Response.ContentType = "video/x-ms-video"; // FIXME
            Response.StatusCode = 200;

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
                    Response.OutputStream.Write(buffer, 0, read);
                }
            } catch (Exception ex) {
                // FIXME: handle
            }

            // close and finish
            if (outStream != null) outStream.Close();
            if (sourceStream != null) sourceStream.Close();
            if (encoder != null) encoder.StopProcess();
            Server.CancelCurrentTimeShifting();
            Response.End();
        }

        public static void run(HttpResponse Response) {
            // connect to TV4Home service
            // FIXME: make hostname configurable
            EndpointAddress address = new EndpointAddress(String.Format("http://{0}:4321/TV4Home.Server.CoreService/TVEInteractionService", "mediastreamer.lan"));
            TVEInteractionClient tvWebClient = new TVEInteractionClient("BasicHttpBinding_ITVEInteraction", address);

            // FIXME: make this dynamic
            WebChannel ch = tvWebClient.GetChannelById(301);

            // run
            Streamer s = new Streamer(Response, tvWebClient, ch);
            s.stream();
        }
    }
}