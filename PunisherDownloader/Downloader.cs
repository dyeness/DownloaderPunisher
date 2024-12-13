using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LibGit2Sharp;

class Program
{
    // Класс для представления настроек из файла Settings.json
    public class Settings
    {
        public string RepositoryUrl { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
    }

    // Класс для представления релиза из GitHub API
    public class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("zipball_url")]
        public string ZipballUrl { get; set; } = string.Empty;
    }

    static async Task Main(string[] args)
    {
        // Определяем путь к файлу Settings.json в папке с исполняемым файлом
        string settingsFilePath = Path.Combine(AppContext.BaseDirectory, "Settings.json");

        try
        {
            // Читаем настройки из файла Settings.json
            if (!File.Exists(settingsFilePath))
            {
                Console.WriteLine($"Settings file '{settingsFilePath}' not found.");
                return;
            }

            var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsFilePath));
            if (settings == null || string.IsNullOrWhiteSpace(settings.RepositoryUrl) || string.IsNullOrWhiteSpace(settings.TargetPath))
            {
                Console.WriteLine("Invalid settings in 'Settings.json'.");
                return;
            }

            // Извлечение названия репозитория из URL
            string repoName = ExtractRepositoryName(settings.RepositoryUrl);
            Console.WriteLine($"Repository: {repoName}\n");

            Console.WriteLine("Fetching available releases...");
            var releases = await GetGitHubReleasesAsync(settings.RepositoryUrl);

            if (releases == null || releases.Length == 0)
            {
                Console.WriteLine("No releases found.");
                return;
            }

            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Download full repository");
            for (int i = 0; i < releases.Length; i++)
            {
                Console.WriteLine($"{i + 2}. Download release: {releases[i].Name} ({releases[i].TagName})");
            }
            Console.Write("Enter your choice: ");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                DownloadFullRepository(settings);
            }
            else if (int.TryParse(choice, out int releaseIndex) && releaseIndex >= 2 && releaseIndex <= releases.Length + 1)
            {
                var selectedRelease = releases[releaseIndex - 2];
                await DownloadReleaseVersionAsync(settings, selectedRelease.ZipballUrl, selectedRelease.TagName);
            }
            else
            {
                Console.WriteLine("Invalid choice. Exiting program.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }

        // Проверка доступности консоли перед вызовом ReadKey
        if (Console.IsOutputRedirected || Console.IsInputRedirected)
        {
            Console.WriteLine("Output or input is redirected. Exiting in 10 seconds...");
            await Task.Delay(10000);
        }
        else
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    // Метод для извлечения названия репозитория из URL
    static string ExtractRepositoryName(string repositoryUrl)
    {
        try
        {
            var uri = new Uri(repositoryUrl);
            string[] segments = uri.AbsolutePath.Split('/');
            return segments.Length > 2 ? segments[2] : "Unknown Repository";
        }
        catch
        {
            return "Unknown Repository";
        }
    }

    // Метод для скачивания полного репозитория
    static void DownloadFullRepository(Settings settings)
    {
        string clonePath = Path.Combine(AppContext.BaseDirectory, "TempRepo");

        // Удаляем старую директорию, если она существует
        if (Directory.Exists(clonePath))
        {
            Console.WriteLine("Cleaning up previous clone...");
            try
            {
                Directory.Delete(clonePath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete temporary directory: {ex.Message}");
                return;
            }
        }

        // Клонируем репозиторий
        Console.WriteLine("Cloning repository...");
        try
        {
            Repository.Clone(settings.RepositoryUrl, clonePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to clone repository: {ex.Message}");
            return;
        }

        // Путь к папке user в репозитории
        string userFolderPath = Path.Combine(clonePath, "user");

        if (!Directory.Exists(userFolderPath))
        {
            Console.WriteLine("Folder 'user' not found in the repository.");
            return;
        }

        // Копируем папку user в целевую директорию
        Console.WriteLine("Copying 'user' folder...");
        try
        {
            CopyDirectory(userFolderPath, settings.TargetPath);
            Console.WriteLine("Repository downloaded and 'user' folder copied successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to copy 'user' folder: {ex.Message}");
        }
    }

    // Метод для скачивания конкретного релиза
    static async Task DownloadReleaseVersionAsync(Settings settings, string zipUrl, string version)
    {
        string tempFile = Path.Combine(AppContext.BaseDirectory, "release.zip");
        string targetPath = settings.TargetPath;

        Console.WriteLine($"Downloading release version: {version}");

        try
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(zipUrl);
                response.EnsureSuccessStatusCode();

                await using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }

            Console.WriteLine("Extracting release...");
            if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath, true);
            }

            System.IO.Compression.ZipFile.ExtractToDirectory(tempFile, targetPath);

            Console.WriteLine("Release version downloaded and extracted successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to download or extract release: {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    // Метод для получения списка релизов с GitHub
    static async Task<GitHubRelease[]> GetGitHubReleasesAsync(string repositoryUrl)
    {
        string apiUrl = repositoryUrl.Replace("https://github.com/", "https://api.github.com/repos/") + "/releases";

        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");
                var response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GitHubRelease[]>(json);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to fetch releases: {ex.Message}");
            return null;
        }
    }

    // Метод для копирования содержимого папки
    static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, destFile, true); // Перезапись файлов
        }

        foreach (string dir in Directory.GetDirectories(sourceDir))
        {
            string destDir = Path.Combine(targetDir, Path.GetFileName(dir));
            CopyDirectory(dir, destDir);
        }
    }
}
