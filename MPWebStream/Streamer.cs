using System;
using System.Web;

namespace MPWebStream {
    public class Streamer {
        public static void run(HttpResponse Response) {
            Response.Write("HEllo!");
        }
    }
}