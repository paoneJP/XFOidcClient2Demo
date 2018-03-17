/*
 *  IdendityModel.OidcClient2 Helper Library for Xamarin.Forms
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using Android.Content;
using IdentityModel.OidcClient.Browser;
using paoneJP.OidcClient2Helper.Droid;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(BrowserImpl))]
namespace paoneJP.OidcClient2Helper.Droid
{
    public class BrowserImpl : IBrowserEx
    {
        public const string StartUrlKey = "startUrl";
        public const string UserCancelResponse = "UserCancel";
        public string CustomUrlScheme { private get; set; }
        public string RedirectUri { private get; set; }
        public event Action<string> BeforeAuthzRequestHooks;
        public event Func<string, Task> BeforeAuthzRequestAsyncHooks;
        public event Action<string, string> AfterAuthzResponseHooks;
        public event Func<string, string, Task> AfterAuthzResponseAsyncHooks;

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options)
        {
            var tcs = new TaskCompletionSource<BrowserResult>();
            await InvokeBeforeAuthzRequestHooksAsync(options.StartUrl);

            var callback = (Action<string>)null;
            callback = async (response) => {
                BrowserActivity.Callbacks -= callback;
                if (response == UserCancelResponse) {
                    await InvokeAfterAuthzResponseHooksAsync(null, "UserCancel");
                    tcs.SetResult(new BrowserResult { ResultType = BrowserResultType.UserCancel });
                } else {
                    if (!response.StartsWith(RedirectUri + "#") && !response.StartsWith(RedirectUri + "?")) {
                        await InvokeAfterAuthzResponseHooksAsync(null, response);
                        tcs.SetResult(new BrowserResult { ResultType = BrowserResultType.UnknownError });
                    }
                    await InvokeAfterAuthzResponseHooksAsync(response, null);
                    tcs.SetResult(new BrowserResult { ResultType = BrowserResultType.Success, Response = response });
                }
            };
            BrowserActivity.Callbacks += callback;

            var context = Android.App.Application.Context;
            var intent = new Intent(context, typeof(BrowserActivity));
            intent.PutExtra(StartUrlKey, options.StartUrl);
            context.StartActivity(intent);
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