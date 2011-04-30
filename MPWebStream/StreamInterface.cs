using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MPWebStream.Site.Service;

namespace MPWebStream.Site {
    public abstract class StreamInterface : System.Web.UI.Page {
        protected class StreamData {
            public string name;
            public string url;
        }

        protected StreamData parseParameters() {
            StreamData ret = new StreamData();
            MediaStream control = new MediaStream();
            int transcoder = Int32.Parse(HttpContext.Current.Request.Params["transcoder"]);
            Configuration config = new Configuration();

            // live tv
            int channelId = 0;
            Int32.TryParse(HttpContext.Current.Request.Params["channel"], out channelId);
            if (channelId != 0) {
                Channel channel = control.GetChannel(channelId);
                ret.url = control.GetTranscodedTvStreamUrl(channel.IdChannel, config.Username, config.Password, transcoder);
                ret.name = channel.DisplayName;
            }

            // recording
            int recordingId = 0;
            Int32.TryParse(HttpContext.Current.Request.Params["recording"], out recordingId);
            if (recordingId != 0) {
                Recording recording = control.GetRecording(recordingId);
                ret.url = control.GetTranscodedRecordingStreamUrl(recordingId, config.Username, config.Password, transcoder);
                ret.name = recording.Title;
            }

            return ret;
        }
    }
}