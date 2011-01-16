using System;
using System.IO;
using System.ServiceModel;
using System.Web;
using System.Linq;
using MPWebStream.Streaming;
using TV4Home.Server.TVEInteractionLibrary.Interfaces;

namespace MPWebStream.Site {
    public enum StreamSource {
        Channel,
        Recording
    }

    public class Streamer {
        public HttpResponse Response {
            get;
            set;
        }

        public ITVEInteraction Server {
            get;
            set;
        }

        public StreamSource StreamType {
            get;
            set;
        }

        public int SourceId {
            get;
            set;
        }

        public int BufferSize { 
            get; 
            set; 
        }

        public string Username {
            get;
            private set;
        }

        public Streamer(HttpResponse response, ITVEInteraction server, StreamSource streamType, int sourceId) {
            this.Response = response;
            this.Server = server;
            this.StreamType = streamType;
            this.SourceId = sourceId;
            this.BufferSize = 524288;
        }

        public Streamer(HttpResponse response, ITVEInteraction server, WebChannelBasic channel) 
            : this(response, server, StreamSource.Channel, channel.IdChannel) {
        }

        public Streamer(HttpResponse response, ITVEInteraction server, WebRecording recording) 
            : this(response, server, StreamSource.Recording, recording.IdRecording) {
        }

        public void stream() {
            // the real work
            // variables
            Stream sourceStream = null;
            Stream outStream = null;
            EncoderWrapper encoder = null;

            // setup response
            Response.Clear();
            Response.Buffer = false;
            Response.BufferOutput = false;
            Response.AppendHeader("Connection", "Close");
            Response.ContentType = "video/x-ms-video"; // FIXME
            Response.StatusCode = 200;

            // setup encoding
            EncoderConfig config = new EncoderConfig("BLA", false, "", "", TransportMethod.NamedPipe, TransportMethod.NamedPipe);
            // EncoderConfig config = new EncoderConfig("H264", true, @"C:\TvServer\mencoder\mencoder.exe", "{0} -cache 8192 -ovc x264 -x264encopts rc-lookahead=30:ref=2:subme=6:no-8x8dct:bframes=0:no-cabac:cqm=flat:weightp=0 -oac lavc -lavcopts acodec=libfaac -of lavf -lavfopts format=mp4 -vf scale=800:450 -o {1}", TransportMethod.Filename, TransportMethod.NamedPipe);

            // start streaming
            Username = "mpwebstream-" + System.Guid.NewGuid().ToString("D"); // should be random enough
            WebVirtualCard card = Server.SwitchTVServerToChannelAndGetVirtualCard(Username, SourceId);
            if (config.inputMethod == TransportMethod.Filename) {
                encoder = new EncoderWrapper(card.RTSPUrl, config);
            } else {
                sourceStream = new TsBuffer(card.TimeShiftFileName);
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
                    if (!Response.IsClientConnected) 
                        break;
                    Response.OutputStream.Write(buffer, 0, read);
                    Response.Flush();
                }
            } catch (Exception ex) {
                System.Console.WriteLine("Exception while streaming data");
                System.Console.WriteLine(ex.ToString());
            }

            // close and finish
            if (outStream != null) outStream.Close();
            if (sourceStream != null) sourceStream.Close();
            if (encoder != null) encoder.StopProcess();
            Server.CancelCurrentTimeShifting(Username);
            Response.End();
        }

        public static void run(HttpContext context) {
            // parse request parameters and start streamer
            ITVEInteraction tvServiceInterface = ChannelFactory<ITVEInteraction>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 },
                new EndpointAddress("net.pipe://localhost/TV4Home.Server.CoreService/TVEInteractionService"));
            Streamer streamer;
            if (context.Request.Params["channelId"] != null) {
                streamer = new Streamer(context.Response, tvServiceInterface, tvServiceInterface.GetChannelBasicById(Int32.Parse(context.Request.Params["channelId"])));
            } else if (context.Request.Params["recordingId"] != null) {
                int recordingId = Int32.Parse(context.Request.Params["recordingId"]);
                streamer = new Streamer(context.Response, tvServiceInterface, tvServiceInterface.GetRecordings().Where(rec => rec.IdRecording == recordingId).First());
            } else {
                context.Response.Write("Specify at least a channelId or recordingId parameter");
                return;
            }

            // run
            streamer.stream();
        }
    }
}