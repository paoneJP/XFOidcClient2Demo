/*
 *  Xamarin.Forms + IdendityModel.OidcClient2 demonstration application.
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using Android.OS;
using Android.Security;
using Android.Security.Keystore;
using Android.Util;
using Java.Math;
using Java.Security;
using Javax.Crypto;
using Javax.Crypto.Spec;
using Javax.Security.Auth.X500;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using Xamarin.Forms;
using XFOidcClient2Demo.Droid;

[assembly: Dependency(typeof(CryptoUtilImpl))]
namespace XFOidcClient2Demo.Droid
{
    public class CryptoUtilImpl : ICrypto
    {
        private const Base64Flags BASE64_FLAGS = Base64Flags.NoWrap | Base64Flags.NoPadding;

        private const string DATA_ENC_KEY_ALIAS = "data_encrytpion_key";

        private const string KEY_ENC_KEY_ALIAS = "key_encryption_key";
        private const string KEY_ENC_KEY_SUBJECT = "CN=KeyEncryptionKey";
        private const int KEY_ENC_KEY_VALIDITY_YEARS = 10;
        private const long KEY_ENC_KEY_SERIAL_NUMBER = 1;

        public string EncryptString(string data)
        {
            if (data == null) {
                return null;
            }
            try {
                var key = GetDataEncryptionKey();
                var c = Cipher.GetInstance("AES/CBC/PKCS7Padding");
                c.Init(CipherMode.EncryptMode, key);
                return String.Format("{0}:{1}",
                    Base64.EncodeToString(c.GetIV(), BASE64_FLAGS),
                    Base64.EncodeToString(c.DoFinal(Encoding.UTF8.GetBytes(data)), BASE64_FLAGS));
            } catch {
                return null;
            }
        }

        public string DecryptString(string data)
        {
            if (data == null) {
                return null;
            }
            try {
                var d = data.Split(":".ToCharArray(), 2);
                var key = GetDataEncryptionKey();
                var c = Cipher.GetInstance("AES/CBC/PKCS7Padding");
                c.Init(CipherMode.DecryptMode, key,
                    new IvParameterSpec(Base64.Decode(Encoding.UTF8.GetBytes(d[0]), BASE64_FLAGS)));
                return Encoding.UTF8.GetString(c.DoFinal(Base64.Decode(Encoding.UTF8.GetBytes(d[1]), BASE64_FLAGS)));
            } catch {
                return null;
            }
        }

        private ISecretKey GetDataEncryptionKey()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M) {
                return GetDataEncryptionKeyAPI23orHigher();
            } else {
                return GetDataEncryptionKeyAPI22orLower();
            }
        }

        private ISecretKey GetDataEncryptionKeyAPI23orHigher()
        {
            var ks = KeyStore.GetInstance("AndroidKeyStore");
            ks.Load(null);
            if (ks.IsKeyEntry(DATA_ENC_KEY_ALIAS)) {
                var ke = ks.GetEntry(DATA_ENC_KEY_ALIAS, null) as KeyStore.SecretKeyEntry;
                return ke.SecretKey;
            } else {
                var kg = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, "AndroidKeyStore");
                kg.Init(new KeyGenParameterSpec.Builder(DATA_ENC_KEY_ALIAS, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                    .SetKeySize(256)
                    .SetBlockModes(KeyProperties.BlockModeCbc)
                    .SetEncryptionPaddings(KeyProperties.EncryptionPaddingPkcs7)
                    .Build());
                var key = kg.GenerateKey();
                System.Diagnostics.Debug.WriteLine("CryptoImpl.cs: A new data encryption key was generated.");
                return key;
            }
        }

        private ISecretKey GetDataEncryptionKeyAPI22orLower()
        {
            KeyPair GetKeyEncryptionKeyPair()
            {
                var ks = KeyStore.GetInstance("AndroidKeyStore");
                ks.Load(null);
                if (ks.IsKeyEntry(KEY_ENC_KEY_ALIAS)) {
                    var ke = ks.GetEntry(KEY_ENC_KEY_ALIAS, null) as KeyStore.PrivateKeyEntry;
                    return new KeyPair(ke.Certificate.PublicKey, ke.PrivateKey);
                } else {
                    var kpg = KeyPairGenerator.GetInstance("RSA", "AndroidKeyStore");
                    var startDate = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default);
                    var endDate = Java.Util.Calendar.GetInstance(Java.Util.TimeZone.Default);
                    endDate.Add(Java.Util.CalendarField.Year, KEY_ENC_KEY_VALIDITY_YEARS);
#pragma warning disable CS0618
                    kpg.Initialize(new KeyPairGeneratorSpec.Builder(Android.App.Application.Context)
#pragma warning restore CS0618
                        .SetAlias(KEY_ENC_KEY_ALIAS)
                        .SetKeySize(2048)
                        .SetSubject(new X500Principal(KEY_ENC_KEY_SUBJECT))
                        .SetSerialNumber(new BigInteger(BitConverter.GetBytes(KEY_ENC_KEY_SERIAL_NUMBER)))
                        .SetStartDate(startDate.Time)
                        .SetEndDate(endDate.Time)
                        .Build());
                    var kp = kpg.GenerateKeyPair();
                    System.Diagnostics.Debug.WriteLine("CryptoImpl.cs: A new key encryption key pair was generated.");
                    return kp;
                }
            }

            string EncryptAesKey(byte[] k)
            {
                try {
                    var kp = GetKeyEncryptionKeyPair();
                    var c = Cipher.GetInstance("RSA/ECB/PKCS1Padding");
                    c.Init(CipherMode.EncryptMode, kp.Public);
                    return Base64.EncodeToString(c.DoFinal(k), BASE64_FLAGS);
                } catch {
                    return null;
                }
            }

            byte[] DecryptAesKey(string k)
            {
                try {
                    var kp = GetKeyEncryptionKeyPair();
                    var c = Cipher.GetInstance("RSA/ECB/PKCS1Padding");
                    c.Init(CipherMode.DecryptMode, kp.Private);
                    return c.DoFinal(Base64.Decode(k, BASE64_FLAGS));
                } catch {
                    return null;
                }
            }

            void SaveAesKey(string k)
            {
                var file = IsolatedStorageFile.GetUserStoreForApplication();
                using (var stream = file.CreateFile("aeskey.txt")) {
                    using (var writer = new StreamWriter(stream)) {
                        writer.Write(k);
                    }
                }
            }

            string LoadAesKey()
            {
                var file = IsolatedStorageFile.GetUserStoreForApplication();
                try {
                    using (var stream = file.OpenFile("aeskey.txt", FileMode.Open)) {
                        using (var reader = new StreamReader(stream)) {
                            return reader.ReadToEnd();
                        }
                    }
                } catch (FileNotFoundException) { }
                return null;
            }

            var key = DecryptAesKey(LoadAesKey());
            if (key == null) {
                key = new byte[16];
                SecureRandom.GetInstance("SHA1PRNG").NextBytes(key);
                SaveAesKey(EncryptAesKey(key));
                System.Diagnostics.Debug.WriteLine("CryptoImpl.cs: A new data encryption key was generated.");
            }
            return new SecretKeySpec(key, "AES");
        }
    }
}