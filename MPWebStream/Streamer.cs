using System;
using System.Web;
using System.ServiceModel;
using MPWebStream.TVEInteraction;

namespace MPWebStream {
    public class Streamer {
        public static void run(HttpResponse Response) {
            // connect to TV4Home service
            // FIXME: make hostname configurable
            EndpointAddress address = new EndpointAddress(String.Format("http://{0}:4321/TV4Home.Server.CoreService/TVEInteractionService", "mediastreamer.lan"));
            TVEInteractionClient tvWebClient = new TVEInteractionClient("BasicHttpBinding_ITVEInteraction", address);
        }
    }
}