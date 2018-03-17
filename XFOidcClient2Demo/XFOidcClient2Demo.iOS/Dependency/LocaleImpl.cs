/*
 *  Xamarin.Forms + IdendityModel.OidcClient2 demonstration application.
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using Foundation;
using System.Globalization;
using Xamarin.Forms;
using XFOidcClient2Demo.iOS;

[assembly: Dependency(typeof(LocaleImpl))]
namespace XFOidcClient2Demo.iOS
{
    class LocaleImpl : ILocale
    {
        public CultureInfo GetLanguage()
        {
            try {
                return new CultureInfo(NSLocale.PreferredLanguages[0].Split("_-".ToCharArray())[0]);
            } catch (CultureNotFoundException) {
                return new CultureInfo("en");
            }
        }
    }
}