/*
 *  Xamarin.Forms + IdendityModel.OidcClient2 demonstration application.
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XFOidcClient2Demo
{
    public partial class LogPage : ContentPage
    {
        private static List<LogLine> logForDemo = new List<LogLine>();
        private static object logForDemoLock = new object();

        public LogPage()
        {
            InitializeComponent();
            LogListView.ItemsSource = logForDemo;

            var item = new ToolbarItem { Text = StringResources.UiButtonClearLog };
            item.Clicked += OnToolbarItemClicked;
            ToolbarItems.Add(item);
        }

        protected void OnToolbarItemClicked(object sender, EventArgs e)
        {
            ClearLogForDemo();
            LogListView.ItemsSource = null;
            LogListView.ItemsSource = logForDemo;
        }

        internal static void AuthzRequestLogger(string request)
        {
            AppendLogForDemo("AuthzReq", request);
        }

        internal static void AuthzResponseLogger(string response, string error)
        {
            if (response != null) {
                AppendLogForDemo("AuthzResp", response);
            } else {
                AppendLogForDemo("AuthzError", error);
            }
        }

        internal static async Task HttpRequestLoggerAsync(HttpRequestMessage request)
        {
            var message = $"{request.Method} {request.RequestUri}";
            if (request.Headers.Authorization != null) {
                message += $"\nAuthorization: {request.Headers.Authorization}";
            }
            if (request.Method == HttpMethod.Post) {
                message += $"\n\n{await request.Content.ReadAsStringAsync()}";
            }
            AppendLogForDemo("HttpReq", message);
        }

        internal static async Task HttpResponseLoggerAsync(HttpResponseMessage response)
        {
            var message = $"HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}";
            var body = await response.Content.ReadAsStringAsync();
            if (body.Length > 0) {
                message += $"\n\n{body}";
            }
            AppendLogForDemo("HttpResp", message);
        }

        internal static void AppendLogForDemo(string type, string message)
        {
            lock (logForDemoLock) {
                logForDemo.Add(new LogLine { Type = type, Message = message });
            }
        }

        internal static void ClearLogForDemo()
        {
            lock (logForDemoLock) {
                logForDemo.Clear();
            }
        }

        public class LogLine
        {
            public DateTime Timestamp { get; private set; }
            public string Type { get; set; }
            public string Message { get; set; }

            public LogLine()
            {
                Timestamp = DateTime.Now;
            }
        }
    }
}