/*
 *  IdendityModel.OidcClient2 Helper Library for Xamarin.Forms
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using Foundation;
using SafariServices;
using UIKit;

namespace paoneJP.OidcClient2Helper.iOS
{
    class SafariViewDelegate : SFSafariViewControllerDelegate
    {
        private static SafariViewDelegate instance = new SafariViewDelegate();
        private CompletionHandler completionHandler;
        private SFSafariViewController safariView;

        internal static SafariViewDelegate GetInstance()
        {
            return instance;
        }

        private SafariViewDelegate() { }

        public delegate void CompletionHandler(NSUrl url, NSError error);

        public void Start(NSUrl url, string scheme, CompletionHandler handler)
        {
            completionHandler = handler;
            var root = UIApplication.SharedApplication.KeyWindow.RootViewController;
            safariView = new SFSafariViewController(url) {
                Delegate = this
            };
            root.PresentViewController(safariView, true, null);
        }

        public async void ProcessAuthorizationResponse(NSUrl url)
        {
            await safariView?.DismissViewControllerAsync(true);
            completionHandler(url, null);
        }

        public override void DidFinish(SFSafariViewController controller)
        {
            completionHandler(null, new NSError());
        }
    }
}