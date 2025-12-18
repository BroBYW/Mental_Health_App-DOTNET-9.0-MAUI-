namespace PROJECT.Pages
{
    // This attribute maps the query parameter "url" to the property "Url"
    [QueryProperty(nameof(Url), "url")]
    public partial class ArticlePage : ContentPage
    {
        public ArticlePage()
        {
            InitializeComponent();
        }

        // FIX: Initialize with string.Empty to satisfy the compiler
        private string _url = string.Empty;

        public string Url
        {
            get => _url;
            set
            {
                _url = value;
                // When the URL is set, load it into the WebView
                if (!string.IsNullOrEmpty(_url))
                {
                    NewsWebView.Source = _url;
                }
            }
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            // Go back to the Dashboard
            await Shell.Current.GoToAsync("..");
        }
    }
}