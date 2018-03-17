/*
 *  Xamarin.Forms + IdendityModel.OidcClient2 demonstration application.
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using IdentityModel.OidcClient;
using paoneJP.OidcClient2Helper;
using Serilog;
using System.IO;
using System.IO.IsolatedStorage;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace XFOidcClient2Demo
{
    public partial class App : Application
    {
        internal const string CUSTOM_URL_SCHEME = "net.paonejp.tsxjxb.xfoidcclient2demo";

#if __ANDROID__
        private const string CLIENT_ID_ANDROID = "999999999999-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx.apps.googleusercontent.com";
#elif __IOS__
        private const string CLIENT_ID_IOS = "999999999999-yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy.apps.googleusercontent.com";
#endif

        private const string ISSUER_URI = "https://accounts.google.com";
        private const string REDIRECT_URI = CUSTOM_URL_SCHEME + ":/cb";
        private const string SCOPE = "profile";

        internal const string API_URI = "https://www.googleapis.com/oauth2/v3/userinfo";
        private const string PREFERENCE_FILE_NAME = "authstate.json";

        internal static AuthState AuthState { get; private set; }

        public App()
        {
            StringResources.Culture = DependencyService.Get<ILocale>().GetLanguage();

            AuthState = new AuthState();
            AuthState.UpdateHooks += SaveAuthState;

            var browser = DependencyService.Get<IBrowserEx>(DependencyFetchTarget.NewInstance);
            browser.CustomUrlScheme = CUSTOM_URL_SCHEME;
            browser.RedirectUri = REDIRECT_URI;

            // Setup for demonstration. (next 2 lines)
            browser.BeforeAuthzRequestHooks += LogPage.AuthzRequestLogger;
            browser.AfterAuthzResponseHooks += LogPage.AuthzResponseLogger;

            var options = new OidcClientOptions {
                Authority = ISSUER_URI,
#if __ANDROID__
                ClientId = CLIENT_ID_ANDROID,
#elif __IOS__
                ClientId = CLIENT_ID_IOS,
#endif
                Scope = SCOPE,
                RedirectUri = REDIRECT_URI,
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
                RefreshDiscoveryDocumentForLogin = false,
                Browser = browser
            };

            // Google Accounts has endpoints provided from www.googleapis.com in addition to accounts.google.com.
            options.Policy.Discovery.AdditionalEndpointBaseAddresses = new[] { "https://www.googleapis.com" };

            // Setup for demonstration. (next 4 lines)
            var handler = new HttpClientHandlerEx();
            handler.BeforeRequestAsyncHooks += LogPage.HttpRequestLoggerAsync;
            handler.AfterResponseAsyncHooks += LogPage.HttpResponseLoggerAsync;
            options.BackchannelHandler = handler;

            // Setup for debugging. (next 5 lines)
            var serilog = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Debug()
                .CreateLogger();
            options.LoggerFactory.AddSerilog(serilog);

            var client = new OidcClient(options);
            AuthState.Update(client);

            var file = IsolatedStorageFile.GetUserStoreForApplication();
            try {
                using (var stream = file.OpenFile(PREFERENCE_FILE_NAME, FileMode.Open)) {
                    using (var reader = new StreamReader(stream)) {
                        var crypto = DependencyService.Get<ICrypto>();
                        AuthState.LoadFromJson(crypto.DecryptString(reader.ReadToEnd()));
                    }
                }
            } catch (FileNotFoundException) { }

            InitializeComponent();
            MainPage = new MasterDetail();
        }

        //protected override void OnStart()
        //{
        //    // Handle when your app starts
        //}

        //protected override void OnSleep()
        //{
        //    // Handle when your app sleeps
        //}

        //protected override void OnResume()
        //{
        //    // Handle when your app resumes
        //}

        private void SaveAuthState()
        {
            var file = IsolatedStorageFile.GetUserStoreForApplication();
            using (var stream = file.CreateFile(PREFERENCE_FILE_NAME)) {
                using (var writer = new StreamWriter(stream)) {
                    var crypto = DependencyService.Get<ICrypto>();
                    writer.Write(crypto.EncryptString(AuthState.SaveToJson()));
                }
            }
        }
    }
}