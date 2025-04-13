using ExecList.Helpers;
using ExecList.Models;
using System.Text.Json;
using System.Diagnostics;

namespace ExecList.Services
{
    public interface IExecListServices
    {
        public Profile GetInitialProfile();
        public Profile LoadFromJson(string jsonFilePath);
        public Task<bool> OpenFile(FileItem? fileItem);
    }

    public class ExecListServices : IExecListServices
    {
        public Profile GetInitialProfile()
        {
            List<string> jsonFilePaths = GetFilePathsFromDataFolder();

            string jsonFilePathToLoad = "";
            JsonElement root = GetJsonConfig();
            // Get the default profile from config
            if (root.TryGetProperty(AppConfig.DefaultProfile, out JsonElement defaultProfile))
            { jsonFilePathToLoad = defaultProfile.ToString(); }

            // Get the first profile from the collection
            if (string.IsNullOrEmpty(jsonFilePathToLoad))
            { jsonFilePathToLoad = jsonFilePaths.First(); }

            Profile profile = LoadFromJson(jsonFilePathToLoad);

            return profile;
        }

        private List<string> GetFilePathsFromDataFolder()
        {
            // Get the assigned default folder
            JsonElement root = GetJsonConfig();
            string defaultFolder = "";
            if (root.TryGetProperty(AppConfig.AssignedProfilesDirectory, out JsonElement assignedProfilesDirectory))
            {
                defaultFolder = assignedProfilesDirectory.ToString();
            }

            // Assign either the predefined path or default path if it's set 
            string dataFolder = string.IsNullOrEmpty(defaultFolder)
                ? Path.Combine(AppContext.BaseDirectory, NameDefinitions.Profiles.ToString())
                : defaultFolder;
            List<string> jsonFilePaths = Directory.GetFiles(dataFolder, "*.json").ToList();

            return jsonFilePaths;
        }

        public Profile LoadFromJson(string jsonFilePath)
        {
            string json = File.ReadAllText(jsonFilePath);
            return JsonSerializer.Deserialize<Profile>(json)!;
        }

        public async Task<bool> OpenFile(FileItem? fileItem)
        {
            if (fileItem == null || string.IsNullOrWhiteSpace(fileItem.FilePath))
            { return false; }

            if (!File.Exists(fileItem.FilePath))
            {
                await Shell.Current.DisplayAlert("Error", $"File not found!\n{fileItem?.FilePath}", "OK");
                return false;
            }

            string extension = Path.GetExtension(fileItem.FilePath).ToLower();

            //#if WINDOWS
            if (extension == ".exe")
            { return await OpenExecutableFile(fileItem.FilePath); }

            if (extension == ".bat")
            { return await OpenBatchFile(fileItem.FilePath); }
            //#endif

            // Fallback to launcher for safe file types
            return await OpenSafeFile(fileItem.FilePath);
        }

        private JsonElement GetJsonConfig()
        {
            string path = Path.Combine(
                AppContext.BaseDirectory,
                NameDefinitions.Config.ToString(),
                "applicationConfiguration.json");

            string json = File.ReadAllText(path);
            JsonDocument jsonDoc = JsonDocument.Parse(json);
            JsonElement root = jsonDoc.RootElement;

            return root;
        }


        /// <summary>
        /// In case file has moved
        /// </summary>
        public void FindFile()
        {
            // Have user input the directory where the files are
        }

        #region Methods for opening files
        public async Task<bool> OpenExecutableFile(string filePath)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true // Needed to launch associated shell
                };

                Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to open file:\n{ex.Message}", "OK");
                return false;
            }
        }

        public async Task<bool> OpenBatchFile(string filePath)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe", // Launch cmd.exe first before .bat file
                    Arguments = $"/c \"{filePath}\"", // /c = run then exit
                    UseShellExecute = false,          // false if you want output capture, true to let it pop on screen
                    CreateNoWindow = false,           // false = show the console window
                    WorkingDirectory = Path.GetDirectoryName(filePath) // Optional: helps with relative paths in .bat
                };

                Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to open .bat file:\n{ex.Message}", "OK");
            }

            return false;
        }        
        
        public async Task<bool> OpenSafeFile(string filePath)
        {
            try
            {
                await Launcher.Default.OpenAsync(filePath);
                return true;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to open file:\n{ex.Message}", "OK");
                return false;
            }
        }
        #endregion
    }
}
