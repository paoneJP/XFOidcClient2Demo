/*
 *  IdendityModel.OidcClient2 Helper Library for Xamarin.Forms
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using Foundation;
using IdentityModel.OidcClient.Browser;
using paoneJP.OidcClient2Helper.iOS;
using SafariServices;
using System;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(BrowserImpl))]
namespace paoneJP.OidcClient2Helper.iOS
{
    class BrowserImpl : IBrowserEx
    {
        public string CustomUrlScheme { private get; set; }
        public string RedirectUri { private get; set; }
        public event Action<string> BeforeAuthzRequestHooks;
        public event Func<string, Task> BeforeAuthzRequestAsyncHooks;
        public event Action<string, string> AfterAuthzResponseHooks;
        public event Func<string, string, Task> AfterAuthzResponseAsyncHooks;
        private SFAuthenticationSession session;

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options)
        {
            var tcs = new TaskCompletionSource<BrowserResult>();
            await InvokeBeforeAuthzRequestHooksAsync(options.StartUrl);

            async void completionHandler(NSUrl url, NSError error)
            {
                session = null;
                if (url == null) {
                    await InvokeAfterAuthzResponseHooksAsync(null, "UserCancel");
                    tcs.SetResult(new BrowserResult { ResultType = BrowserResultType.UserCancel });
                } else {
                    var response = url.AbsoluteString;
                    if (!response.StartsWith(RedirectUri + "#") && !response.StartsWith(RedirectUri + "?")) {
                        await InvokeAfterAuthzResponseHooksAsync(null, response);
                        tcs.SetResult(new BrowserResult { ResultType = BrowserResultType.UnknownError });
                    }
                    await InvokeAfterAuthzResponseHooksAsync(response, null);
                    tcs.SetResult(new BrowserResult { ResultType = BrowserResultType.Success, Response = response });
                }
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0)) {
                session = new SFAuthenticationSession(new NSUrl(options.StartUrl), CustomUrlScheme, completionHandler);
                session.Start();
            } else {
                SafariViewDelegate.GetInstance().Start(new NSUrl(options.StartUrl), CustomUrlScheme, completionHandler);
            }
            return await tcs.Task;
        }

        private async Task InvokeBeforeAuthzRequestHooksAsync(string request)
        {
            BeforeAuthzRequestHooks?.Invoke(request);
            if (BeforeAuthzRequestAsyncHooks != null) {
                await BeforeAuthzRequestAsyncHooks?.Invoke(request);
            }
        }

        private async Task InvokeAfterAuthzResponseHooksAsync(string response, string error)
        {
            AfterAuthzResponseHooks?.Invoke(response, error);
            if (AfterAuthzResponseAsyncHooks != null) {
                await AfterAuthzResponseAsyncHooks?.Invoke(response, error);
            }
        }
    }
}