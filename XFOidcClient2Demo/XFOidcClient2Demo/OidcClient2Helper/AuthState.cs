/*
 *  IdendityModel.OidcClient2 Helper Library for Xamarin.Forms
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using IdentityModel.Client;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Results;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace paoneJP.OidcClient2Helper
{
    class AuthState
    {
        public OidcClient Client { get; private set; }

        public string AccessToken { get; private set; }
        public DateTime AccessTokenExpirationTime { get; private set; }
        public string RefreshToken { get; private set; }
        public string IdToken { get; private set; }
        public List<string> Scope { get; private set; }

        public bool IsAuthorized => AccessToken != null | IdToken != null;

        private bool forceTokenRefresh;
        public bool NeedsTokenRefresh
        {
            get { return forceTokenRefresh | DateTime.Now > AccessTokenExpirationTime; }
            set { forceTokenRefresh = value; }
        }

        public event Action UpdateHooks;

        public void Update(OidcClient client)
        {
            Client = client;
        }

        public async void Update(LoginResult result)
        {
            if (result.IsError) {
                Reset();
                return;
            }
            AccessToken = result.AccessToken;
            AccessTokenExpirationTime = result.AccessTokenExpiration;
            RefreshToken = result.RefreshToken;
            IdToken = result.IdentityToken;
            Scope = new List<string>(Client.Options.Scope.Split(" +".ToCharArray()));
            await UpdateRevocationEndpointAsync();
            forceTokenRefresh = false;
            UpdateHooks?.Invoke();
        }

        public async void Update(RefreshTokenResult result)
        {
            if (result.IsError) {
                if (result.Error == "invalid_grant") {
                    Reset();
                }
                return;
            }
            AccessToken = result.AccessToken;
            AccessTokenExpirationTime = DateTime.Now.AddSeconds(result.ExpiresIn);
            if (result.RefreshToken != null) {
                RefreshToken = result.RefreshToken;
            }
            if (result.IdentityToken != null) {
                IdToken = result.IdentityToken;
            }
            await UpdateRevocationEndpointAsync();
            forceTokenRefresh = false;
            UpdateHooks?.Invoke();
        }

        public void Update(Exception ex)
        {
            Reset();
        }

        public void Reset()
        {
            InitializeState();
            UpdateHooks?.Invoke();
        }

        private void InitializeState()
        {
            AccessToken = null;
            AccessTokenExpirationTime = DateTime.MinValue;
            RefreshToken = null;
            IdToken = null;
            Scope = null;
            forceTokenRefresh = false;
        }

        public string SaveToJson()
        {
            return JsonConvert.SerializeObject(new {
                AccessToken,
                AccessTokenExpirationTime,
                RefreshToken,
                IdToken,
                Scope,
                Client.Options.Authority,
                RevocationEndpoint = revocationEndpoint
            });
        }

        public void LoadFromJson(string json)
        {
            try {
                dynamic data = JsonConvert.DeserializeObject(json);
                if (data.Authority == Client.Options.Authority) {
                    AccessToken = data.AccessToken;
                    AccessTokenExpirationTime = data.AccessTokenExpirationTime;
                    RefreshToken = data.RefreshToken;
                    IdToken = data.IdToken;
                    Scope = data.Scope?.ToObject<List<string>>();
                    revocationEndpoint = data.RevocationEndpoint;
                }
            } catch {
                InitializeState();
            }
        }


        private string revocationEndpoint;
        private bool isRevocationEndpointInitialized;

        public async Task<string> GetRevocationEndpointAsync()
        {
            await UpdateRevocationEndpointAsync();
            return revocationEndpoint;
        }

        private async Task UpdateRevocationEndpointAsync()
        {
            if (!isRevocationEndpointInitialized) {
                var client = new DiscoveryClient(Client.Options.Authority) {
                    Policy = Client.Options.Policy.Discovery,
                    Timeout = Client.Options.BackchannelTimeout
                };

                var saved = client.Policy.RequireKeySet;
                client.Policy.RequireKeySet = false;
                var result = await client.GetAsync();
                client.Policy.RequireKeySet = saved;

                if (!result.IsError) {
                    revocationEndpoint = result.RevocationEndpoint;
                    isRevocationEndpointInitialized = true;
                }
            }
        }
    }
}