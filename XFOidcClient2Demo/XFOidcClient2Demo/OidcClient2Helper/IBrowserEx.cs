/*
 *  IdendityModel.OidcClient2 Helper Library for Xamarin.Forms
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using IdentityModel.OidcClient.Browser;
using System;
using System.Threading.Tasks;

namespace paoneJP.OidcClient2Helper
{
    interface IBrowserEx : IBrowser
    {
        string CustomUrlScheme { set; }
        string RedirectUri { set; }
        event Action<string> BeforeAuthzRequestHooks;
        event Func<string, Task> BeforeAuthzRequestAsyncHooks;
        event Action<string, string> AfterAuthzResponseHooks;
        event Func<string, string, Task> AfterAuthzResponseAsyncHooks;
    }
}