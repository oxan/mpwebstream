using System;
using System.Web;
using System.Text;
using System.Security.Cryptography;

// FIXME: this should be done using some ASP.NET thing
namespace MPWebStream.Site {
    public class Authentication {
        public static bool authenticate(HttpContext context, bool allowUrl = false) {
            Configuration config = new Configuration();
            if (!config.EnableAuthentication)
                return true;

            if (context.Request.Headers["Authorization"] != null) {
                byte[] decodebuffer = Convert.FromBase64String(context.Request.Headers["Authorization"].Substring(6).Trim());
                string input = System.Text.Encoding.ASCII.GetString(decodebuffer);
                if (input == config.Username + ":" + config.Password)
                    return true;
            }

            if (allowUrl) {
                string hash = Authentication.createLoginArgument(config.Username, config.Password);
                if (context.Request.Params["login"] != null && context.Request.Params["login"] == hash)
                    return true;
            }

            // no access
            context.Response.StatusCode = 401;
            context.Response.StatusDescription = "Authorization Required";
            context.Response.AddHeader("WWW-Authenticate", "Basic realm=\"MPWebStream\"");
            context.Response.AddHeader("Content-Type", "text/plain");
            context.Response.Write("Authorization Required");
            context.Response.Flush();
            return false;
        }

        public static string createLoginArgument(string username, string password) {
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] token = encoding.GetBytes(username + ":" + password);
            SHA256 hashprovider = new SHA256Managed();
            byte[] hash = hashprovider.ComputeHash(token);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}