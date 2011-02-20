using System;
using System.Web;
using System.Web.UI.WebControls;

namespace MPWebStream.Site {
    public partial class Default : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            // this is probably not the way you should do it, but I'm not a ASP.NET coder and I don't have time to learn it. Suggestions or patches welcome. 
            if (!Authentication.authenticate(HttpContext.Current))
                return;

            Configuration config = new Configuration();
            MediaStream remoteControl = new MediaStream();
            foreach (Channel channel in remoteControl.GetChannels()) {
                Channels.Items.Add(new ListItem(channel.DisplayName, remoteControl.GetTvStreamUrl(channel.IdChannel, config.Username, config.Password)));
            }
            Channels.DisplayMode = BulletedListDisplayMode.HyperLink;
        }
    }
}