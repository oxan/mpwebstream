using System.Web;

namespace MPWebStream.Site.Pages {
    public class Stream : IHttpHandler {
        public void ProcessRequest(HttpContext context) {
            if (!Authentication.authenticate(context, true))
                return;

            Streamer.run(context);
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}