# Xamarin.Forms + IdentityModel.OidcClient2 ネイティブアプリ実装サンプル

これは OAuth 2.0 で保護されたバックエンドAPIを使用する Xamarin.Forms を使ったクロスプラットフォームネイティブアプリの実装サンプルです。

ネイティブアプリでの OAuth 2.0 の実装については [RFC 8252][BCP 212] OAuth 2.0 for Native Apps として現時点のベストプラクティスがまとめられています。
今回は C# 向けの OpenID Certified ライブラリ IdentityModel.OidcClient2 を用いて OAuth 2.0 for Natvie Apps のプラクティスに沿った実装をしてみます。

  > 内容は以前に作成した「Kotlin + AppAuth for Android ネイティブアプリ実装サンプル」を Xamarin.Forms と IdentityModel.OidcClient2 で実装しなおしたものになっています。
  > 以下の情報も参考にしてください。
  >
  >  * Kotlin + AppAuth for Android ネイティブアプリ実装サンプル
  >     - https://github.com/paoneJP/AppAuthDemo-Android
  >  * Kotlin と AppAuth for Android でネイティブアプリの実装サンプルを作ってみた
  >     - https://paonejp.github.io/2017/11/04/making_kotlin_appauth_android_demo_application.html


## 開発環境等

 * Visual Studio Community 2017 (Version 15.6.2)
 * Visual Studio Community 2017 for Mac (Version 7.4)
 * IdentityModel.OidcClient2 (Version 2.5.1)
 * Shared Projects 形式
 * Android API Level 21 以上 (Android 5.0 以上)
 * iOS 10 以上


## 動作確認状況

 * Android Debugビルド + Android エミュレーター (Android 7.1.1)
 * Android Debugビルド + Android エミュレーター (Android 5.0)
 * Android Releaseビルド (アドホック) + Android 実機 (Android 7.1.2)
 * iOS Debugビルド + iPhone シミュレーター (iPhone X + iOS 11.2)
 * iOS Debugビルド + iPhone シミュレーター (iPhone 7 + iOS 10.3.1)


## 実装されている機能

 * Google Accounts 使ったサインイン (OAuth 2.0 Authorization)
 * Google Accounts の UserInfo エンドポイントをバックエンドAPIに見立てたAPIアクセス
 * リフレッシュトークンを使ったアクセストークンの更新
 * IsolatedStorageFile を使ったアクセストークン、リフレッシュトークンの保存とその際の暗号化
 * サインアウトとその際のトークン失効 (OAuth 2.0 Token Revocation)
 * その他補助的な機能として
    - 強制的にアクセストークンを更新
    - 認証要求と応答、HTTP要求と応答のログ表示


## 遊び方

 * Google Cloud Platform の「APIとサービス」でプロジェクトを作成します。
 * 「認証情報を作成」で OAuthクライアントID を作成します。アプリケーションの種類に Android を選択します。
 * 発行されたクライアントIDを確認します。
 * 「認証情報を作成」でもう一つ OAuthクライアントID を作成します。アプリケーションの種類に iOS を選択します。
 * 発行されたクライアントIDを確認します。
 * Visual Studio でこのプロジェクトを開きます。
 * `App.xaml.cs` の `CLIENT_ID_ANDROID`, `CLIENT_ID_IOS` の値に、先に確認したクライアントIDの値を設定しビルド、実行します。

取り急ぎ動作の確認をしたい方のために、Android用のビルド済み apk ファイルを built ディレクトリに収録しています。


## 操作

* 「サインイン」
  - Google Accounts で認証を行ないアクセストークン、リフレッシュトークンを取得します。
  - 内部状態を authState (Summary) および authState (Full) エリアに表示します。
* 「API呼出し」
  - Google Accounts の UserInfo エンドポイントへAPIアクセスし、その結果を Response エリアに表示します。
* 「サインアウト」
  - Google Accounts にアクセストークン、リフレッシュトークンの失効を要求し、内部状態を初期化します。
* 「認証状態表示」
  - 内部状態を authStatre (Summary) および authState (Full) エリアに表示します。
