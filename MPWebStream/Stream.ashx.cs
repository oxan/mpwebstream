using System.Web;
using System;
using TV4Home.Server.TVEInteractionLibrary.Interfaces;
using MPWebStream;
using System;
using System.IO;
using System.ServiceModel;
using System.Web;
using System.Linq;
using MPWebStream;
using MPWebStream.Streaming;
using TV4Home.Server.TVEInteractionLibrary.Interfaces;

namespace MPWebStream.Site.Pages {
    public class Stream : IHttpHandler {
        public void ProcessRequest(HttpContext context) {
            if (!Authentication.authenticate(context, true))
                return;

            // get transcoder
            Configuration config = new Configuration();
            TranscoderProfile transcoder;
            int transcoderId;
            if (Int32.TryParse(context.Request.Params["transcoder"], out transcoderId) && config.GetTranscoder(transcoderId) != null) {
                transcoder = config.GetTranscoder(transcoderId);
            } else {
                context.Response.Write("Specify a valid transcoder");
                return;
            }

            // parse request parameters and start streamer
            ITVEInteraction tvServiceInterface = ChannelFactory<ITVEInteraction>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 },
                new EndpointAddress("net.pipe://localhost/TV4Home.Server.CoreService/TVEInteractionService"));
            Streamer streamer;
            if (context.Request.Params["channelId"] != null) {
                streamer = new Streamer(tvServiceInterface, tvServiceInterface.GetChannelBasicById(Int32.Parse(context.Request.Params["channelId"])), transcoder);
            } else if (context.Request.Params["recordingId"] != null) {
                int recordingId = Int32.Parse(context.Request.Params["recordingId"]);
                streamer = new Streamer(tvServiceInterface, tvServiceInterface.GetRecordings().Where(rec => rec.IdRecording == recordingId).First(), transcoder);
            } else {
                context.Response.Write("Specify at least a channelId or recordingId parameter");
                return;
            }

            // run
            streamer.startTranscoding();
            streamer.streamToClient(context.Response);
            streamer.finishTranscoding();
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}