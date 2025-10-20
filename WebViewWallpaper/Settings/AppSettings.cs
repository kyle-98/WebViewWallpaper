using System.IO;
using System.Text.Json;

namespace WebViewWallpaper.Settings
{
     public class AppSettings
     {
          public string URL { get; set; } = "https://www.youtube.com/channel/UCmbs8T6MWqUHP1tIQvSgKrg";
     }

     public static class SettingsManager
     {
          private static readonly string path = Path.Combine(
               Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
               "WebViewWallpaper",
               "settings.json"
          );

          public static AppSettings Load()
          {
               if (!File.Exists(path))
               {
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    var defaultSettings = new AppSettings();
                    File.WriteAllText(path, JsonSerializer.Serialize(defaultSettings, new JsonSerializerOptions { WriteIndented = true }));
                    return defaultSettings;
               }

               return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path))!;
          }

          public static void Save(AppSettings settings)
          {
               Directory.CreateDirectory(Path.GetDirectoryName(path)!);
               File.WriteAllText(path, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
          }
     }
}
