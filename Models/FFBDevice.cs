using HidLibrary;

namespace FFBWheelProperties.Models
{
    public class FFBDevice
    {
        public const int VendorId = 0x1209;
        public const int ProductId = 0x4711;
        public const string ProductName = "OpenFFB Wheel";
        
        public HidDevice? Device { get; set; }
        public bool IsConnected { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string FirmwareVersion { get; set; } = string.Empty;
        
        // Current device state
        public int WheelPosition { get; set; }
        public bool[] ButtonStates { get; set; } = new bool[16];
        public byte FFBStatus { get; set; }
    }
}