* 「トークン強制更新」
  - 内部状態の `NeedsTokenRefresh` を `true` にしてAPI呼出しを実行します。アクセストークンが強制的に更新されてからAPIが呼び出されます。
* メニューから「ログを表示」を選ぶと、リクエスト、レスポンスのログを表示できます。

ネットワークアクセスができない状況や Google Accouns の「アカウントにアクセスできるアプリ」でアクセス権を削除した状態などで動作を試してみると良いでしょう。


## 実装上のポイント

### 認証状態を保持する AuthState クラスを実装

 * IdentityModel.OidcClient2 は OpenID Connect および OAuth のフローを実行することに特化したライブラリとなっており、認証（認可）状態を管理する機能は提供されていません。
 * ネイティブアプリを実装する場合、その状態管理が必要となるので AuthState クラスとして機能を実装しています。
 * `IsAuthorized`, `NeedsTokenRefresh` の振る舞いは、 AppAuth for Android の振る舞いと合わせています。（実装を参考にしました）

### ブラウザ動作を制御するクラスを実装

 * IdentityModel.OidcClient2 では、認証（認可）フローで必要なブラウザの制御を行なう機能が提供されておらず、アプリケーション側で実装をする必要があります。
 * OAuth 2.0 for Native Apps のプラクティスに沿うには、WebViewではなく、外部ブラウザや Android の CustomTabs、iOS の SFSafariViewController, SFAuthenticationSession などを使う必要があります。
 * 今回の実装では、プラットフォームごとにブラウザを制御する BrowserImpl クラスを実装し、Xamarin.Forms から DependencyService を使って呼び出しています。

#### Android用のブラウザ制御

 * プラットフォームが CustomTabs をサポートする場合は CustomTabsActivityManager を使ってブラウザを起動しています。
   この時、デフォルトブラウザが優先されるようになっており、そのブラウザが CustomTabs をサポートしない場合は、ブラウザが普通に起動されます。
 * プラットフォームが CustomTabs をサポートしない場合はインテントを使ったデフォルトブラウザ起動を行ないます。
 * ブラウザを閉じて認証（認可）フローをキャンセルするケースを扱えるよう、画面を持たない BrowserActivity を実装し、BrowserActivity 経由でブラウザを起動しています。
 * Callback に使われる Custom URL Scheme は BrowserActivity が受け取ることで、認証（認可）レスポンスを処理できるようにしています。

#### iOS用のブラウザ制御

 * iOS 11 の場合は SFAuthenticationSession を、iOS 10 の場合は SFSafariViewController を使い認証画面を出しています。
 * SFAuthenticationSession は、キャンセル時の動作を含めこのクラスがいい感じに処理をしてくれます。
 * SFSafariViewController を使うケースは、キャンセル時の処理や Custom URL Scheme を受け取る処理を実装する必要があります。
   SFAuthenticationSession に近い動作をする SafariViewDelegate を実装することで対応しています。

### PKCEへの対応

 * OAuth 2.0 for Native Apps では、認証（認可）結果をブラウザを経由して安全にネイティブアプリに返すことができるよう、OAuth 2.0 の Authorization Code Grant を使用することと、
   [RFC 7636] Proof Key for Code Exchange by OAuth Public Clients (PKCE) を使用することが示されています。
 * IdentityModel.OidcClient2 は自動的に code challenge method に S256 を用いた PKCE の処理を行なってくれます。
    > ただ IdentityModel.OidcClient2 では PKCE の処理がハードコードされていて、サーバーの PKCE 対応状況によらず
    > S256 で PKCE を使うパラメータを付けてリクエストしてしまっているようですが…。

