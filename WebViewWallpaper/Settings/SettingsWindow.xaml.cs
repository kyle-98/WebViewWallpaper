using System.Windows;

namespace WebViewWallpaper.Settings
{
     public partial class SettingsWindow : Window
     {
          private readonly AppSettings _settings;

          public event Action<string>? OnUrlSaved;

          public SettingsWindow(AppSettings settings)
          {
               InitializeComponent();
               _settings = settings;
               UrlTextBox.Text = _settings.URL;
          }

          private void Save_Click(object sender, RoutedEventArgs e)
          {
               _settings.URL = UrlTextBox.Text;
               SettingsManager.Save(_settings);
               OnUrlSaved?.Invoke(_settings.URL);
               Close();
          }

          private void Cancel_Click(object sender, RoutedEventArgs e)
          {
               Close();
          }
     }
}
