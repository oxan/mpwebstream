using System;
using System.Web;
using System.Web.UI.WebControls;
using System.Collections.Specialized;

namespace MPWebStream.Site {
    public partial class Default : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            // this whole site probably qualifies for The Daily WTF, but I'm not a ASP.NET coder and I don't have enough time to learn it. Suggestions and patches welcome.
            if (!Authentication.authenticate(HttpContext.Current))
                return;

            Configuration config = new Configuration();
            MediaStream remoteControl = new MediaStream();
            foreach (Channel channel in remoteControl.GetChannels()) {
                if(config.StreamType == Configuration.StreamlinkType.VLC) {
                    Channels.Items.Add(new ListItem(channel.DisplayName, "VLC.aspx?channel=" + channel.IdChannel.ToString()));
                } else {
                    Channels.Items.Add(new ListItem(channel.DisplayName, remoteControl.GetTvStreamUrl(channel.IdChannel, config.Username, config.Password)));
                }
            }
            Channels.DisplayMode = BulletedListDisplayMode.HyperLink;
        }
    }
}