### アクセストークンの取得、APIアクセス、アクセストークンの失効

 * アクセストークンの取得は `MainPage.xaml.cs` の `OnSigninButtonClicked()` に実装しています。
 * APIアクセスの処理は `MainPage.xaml.cs` の `OnCallApiButtonClicked()` に実装しています。
    - アクセストークンを使いリクエストを送信する機能を `HttpUtil.cs` の `HttpGetJsonAsync()` に実装し、それを使用しています。
    - `HttpGetJsonAsync()` の中で、アクセストークンの期限が切れている場合にリフレッシュトークンを使った更新を行なう処理を実装しています。
 * アクセストークンを失効する処理は `MainPage.xaml.cs` の `OnSignoutButtonClicked()` に実装しています。
    - アクセストークン失効のためのエンドポイント (revocation_endpoint) の情報は、 openid-configuration から取得できますが、
      IdentityModel.OidcClient2 では認証（認可）中に取得した revocation_endpoint の情報を保持していないため、 AuthState クラス内で代替する処理を書いています。
 * いずれの処理についても、アプリケーション固有の振る舞いは `#region` ～ `#endregion` で囲まれた位置に実装することになります。

### アクセストークンの保存 (AuthStateの保存)

 * AuthState が更新された時、 AuthState オブジェクトの `UpdateHooks` に登録した `App.xaml.cs` の `SaveAuthState()` を呼び出し、
   IsolatedStorageFile を使ってアクセストークンやリフレッシュトークンの情報を保存しています。
 * アクセストークンやリフレッシュトークンを暗号化して保存するため、プラットフォームごとに暗号化を行なう CryptoImpl クラスを実装し、
   Xamarin.Forms から DependencyService を使って呼び出しています。

#### Androidでの暗号化処理

 * Android では Android KeyStore System を使った暗号化の処理をしています。
 * API Level 23 以上 (Android 6.0 以上) の場合は、Android KeyStore System で AES/CBC/PKCS7Padding を使った暗号化を行なっています。
 * API Level 21, 22 (Android 5.x) の場合も、AES/CBC/PKCS7Padding で暗号化をしますが、その時に使用する共通鍵の保存が必要になります。
   その共通鍵は Android KeyStore System を使って RSA/ECB/PKCS1Padding を使った暗号化をして保存する対応をしています。

#### iOSでの暗号化処理

 * iOS では KeyChain Services を使い、 RsaEncryptionOaepSha256AesGcm で暗号化の処理をしています。


## 実装上のポイント (Xamarin.Forms的に)

### 多言語表示への対応

 * 以下の Xamarin.Forms のドキュメントとサンプルにあるRESXを用いる方法を参考に多言語表示に対応しています。
    - https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/localization
    - https://github.com/xamarin/xamarin-forms-samples/tree/master/TodoLocalized/SharedProject/
 * Shared Projects形式を使っているため、ドキュメントで触れられている通りUWPに対応できていません。
 * XAMLファイルから多言語文字列にアクセスするため、中継クラスとして XamlStringResources を実装しています。
 * プラットフォームの言語設定を取得するために、プラットフォームごとに LocaleImpl クラスを実装し、Xamarin.Forms から DependencyService を使って呼び出しています。


### Android向けのリリースビルド

 * IdentityModel.OidcClient2 は内部的に Newtonsoft.Json を用いてJSONオブジェクトのシリアライズ、デシリアライズをしています。
 * Android向けにリリースビルドをする際、直接参照されないクラスがリンクされるようにする必要があり、`LinkDescription.xml` を書いて対応しています。


## 対応できてないこと

 * UWP向けのプロジェクトが含まれていますが、UWP向けの実装はまだやっていません。
    - Shared Projects 形式の時の多言語化の方法が UWP とあっていない問題があって、いつ対応できるか不明…。
 * ビルド時に多数の警告が出ていますが、消し込みができていません。特に iOS の方で顕著です。
 * iOS向けにリリースビルドをしたときに、このままで動作するかどうかが未確認です。
 * テストコードが書けていません  :-(


## 参考サイト

 * [RFC 8252][BCP 212] OAuth 2.0 for Native Apps
    - https://datatracker.ietf.org/doc/rfc8252/
    - https://datatracker.ietf.org/doc/bcp212/
 * IdentityModel.OidcClient2
    - https://github.com/IdentityModel/IdentityModel2


## 使用しているアイコンファイルについて

 * この実装サンプルでは Material Design icons by Google サイトで提供されているアイコンを使用しています。
    - https://material.io/icons/


## ライセンス等

 * この実装サンプルのオリジナルは、以下の場所で MIT License で公開しています。 
    - https://github.com/paoneJP/XFOidcClient2Demo
