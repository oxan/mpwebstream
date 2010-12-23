using System;
using System.IO;
using System.ServiceModel;
using System.Web;
using MPWebStream.Streaming;
using TV4Home.Server.TVEInteractionLibrary.Interfaces;

namespace MPWebStream.Site {
    public class Streamer {
        public HttpContext Context {
            get;
            set;
        }

        public ITVEInteraction Server {
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

        public Streamer(HttpContext context, ITVEInteraction server, WebChannel channel) {
            this.Context = context;
            this.Server = server;
            this.Channel = channel;
            this.BufferSize = 524288;
        }

        public Streamer(HttpContext context, ITVEInteraction server, WebChannel channel, int bufferSize) : this(context, server, channel) {
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
                while (true) {
                    // read into buffer, but break out if transcoding process is done (prevent deadlock)
                    do {
                        read = outStream.Read(buffer, 0, buffer.Length);
                    } while (read == 0 && !encoder.IsTranscodingDone);
                    if (encoder.IsTranscodingDone)
                        break;

                    // write to client, if connected
                    if (!Context.Response.IsClientConnected) 
                        break;
                    Context.Response.OutputStream.Write(buffer, 0, read);
                    Context.Response.Flush();
                }
            } catch (Exception ex) {
                System.Console.WriteLine("Exception while streaming data");
                System.Console.WriteLine(ex.ToString());
            }

            // close and finish
            if (outStream != null) outStream.Close();
            if (sourceStream != null) sourceStream.Close();
            if (encoder != null) encoder.StopProcess();
            Server.CancelCurrentTimeShifting();
            Context.Response.End();
        }

        public static void run(HttpContext context) {
            // connect to TV4Home service and get channel
            ITVEInteraction tvServiceInterface = ChannelFactory<ITVEInteraction>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 },
                new EndpointAddress("net.pipe://localhost/TV4Home.Server.CoreService/TVEInteractionService"));
            WebChannel ch = tvServiceInterface.GetChannelById(Int32.Parse(context.Request.Params["channelId"]));

            // run
            Streamer s = new Streamer(context, tvServiceInterface, ch);
            s.stream();
        }
    }
}