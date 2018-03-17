/*
 *   This is an implementation of the workaround for using localization
 *   in Xamarin.Forms Shared Project.
 *
 *   This workaround is implemented by referring to this example.
 *   https://github.com/xamarin/xamarin-forms-samples/tree/master/TodoLocalized/SharedProject/
 */

using System.Reflection;
using Xamarin.Forms;

namespace XFOidcClient2Demo
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class StringResources
    {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal StringResources() { }

        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null)) {
                    var name = "Resources";
                    if (Device.RuntimePlatform == Device.Android) {
                        name = "Droid.Resources";
                    } else if (Device.RuntimePlatform == Device.iOS) {
                        name = "iOS";
                    }
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager($"XFOidcClient2Demo.{name}.StringResources", typeof(StringResources).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture
        {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        internal static string MsgAlreadyAuthed
        {
            get { return ResourceManager.GetString("MsgAlreadyAuthed", resourceCulture); }
        }

        internal static string MsgApiNg
        {
            get { return ResourceManager.GetString("MsgApiNg", resourceCulture); }
        }

        internal static string MsgApiOk
        {
            get { return ResourceManager.GetString("MsgApiOk", resourceCulture); }
        }

        internal static string MsgAppStart
        {
            get { return ResourceManager.GetString("MsgAppStart", resourceCulture); }
        }

        internal static string MsgAuthNg
        {
            get { return ResourceManager.GetString("MsgAuthNg", resourceCulture); }
        }

        internal static string MsgAuthOk
        {
            get { return ResourceManager.GetString("MsgAuthOk", resourceCulture); }
        }

        internal static string MsgAuthRevokeNg
        {
            get { return ResourceManager.GetString("MsgAuthRevokeNg", resourceCulture); }
        }

        internal static string MsgAuthRevokeOk
        {
            get { return ResourceManager.GetString("MsgAuthRevokeOk", resourceCulture); }
        }

        internal static string MsgReauthzRequired
        {
            get { return ResourceManager.GetString("MsgReauthzRequired", resourceCulture); }
        }

        internal static string MsgShowAuthState
        {
            get { return ResourceManager.GetString("MsgShowAuthState", resourceCulture); }
        }

        internal static string UiButtonCallApi
        {
            get { return ResourceManager.GetString("UiButtonCallApi", resourceCulture); }
        }

        internal static string UiButtonClearLog
        {
            get { return ResourceManager.GetString("UiButtonClearLog", resourceCulture); }
        }

        internal static string UiButtonShowState
        {
            get { return ResourceManager.GetString("UiButtonShowState", resourceCulture); }
        }

        internal static string UiButtonSignin
        {
            get { return ResourceManager.GetString("UiButtonSignin", resourceCulture); }
        }

        internal static string UiButtonSignout
        {
            get { return ResourceManager.GetString("UiButtonSignout", resourceCulture); }
        }

        internal static string UiButtonTokenRefresh
        {
            get { return ResourceManager.GetString("UiButtonTokenRefresh", resourceCulture); }
        }

        internal static string UiMenuMainPage
        {
            get { return ResourceManager.GetString("UiMenuMainPage", resourceCulture); }
        }

        internal static string UiMenuLogPage
        {
            get { return ResourceManager.GetString("UiMenuLogPage", resourceCulture); }
        }

        internal static string UiTitleLogPage
        {
            get { return ResourceManager.GetString("UiTitleLogPage", resourceCulture); }
        }

        internal static string UiTitleMenuPage
        {
            get { return ResourceManager.GetString("UiTitleMenuPage", resourceCulture); }
        }
    }
}