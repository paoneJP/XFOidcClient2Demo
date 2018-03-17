/*
 *  Xamarin.Forms + IdendityModel.OidcClient2 demonstration application.
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using IdentityModel.Client;
using IdentityModel.OidcClient;
using Newtonsoft.Json;
using paoneJP.OidcClient2Helper;
using System;
using System.Net;
using Xamarin.Forms;

namespace XFOidcClient2Demo
{
    public partial class MainPage : ContentPage
    {
        private AuthState authState;

        public MainPage()
        {
            authState = App.AuthState;

            InitializeComponent();

            ResultLabel.Text = StringResources.MsgAppStart;
            ShowAuthState();
        }

        protected async void OnSigninButtonClicked(object sender, EventArgs e)
        {
            if (authState.IsAuthorized) {
                ResultLabel.Text = StringResources.MsgAlreadyAuthed;
                return;
            }

            try {
                var result = await authState.Client.LoginAsync(new LoginRequest());
                authState.Update(result);
            } catch (Exception ex) {
                authState.Update(ex);
            }

            if (authState.IsAuthorized) {

                #region // Write program to be executed when authorization succeeds.
                ResultLabel.Text = StringResources.MsgAuthOk;
                ShowAuthState();
                #endregion

            } else {

                #region // Write program to be executed when authorization fails.
                ResultLabel.Text = StringResources.MsgAuthNg;
                ShowAuthState();
                #endregion

            }
        }

        protected async void OnCallApiButtonClicked(object sender, EventArgs e)
        {
            var result = await HttpUtil.HttpGetJsonAsync(App.API_URI, authState);

            // Process an API response.
            if (result.Code == HttpUtil.X_HTTP_NEED_REAUTHZ) {

                #region // Write program to be executed when reauthorization required.
                ResultLabel.Text = StringResources.MsgReauthzRequired + "\n\n"
                    + JsonConvert.SerializeObject(result.Json, Formatting.Indented);
                ShowAuthState();
                #endregion

            } else if (result.Code == HttpStatusCode.OK) {

                #region // Write program to be executed when API execution succeeds.
                ResultLabel.Text = StringResources.MsgApiOk + "\n\n"
                    + JsonConvert.SerializeObject(result.Json, Formatting.Indented);
                ShowAuthState();
                #endregion

            } else {

                #region // Write program to be executed when API execution fails.
                ResultLabel.Text = StringResources.MsgApiNg + "\n\n"
                    + JsonConvert.SerializeObject(result.Json, Formatting.Indented);
                ShowAuthState();
                #endregion

            }

        }

        protected void OnTokenRefreshButtonClicked(object sender, EventArgs e)
        {
            authState.NeedsTokenRefresh = true;
            OnCallApiButtonClicked(sender, e);
        }

        protected async void OnSignoutButtonClicked(object sender, EventArgs e)
        {
            if (authState.RefreshToken == null && authState.AccessToken == null) {
                return;
            }
            var endpoint = await authState.GetRevocationEndpointAsync();
            if (endpoint != null) {

#if true
                // Use custom HttpClientHandler for demonstration. (next 4 lines)
                var handler = new HttpClientHandlerEx();
                handler.BeforeRequestAsyncHooks += LogPage.HttpRequestLoggerAsync;
                handler.AfterResponseAsyncHooks += LogPage.HttpResponseLoggerAsync;
                var client = new TokenRevocationClient(endpoint, innerHttpMessageHandler: handler);
#else
                // This is normal implementation.
                var client = new TokenRevocationClient(endpoint);
#endif

                var result = await client.RevokeAsync(new TokenRevocationRequest {
                    Token = authState.RefreshToken ?? authState.AccessToken
                });

#if true
                // Google Accounts will return an "invalid_token" error on HTTP 400, not HTTP 200,
                // in response to a revocation request for a token that has already been revoked.
                if (result.IsError && result.Error != "invalid_token") {
#else
                // This is normal implementation.
                if (result.IsError) {
#endif

                    #region // Write program to be executed when revoking authorization fails.
                    ShowAuthState();
                    ResultLabel.Text = StringResources.MsgAuthRevokeNg;
                    #endregion

                    return;
                }
            }

            #region // Program to be executed when revoking authorization succeeds.
            authState.Reset();
            ShowAuthState();
            ResultLabel.Text = StringResources.MsgAuthRevokeOk;
            #endregion

        }

        protected void OnShowAuthStateButtonClicked(object sender, EventArgs e)
        {
            ShowAuthState();
            ResultLabel.Text = StringResources.MsgShowAuthState;
        }

        private void ShowAuthState()
        {
            AuthStateFullLabel.Text = JsonConvert.SerializeObject(authState, Formatting.Indented);
            AuthStateSummaryLabel.Text = JsonConvert.SerializeObject(new {
                authState.AccessToken,
                authState.AccessTokenExpirationTime,
                authState.RefreshToken,
                authState.IsAuthorized,
                authState.NeedsTokenRefresh
            }, Formatting.Indented);
        }
    }
}