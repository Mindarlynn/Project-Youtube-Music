using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectYM.Event;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ProjectYM
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // CONSTANTS
        public readonly string YoutubeMusicHome = "https://music.youtube.com";

        // EVENTS
        public EventHandler<TitleChangedArgs> TitleChanged;

        // MEMBERS
        private string _title;
        private BackgroundWorker _titleSearcher;
        private bool _isPageLoaded;

        // PROPERTIES
        public string Title
        {
            get => _title;
            set
            {
                _title = value == "" ? "Youtube Music" : value;
                TitleChanged?.Invoke(this, new TitleChangedArgs(_title));
            }
        }

        // CONSTRUCTOR
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            await WebView.EnsureCoreWebView2Async().AsTask();

            WebView.CoreWebView2.ContentLoading += (sender0, e0) => _isPageLoaded = false;
            WebView.CoreWebView2.DOMContentLoaded += (sender0, e0) => _isPageLoaded = true;

            WebView.Source = new Uri(YoutubeMusicHome);

            _titleSearcher = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };
            _titleSearcher.DoWork += SearchTitle;
            _titleSearcher.RunWorkerAsync();
        }

        // CALLBACKS
        private void SearchTitle(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (e.Cancel) break;
                if (!_isPageLoaded) continue;

                var html = "";

                var isFinished = false;
                WebView?.DispatcherQueue.TryEnqueue(async () =>
                {
                    html = await WebView.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML;");
                    isFinished = true;
                });
                while (!isFinished) ;

                html = Regex.Unescape(html);
                html = html.Remove(0, 1);
                html = html.Remove(html.Length - 1, 1);

                Title = GetTitleFromYoutubeMusicDOMContent(html);

                Thread.Sleep(100);
            }
        }  

        // FUNCTIONS
        private string GetTitleFromYoutubeMusicDOMContent(string html)
        {
            const string playerprop = "class=\"title style-scope ytmusic-player-bar\"";
            var playeridx = html.IndexOf(playerprop, StringComparison.Ordinal) + playerprop.Length;
            const string titleprop = "title=\"";
            var titleidx = html.IndexOf(titleprop, playeridx, StringComparison.Ordinal) + titleprop.Length;
            var title = html.Substring(titleidx, html.IndexOf('"', titleidx) - titleidx);

            return title;
        }

        public void Destroy()
        {
            _titleSearcher.CancelAsync();
        }
    }
}
