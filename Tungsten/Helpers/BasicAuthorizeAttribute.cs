using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Tungsten.Helpers {
    public class BasicAuthorizeAttribute : ActionFilterAttribute {
        public string Username { get; set; }
        public string Password { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context) {
            var request = context.HttpContext.Request;
            var authHeader = request.Headers.Authorization;

            if (!string.IsNullOrEmpty(authHeader)) {
                string auth = authHeader.ToString();
                if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Basic ")) {
                    var encodedCreds = auth.Substring(6);
                    var decoded = System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(encodedCreds));
                    var parts = decoded.Split(':');
                    if (parts.Length == 2) {
                        var user = parts[0];
                        var pass = parts[1];

                        if (user == Username && pass == Password) {
                            return;
                        }
                    }
                }
            }

            var response = context.HttpContext.Response;
            response.StatusCode = 401;
            response.Headers.TryAdd("WWW-Authenticate", "Basic realm=\"Secure Area\"");
            context.Result = new EmptyResult();
        }
    }
}
