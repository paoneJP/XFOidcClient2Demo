/*
 *  IdendityModel.OidcClient2 Helper Library for Xamarin.Forms
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace paoneJP.OidcClient2Helper
{
    class HttpClientHandlerEx : HttpClientHandler
    {
        public event Action<HttpRequestMessage> BeforeRequestHooks;
        public event Func<HttpRequestMessage, Task> BeforeRequestAsyncHooks;
        public event Action<HttpResponseMessage> AfterResponseHooks;
        public event Func<HttpResponseMessage, Task> AfterResponseAsyncHooks;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await InvokeBeforeRequestHooksAsync(request);
            var response = await base.SendAsync(request, cancellationToken);
            await InvokeAfterResponseHooksAsync(response);
            return response;
        }

        private async Task InvokeBeforeRequestHooksAsync(HttpRequestMessage request)
        {
            BeforeRequestHooks?.Invoke(request);
            if (BeforeRequestAsyncHooks != null) {
                await BeforeRequestAsyncHooks?.Invoke(request);
            }
        }

        private async Task InvokeAfterResponseHooksAsync(HttpResponseMessage response)
        {
            AfterResponseHooks?.Invoke(response);
            if (AfterResponseAsyncHooks != null) {
                await AfterResponseAsyncHooks?.Invoke(response);
            }
        }
    }
}