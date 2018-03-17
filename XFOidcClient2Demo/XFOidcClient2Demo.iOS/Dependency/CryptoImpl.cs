/*
 *  Xamarin.Forms + IdendityModel.OidcClient2 demonstration application.
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using Foundation;
using Security;
using Xamarin.Forms;
using XFOidcClient2Demo.iOS;

[assembly: Dependency(typeof(CryptoUtilImpl))]
namespace XFOidcClient2Demo.iOS
{
    class CryptoUtilImpl : ICrypto
    {
        private const string APPLICATION_TAG = "DataEncryptionKey";

        public string EncryptString(string data)
        {
            var key = GetDataEncryptionKey();
            if (key == null) {
                return null;
            }
            var alg = SecKeyAlgorithm.RsaEncryptionOaepSha256AesGcm;
            var result = key.GetPublicKey().CreateEncryptedData(alg, data, out var e);
            if (e != null) {
                return null;
            }
            return result.GetBase64EncodedString(NSDataBase64EncodingOptions.None);
        }

        public string DecryptString(string data)
        {
            var key = GetDataEncryptionKey();
            if (key == null) {
                return null;
            }
            var alg = SecKeyAlgorithm.RsaEncryptionOaepSha256AesGcm;
            var ct = new NSData(data, NSDataBase64DecodingOptions.None);
            var result = key.CreateDecryptedData(alg, ct, out var e);
            if (e != null) {
                return null;
            }
            return result.ToString();
        }

        private SecKey GetDataEncryptionKey()
        {
            var query = new SecRecord(SecKind.Key) {
                ApplicationTag = APPLICATION_TAG,
            };

            var keys = SecKeyChain.QueryAsReference(query, 2, out var code);
            if (code == SecStatusCode.Success) {
                return keys[0] as SecKey;
            }

            SecKeyChain.Remove(query);
            var key = SecKey.CreateRandomKey(SecKeyType.RSA, 2048, null, out var e);
            if (e != null) {
                return null;
            }
            var rec = new SecRecord(key) {
                ApplicationTag = APPLICATION_TAG,
                KeyType = SecKeyType.RSA,
                KeyClass = SecKeyClass.Private,
                Accessible = SecAccessible.AfterFirstUnlock
            };
            var r = SecKeyChain.Add(rec);
            if (r != SecStatusCode.Success) {
                System.Diagnostics.Debug.WriteLine($"CryptoImpl.cs: Could not add a new key pair to KeyChain. status = \"{r}\"\n"
                    + "    Please make sure \"Entitlements.plist\" is set for custom entitlements in project property page.");
                return null;
            }
            System.Diagnostics.Debug.WriteLine("CryptoImpl.cs: A new key encryption key pair was generated.");
            return key;
        }
    }
}