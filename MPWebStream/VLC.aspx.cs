#region Copyright
/* 
 *  Copyright (C) 2010, 2011 Oxan
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion

using MPWebStream.Site.Service;
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
            url.Value = control.GetTranscodedTvStreamUrl(channel.IdChannel, config.Username, config.Password, Int32.Parse(HttpContext.Current.Request.Params["transcoder"]));
            name.Value = channel.DisplayName;
        }
    }
}