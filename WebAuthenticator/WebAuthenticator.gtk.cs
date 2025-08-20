using Microsoft.Maui.ApplicationModel;
using System.Net;
using System.Text;

namespace Microsoft.Maui.Authentication
{
    class WebAuthenticatorImplementation : IWebAuthenticator, IPlatformWebAuthenticatorCallback
    {
        public async Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
        {
            using var listener = new HttpListener();

            listener.Prefixes.Add(webAuthenticatorOptions.CallbackUrl.OriginalString);
            listener.Start();

            await Browser.OpenAsync(Uri.EscapeUriString(webAuthenticatorOptions.Url.OriginalString));

            var cancelToken = new CancellationTokenSource();
            var context = await listener.GetContextAsync().WaitAsync(TimeSpan.FromMinutes(1), cancelToken.Token);

            var response = context.Response;
            string responseString = "<html><head><style>h1{color:green;font-size:20px;}</style></head><body><h1>You can now close this window.</h1></body></html>";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            await responseOutput.WriteAsync(buffer, 0, buffer.Length);
            responseOutput.Close();
            listener.Stop();

            if (webAuthenticatorOptions.ResponseDecoder is not null)
            {
                var dictionary = webAuthenticatorOptions.ResponseDecoder.DecodeResponse(context.Request.Url);
                return new WebAuthenticatorResult(dictionary);
            }
            return new WebAuthenticatorResult(context.Request.Url);
        }
    }
}

