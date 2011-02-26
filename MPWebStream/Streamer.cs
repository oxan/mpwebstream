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
            set;
        }

        public TranscoderProfile Transcoder {
            get;
            set;
        }

        public Streamer(HttpResponse response, ITVEInteraction server, StreamSource streamType, int sourceId, TranscoderProfile transcoder) {
            this.Response = response;
            this.Server = server;
            this.StreamType = streamType;
            this.SourceId = sourceId;
            this.BufferSize = 524288;
            this.Transcoder = transcoder;
        }

        public Streamer(HttpResponse response, ITVEInteraction server, WebChannelBasic channel, TranscoderProfile transcoder) 
            : this(response, server, StreamSource.Channel, channel.IdChannel, transcoder) {
        }

        public Streamer(HttpResponse response, ITVEInteraction server, WebRecording recording, TranscoderProfile transcoder) 
            : this(response, server, StreamSource.Recording, recording.IdRecording, transcoder) {
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

            // encoder configuration
            TransportMethod inputMethod = (TransportMethod)Enum.Parse(typeof(TransportMethod), Transcoder.InputMethod, true);
            TransportMethod outputMethod = (TransportMethod)Enum.Parse(typeof(TransportMethod), Transcoder.OutputMethod, true);
            EncoderConfig config = new EncoderConfig(Transcoder.Name, Transcoder.UseTranscoding, Transcoder.Transcoder, Transcoder.Parameters, inputMethod, outputMethod);

            // get the path to the source
            string path = "";
            if (StreamType == StreamSource.Channel) {
                Username = "mpwebstream-" + System.Guid.NewGuid().ToString("D"); // should be random enough
                WebVirtualCard card = Server.SwitchTVServerToChannelAndGetVirtualCard(Username, SourceId);
                path = config.inputMethod == TransportMethod.Filename ? card.RTSPUrl : card.TimeShiftFileName;
            } else if(StreamType == StreamSource.Recording) {
                WebRecording recording = Server.GetRecordings().Where(rec => rec.IdRecording == SourceId).First();
                path = recording.FileName;
            }

            // setup the encoder and input/output streams
            if(config.inputMethod == TransportMethod.Filename && config.useTranscoding) {
                encoder = new EncoderWrapper(path, config);
            } else {
                sourceStream = new TsBuffer(path);
                encoder = new EncoderWrapper(sourceStream, config);
            }
            if (config.useTranscoding) {
                outStream = encoder;
            } else {
                outStream = sourceStream;
            }

            // stream it to the client
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
            // get transcoder
            Configuration config = new Configuration();
            TranscoderProfile transcoder = null;
            foreach (TranscoderProfile profile in config.Transcoders) {
                if (profile.Name == context.Request.Params["transcoder"]) {
                    transcoder = profile;
                    break;
                }
            }
            if (transcoder == null) {
                context.Response.Write("Specify a transcoder");
                return;
            }

            // parse request parameters and start streamer
            ITVEInteraction tvServiceInterface = ChannelFactory<ITVEInteraction>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 },
                new EndpointAddress("net.pipe://localhost/TV4Home.Server.CoreService/TVEInteractionService"));
            Streamer streamer;
            if (context.Request.Params["channelId"] != null) {
                streamer = new Streamer(context.Response, tvServiceInterface, tvServiceInterface.GetChannelBasicById(Int32.Parse(context.Request.Params["channelId"])), transcoder);
            } else if (context.Request.Params["recordingId"] != null) {
                int recordingId = Int32.Parse(context.Request.Params["recordingId"]);
                streamer = new Streamer(context.Response, tvServiceInterface, tvServiceInterface.GetRecordings().Where(rec => rec.IdRecording == recordingId).First(), transcoder);
            } else {
                context.Response.Write("Specify at least a channelId or recordingId parameter");
                return;
            }

            // run
            streamer.stream();
        }
    }
}