using Newtonsoft.Json;

namespace FFBWheelProperties.Models
{
    public class DeviceProfile
    {
        public string Name { get; set; } = "Default";
        public int FFBGlobalStrength { get; set; } = 100;
        public int WheelRotationRange { get; set; } = 900;
        public int CenterDeadzone { get; set; } = 0;
        public int WheelCenterPosition { get; set; } = 0;
        public Dictionary<string, int> EffectStrengths { get; set; } = new();
        public Dictionary<int, string> ButtonMappings { get; set; } = new();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
        
        public DeviceProfile()
        {
            InitializeDefaults();
        }
        
        private void InitializeDefaults()
        {
            EffectStrengths = new Dictionary<string, int>
            {
                ["ConstantForce"] = 100,
                ["Spring"] = 100,
                ["Damper"] = 100,
                ["Friction"] = 100,
                ["Inertia"] = 100,
                ["Periodic"] = 100,
                ["Custom"] = 100
            };
            
            // Default button mappings
            for (int i = 0; i < 16; i++)
            {
                ButtonMappings[i] = $"Button {i + 1}";
            }
        }
        
        public void UpdateLastModified()
        {
            LastModified = DateTime.Now;
        }
    }
}