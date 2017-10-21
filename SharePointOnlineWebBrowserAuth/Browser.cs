using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SharePointOnlineWebBrowserAuth
{
    public static class Browser
    {


        private static void UseWPFThread(Action codeToRun)
        {
            var thread = new Thread(() =>
            {
                codeToRun();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }



        public static Task<Microsoft.SharePoint.Client.ClientContext> GetAuthenticatedContext(string sharepointSiteUrl)
        {
            // see: https://stackoverflow.com/questions/15316613/when-should-taskcompletionsourcet-be-used

            var promise = new TaskCompletionSource<Microsoft.SharePoint.Client.ClientContext>();

            UseWPFThread(() =>
            {
                var win = new System.Windows.Window();

                var webBrowser = new WebBrowser();

                win.Content = webBrowser;

                webBrowser.Navigated += (_s, _args) =>
                {
                    var cookies = CookieReader.GetCookieCollection(_args.Uri)
                                .OfType<Cookie>();

                    var FedAuth = cookies.FirstOrDefault(c => string.Equals("FedAuth", c.Name, StringComparison.OrdinalIgnoreCase));
                    var rtfa = cookies.FirstOrDefault(c => string.Equals("rtFa", c.Name, StringComparison.OrdinalIgnoreCase));

                    if (FedAuth != null && rtfa != null)
                    {
                        // from: http://jcardy.co.uk/creating-a-sharepoint-csom-clientcontext-with-an-authentication-cookie/
                        var context = new Microsoft.SharePoint.Client.ClientContext(_args.Uri);
                        context.ExecutingWebRequest += (sender, e) =>
                        {
                            e.WebRequestExecutor.WebRequest.Headers[HttpRequestHeader.Cookie] = "FedAuth=" + FedAuth.Value + ";rtFa=" + rtfa.Value;
                        };

                        if (!promise.Task.IsCompleted)
                        {
                            promise.SetResult(context);
                        }

                        win.Close(); // we are done so close the window

                    }
                };

                win.Closed += (_s, _args) =>
                {
                    if (!promise.Task.IsCompleted)
                    {
                        throw new Exception("Window closed before got authentication");
                    }
                };


                // make this one of the last things that happens
                webBrowser.Navigate(sharepointSiteUrl);

                win.ShowDialog();
            });// end of wpf thread

            return promise.Task;
        }




    }
}
