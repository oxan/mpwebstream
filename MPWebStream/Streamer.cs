using System;
using System.Web;
using System.ServiceModel;
using System.IO;
using MPWebStream.TVEInteraction;

namespace MPWebStream {
    public class Streamer {
        public HttpResponse Response {
            get;
            private set;
        }

        public string Source {
            get;
            private set;
        }

        public int BufferSize { 
            get; 
            set; 
        }

        public Streamer(HttpResponse response, string source) {
            this.Response = response;
            this.Source = source;
            this.BufferSize = 524288;
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
            Stream sourceStream;
            Stream outStream;
            outStream = sourceStream;

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
            Response.End();
        }

        public static void run(HttpResponse Response) {
            // connect to TV4Home service
            // FIXME: make hostname configurable
            EndpointAddress address = new EndpointAddress(String.Format("http://{0}:4321/TV4Home.Server.CoreService/TVEInteractionService", "mediastreamer.lan"));
            TVEInteractionClient tvWebClient = new TVEInteractionClient("BasicHttpBinding_ITVEInteraction", address);

            // FIXME: make this dynamic
            WebChannel ch = tvWebClient.GetChannelById(301);
            String streamingUrl = tvWebClient.SwitchTVServerToChannelAndGetStreamingUrl(ch.IdChannel);

            // run
            Streamer s = new Streamer(Response, ch);
            s.stream();
        }
    }
}