using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.SharePoint.Client;

namespace TestSharePointBrowserAuthWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GetSharePointButton_Click(object sender, RoutedEventArgs e)
        {
            string targetSite = SiteUrlTextBox.Text;

            using (ClientContext ctx = SharePointOnlineWebBrowserAuth.ClaimClientContext.GetAuthenticatedContext(targetSite))
            {
                if (ctx != null)
                {
                    ctx.Load(ctx.Web); // Query for Web
                    ctx.ExecuteQuery(); // Execute

                    OutputTextBox.Text += Environment.NewLine + "Site Title: " + ctx.Web.Title;
                }
            }
        }
    }
}
