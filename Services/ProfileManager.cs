using Newtonsoft.Json;
using FFBWheelProperties.Models;
using FFBWheelProperties.Utils;

namespace FFBWheelProperties.Services
{
    public class ProfileManager
    {
        private readonly string _profilesPath;
        private Dictionary<string, DeviceProfile> _profiles;
        
        public ProfileManager()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FFBWheelProperties"
            );
            
            _profilesPath = Path.Combine(appDataPath, "profiles.json");
            
            try
            {
                Directory.CreateDirectory(appDataPath);
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to create application data directory: {ex.Message}");
            }
            
            _profiles = new Dictionary<string, DeviceProfile>();
            LoadProfiles();
        }
        
        public IEnumerable<string> GetProfileNames() => _profiles.Keys.OrderBy(x => x);
        
        public DeviceProfile GetProfile(string name)
        {
            if (_profiles.ContainsKey(name))
            {
                return _profiles[name];
            }
            
            // Return default profile if requested profile doesn't exist
            return _profiles.ContainsKey("Default") ? _profiles["Default"] : new DeviceProfile();
        }
        
        public void SaveProfile(string name, DeviceProfile profile)
        {
            try
            {
                profile.Name = name;
                profile.UpdateLastModified();
                _profiles[name] = profile;
                SaveProfiles();
                Logger.Log($"Profile '{name}' saved successfully");
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to save profile '{name}': {ex.Message}");
                throw;
            }
        }
        
        public bool DeleteProfile(string name)
        {
            if (name == "Default")
            {
                Logger.Log("Cannot delete default profile");
                return false; // Cannot delete default profile
            }
            
            if (_profiles.ContainsKey(name))
            {
                try
                {
                    _profiles.Remove(name);
                    SaveProfiles();
                    Logger.Log($"Profile '{name}' deleted successfully");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to delete profile '{name}': {ex.Message}");
                    return false;
                }
            }
            
            return false;
        }
        
        public DeviceProfile CreateNewProfile(string name)
        {
            var newProfile = new DeviceProfile { Name = name };
            _profiles[name] = newProfile;
            return newProfile;
        }
        
        private void LoadProfiles()
        {
            try
            {
                if (File.Exists(_profilesPath))
                {
                    var json = File.ReadAllText(_profilesPath);
                    var loadedProfiles = JsonConvert.DeserializeObject<Dictionary<string, DeviceProfile>>(json);
                    
                    if (loadedProfiles != null)
                    {
                        _profiles = loadedProfiles;
                        Logger.Log($"Loaded {_profiles.Count} profiles from file");
                    }
                }
                
                // Ensure default profile exists
                if (!_profiles.ContainsKey("Default"))
                {
                    _profiles["Default"] = new DeviceProfile { Name = "Default" };
                    Logger.Log("Created default profile");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error loading profiles: {ex.Message}");
                _profiles = new Dictionary<string, DeviceProfile>
                {
                    ["Default"] = new DeviceProfile { Name = "Default" }
                };
            }
        }
        
        private void SaveProfiles()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_profiles, Formatting.Indented);
                File.WriteAllText(_profilesPath, json);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error saving profiles: {ex.Message}");
                throw;
            }
        }
    }
}
