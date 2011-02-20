using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MPWebStream.Site {
    public partial class VLC : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            MediaStream control = new MediaStream();
            Channel channel = control.GetChannel(Int32.Parse(HttpContext.Current.Request.Params["channel"]));
            Configuration config = new Configuration();
            url.Value = control.GetTvStreamUrl(channel.IdChannel, config.Username, config.Password);
            name.Value = channel.DisplayName;
        }
    }
}