/*
 *  Xamarin.Forms + IdendityModel.OidcClient2 demonstration application.
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using IdentityModel.OidcClient.Results;
using Newtonsoft.Json;
using paoneJP.OidcClient2Helper;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace XFOidcClient2Demo
{
    class HttpUtil
    {
        public const HttpStatusCode X_HTTP_ERROR = (HttpStatusCode)(-1);
        public const HttpStatusCode X_HTTP_NEED_REAUTHZ = (HttpStatusCode)(-9);

        public class HttpGetJsonResult
        {
            public HttpStatusCode Code;
            public Object Json;
        }

        public static async Task<HttpGetJsonResult> HttpGetJsonAsync(string uri, AuthState authState)
        {
            if (!authState.IsAuthorized) {
                return new HttpGetJsonResult { Code = X_HTTP_NEED_REAUTHZ };
            }
            if (authState.NeedsTokenRefresh) {
                RefreshTokenResult result;
                try {
                    result = await authState.Client.RefreshTokenAsync(authState.RefreshToken);
                    authState.Update(result);
                } catch (Exception e) {
                    result = new RefreshTokenResult { Error = e.Message };
                }
                if (result.IsError) {
                    return new HttpGetJsonResult {
                        Code = authState.IsAuthorized ? X_HTTP_ERROR : X_HTTP_NEED_REAUTHZ,
                        Json = new { error = result.Error }
                    };
                }
            }

            try {
#if true
                // Use custom HttpClientHandler for demonstration. (next 4 lines)
                var handler = new HttpClientHandlerEx();
                handler.BeforeRequestAsyncHooks += LogPage.HttpRequestLoggerAsync;
                handler.AfterResponseAsyncHooks += LogPage.HttpResponseLoggerAsync;
                using (var client = new HttpClient(handler: handler)) {
#else
                // This is normal implementation.
                using (var client = new HttpClient()) {
#endif
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.AccessToken);
                    var response = await client.GetAsync(uri);
                    return new HttpGetJsonResult {
                        Code = response.StatusCode == HttpStatusCode.Unauthorized ? X_HTTP_NEED_REAUTHZ : response.StatusCode,
                        Json = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync())
                    };
                }
            } catch (Exception ex) {
                return new HttpGetJsonResult {
                    Code = X_HTTP_ERROR,
                    Json = new { error = ex.GetType().Name, error_description = ex.Message }
                };
            }
        }
    }
}