# FFB Racing Wheel Properties

A Windows configuration application for OpenFFB Racing Wheels using C# and .NET 6.

## Features

- **Real-time device communication** via HID
- **Force Feedback configuration** with individual effect controls
- **Profile management** - save and load custom configurations
- **Wheel calibration** - set rotation range and center position
- **Button mapping** - visualize and configure button states
- **Automatic device detection** and reconnection
- **Comprehensive logging** for troubleshooting

## Requirements

- Windows 10/11
- .NET 6.0 Runtime
- OpenFFB compatible racing wheel with VID: 0x1209, PID: 0x4711

## Installation

### From Source
1. Clone this repository
2. Open `FFBWheelProperties.sln` in Visual Studio 2022
3. Build and run (F5)

### From Release
1. Download the latest release from the releases page
2. Run the installer
3. Launch "FFB Racing Wheel Properties" from the Start Menu

## Usage

1. **Connect your wheel** - The application will automatically detect and connect
2. **Select a profile** from the dropdown or create a new one
3. **Configure settings** in the Force Feedback, Calibration, and Button Mapping tabs
4. **Save your profile** to preserve your settings
5. **Test effects** using the built-in test buttons

## Building

### Prerequisites
- Visual Studio 2022 with .NET desktop development workload
- Or .NET 6.0 SDK + your preferred editor

### Build Steps
```bash
git clone https://github.com/yourusername/FFBWheelProperties.git
cd FFBWheelProperties
dotnet restore
dotnet build
dotnet run
```

### Creating Installer
1. Install Visual Studio Installer Projects extension
2. Add Setup Project to solution
3. Build installer project

## Configuration Files

Profiles are stored in: `%AppData%\FFBWheelProperties\profiles.json`
Logs are stored in: `%AppData%\FFBWheelProperties\app.log`

## Device Protocol

The application communicates with devices using the following HID reports:

- **Report ID 1**: Wheel position + button states (Input)
- **Report ID 2**: FFB status (Input)  
- **Report ID 3-9**: Various FFB effects (Output)

## Troubleshooting

1. **Device not detected**: 
   - Check device is properly connected
   - Verify VID/PID match (0x1209/0x4711)
   - Check Windows Device Manager

2. **Permission issues**:
   - Run as Administrator
   - Check Windows driver installation

3. **Effects not working**:
   - Verify device firmware supports FFB
   - Check effect strength settings
   - Review application logs

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see LICENSE file for details.

## Support

- Create an issue for bug reports
- Check the logs in About tab for diagnostics
- Review the HID communication protocol documentation
