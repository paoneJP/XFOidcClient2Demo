/*
 *  Xamarin.Forms + IdendityModel.OidcClient2 demonstration application.
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

namespace XFOidcClient2Demo
{
    interface ICrypto
    {
        string EncryptString(string data);
        string DecryptString(string data);
    }
}