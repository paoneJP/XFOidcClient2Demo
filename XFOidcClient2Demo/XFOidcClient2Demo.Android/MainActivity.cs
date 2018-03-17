using Android.App;
using Android.Content.PM;
using Android.OS;

namespace XFOidcClient2Demo.Droid
{
    [Activity(Label = "XFOidcClient2Demo", Icon = "@drawable/icon", Theme = "@style/MainTheme",
        MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            try {
                LoadApplication(new App());
            } catch (System.Exception e) {
                System.Diagnostics.Debug.WriteLine($"{e}");
                throw e;
            }
        }
    }
}