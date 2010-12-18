using System;
using System.Web;

// FIXME: this should be done using some ASP.NET thing
namespace MPWebStream.Site {
    public class Authentication {
        public static bool authenticate(HttpContext context) {
            bool hasAccess = false;

            if (context.Request.Headers["Authorization"] != null) {
                byte[] decodebuffer = Convert.FromBase64String(context.Request.Headers["Authorization"].Substring(6).Trim());
                string input = System.Text.Encoding.ASCII.GetString(decodebuffer);
                Configuration config = new Configuration();
                if (input == config.Username + ":" + config.Password)
                    hasAccess = true;
            }

            if(!hasAccess) {
                context.Response.StatusCode = 401;
                context.Response.StatusDescription = "Authorization Required";
                context.Response.AddHeader("WWW-Authenticate", "Basic realm=\"MPWebStream\"");
                context.Response.AddHeader("Content-Type", "text/plain");
                context.Response.Write("Authorization Required");
                context.Response.Flush();
                return false;
            }
            return true;
        }
    }
}