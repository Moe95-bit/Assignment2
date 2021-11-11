using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Assignment2
{


    public class Article
    {
        public string SiteName { get; set; }
        public DateTime Published { get; set; }
        public string Title { get; set; }
    }

    public class Feed
    {
        public string SiteName { get; set; }
        public string Url { get; set; }
    }

    public partial class MainWindow : Window
    {
        private List<Feed> feeds = new List<Feed>();
        private Thickness spacing = new Thickness(5);
        private HttpClient http = new HttpClient();
        // We will need these as instance variables to access in event handlers.
        private TextBox addFeedTextBox;
        private Button addFeedButton;
        private ComboBox selectFeedComboBox;
        private Button loadArticlesButton;
        private StackPanel articlePanel;

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        private void Start()
        {
            // Window options
            Title = "Feed Reader";
            Width = 800;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Scrolling
            var root = new ScrollViewer();
            root.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Content = root;

            // Main grid
            var grid = new Grid();
            root.Content = grid;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var addFeedLabel = new Label
            {
                Content = "Feed URL:",
                Margin = spacing
            };
            grid.Children.Add(addFeedLabel);

            addFeedTextBox = new TextBox
            {
                Margin = spacing,
                Padding = spacing
            };
            grid.Children.Add(addFeedTextBox);
            Grid.SetColumn(addFeedTextBox, 1);

            addFeedButton = new Button
            {
                Content = "Add Feed",
                Margin = spacing,
                Padding = spacing
            };

            addFeedButton.Click += btn_add_feed_click;
            grid.Children.Add(addFeedButton);
            Grid.SetColumn(addFeedButton, 2);

            var selectFeedLabel = new Label
            {
                Content = "Select Feed:",
                Margin = spacing
            };
            grid.Children.Add(selectFeedLabel);
            Grid.SetRow(selectFeedLabel, 1);

            selectFeedComboBox = new ComboBox
            {
                Margin = spacing,
                Padding = spacing,
                IsEditable = false
            };
            grid.Children.Add(selectFeedComboBox);
            Grid.SetRow(selectFeedComboBox, 1);
            Grid.SetColumn(selectFeedComboBox, 1);

            loadArticlesButton = new Button
            {
                Content = "Load Articles",
                Margin = spacing,
                Padding = spacing,
            };

            loadArticlesButton.Click += btn_load_click;
            grid.Children.Add(loadArticlesButton);
            Grid.SetRow(loadArticlesButton, 1);
            Grid.SetColumn(loadArticlesButton, 2);

            articlePanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = spacing
            };
            grid.Children.Add(articlePanel);
            Grid.SetRow(articlePanel, 2);
            Grid.SetColumnSpan(articlePanel, 3);

        }

        private async void btn_add_feed_click(object sender, RoutedEventArgs e)
        { 
           await LoadFeed();
        }

        private async Task LoadFeed()
        {
           
            try
            {
                addFeedButton.IsEnabled = false;
                var feedDocument = await LoadFeedAsync(addFeedTextBox.Text);
                
                string PublisherWebsite = feedDocument.Descendants().Where(s => s.Name == "title").FirstOrDefault().Value;


                if (selectFeedComboBox.Items.Count == 0)
                {
                    selectFeedComboBox.Items.Add("All Feeds");
                }
                selectFeedComboBox.Items.Add(PublisherWebsite);
                selectFeedComboBox.SelectedIndex = selectFeedComboBox.Items.Count - 1;

                Feed feed = new Feed
                {
                    SiteName = PublisherWebsite,
                    Url = addFeedTextBox.Text
                };

                feeds.Add(feed);
            }
            catch (Exception)
            {
                addFeedButton.IsEnabled = true;
                MessageBox.Show("Something went wrong reading URL");
            }
            addFeedButton.IsEnabled = true;

        }

        private async void btn_load_click(object sender, RoutedEventArgs e)
        {
          await DisplayArticles();
        }

        private async Task DisplayArticles()
        {
            try
            {
                loadArticlesButton.IsEnabled = false;

                var articlePlaceholder = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = spacing
                };

                articlePanel.Children.Clear();
                articlePanel.Children.Add(articlePlaceholder);

                if (selectFeedComboBox.Text == "All Feeds")
                {

                    var ArticlesFromFeeds = feeds.Select(LoadArticlesAsync).ToList();
                    var ListOfArticlesLists = await Task.WhenAll(ArticlesFromFeeds);
                    var AllArticles = ListOfArticlesLists.SelectMany(a => a).ToList();

                    foreach (var article in AllArticles.OrderByDescending(a => a.Published))
                    {
                        var articleTitleAndTime = new TextBlock
                        {
                            Text = article.Published + " - " + article.Title,
                            FontWeight = FontWeights.Bold,
                            TextTrimming = TextTrimming.CharacterEllipsis
                        };
                        var articleSiteName = new TextBlock
                        {
                            Text = article.SiteName,
                        };
                        articlePlaceholder.Children.Add(articleTitleAndTime);
                        articlePlaceholder.Children.Add(articleSiteName);
                    }
                }
                else
                {
                    Feed feed = feeds.Single(f => f.SiteName == selectFeedComboBox.Text);

                    var articleList = await LoadArticlesAsync(feed);
                    foreach (var article in articleList)
                    {
                        var articleTitleAndTime = new TextBlock
                        {
                            Text = article.Published + " - " + article.Title,
                            FontWeight = FontWeights.Bold,
                            TextTrimming = TextTrimming.CharacterEllipsis
                        };
                        var articleSiteName = new TextBlock
                        {
                            Text = article.SiteName,
                        };
                        articlePlaceholder.Children.Add(articleTitleAndTime);
                        articlePlaceholder.Children.Add(articleSiteName);
                    }
                }
            }
            catch (Exception)
            {
                loadArticlesButton.IsEnabled = true;
                MessageBox.Show("Could not read articles");
            }
           

            loadArticlesButton.IsEnabled = true;

        }

        private async Task<XDocument> LoadFeedAsync(string url)
        {

            await Task.Delay(1000);
            var response = await http.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            var feedFromUrl = XDocument.Load(stream);


            return feedFromUrl;
        }

        private async Task<List<Article>> LoadArticlesAsync(Feed feed)
        {

            await Task.Delay(1000);
            var response = await http.GetAsync(feed.Url);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            var feedFromUrl = XDocument.Load(stream);
            string PublisherWebsite = feedFromUrl.Descendants().Where(s => s.Name == "title").FirstOrDefault().Value;

            List<Article> articles = new List<Article>();

            var list = (from x in feedFromUrl.Descendants("item")
                        select new
                        {
                            title = x.Element("title").Value,
                            link = x.Element("link").Value,
                            published = x.Element("pubDate").Value

                        });

            for (int i = 0; i < 5; i++)
            {
                Article article = new Article
                {
                    Title = list.ElementAt(i).title,
                    Published = DateTime.ParseExact(list.ElementAt(i).published.Substring(0, 25), "ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                    SiteName = PublisherWebsite
                };
                articles.Add(article);
            }

            return articles;
        }

    
    }
}
