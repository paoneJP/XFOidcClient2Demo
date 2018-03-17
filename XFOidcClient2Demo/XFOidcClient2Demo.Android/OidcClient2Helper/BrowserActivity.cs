/*
 *  IdendityModel.OidcClient2 Helper Library for Xamarin.Forms
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Support.CustomTabs;
using System;
using XFOidcClient2Demo;

namespace paoneJP.OidcClient2Helper.Droid
{
    [Activity(Label = "BrowserActivity", LaunchMode = LaunchMode.SingleTask)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = App.CUSTOM_URL_SCHEME)]
    public class BrowserActivity : Activity
    {
        public static event Action<string> Callbacks;
        private static bool started = false;

        protected override void OnResume()
        {
            base.OnResume();

            if (!started) {
                started = true;
                var uri = Android.Net.Uri.Parse(Intent.GetStringExtra(BrowserImpl.StartUrlKey));
                var name = CustomTabsHelper.GetPackageNameToUse(this);
                if (name != null) {
                    var manager = new CustomTabsActivityManager(this);
                    var intent = new CustomTabsIntent.Builder(manager.Session).Build();
                    intent.Intent.SetPackage(name);
                    intent.Intent.AddFlags(ActivityFlags.NoHistory);
                    intent.LaunchUrl(this, uri);
                } else {
                    var intent = new Intent(Intent.ActionView, uri);
                    StartActivity(intent);
                }
                return;
            }

            started = false;
            Callbacks?.Invoke(Intent.DataString ?? BrowserImpl.UserCancelResponse);
            Finish();
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            Intent = intent;
        }
    }
}