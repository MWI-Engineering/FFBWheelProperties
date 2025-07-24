using FFBWheelProperties.Services;
using FFBWheelProperties.Models;
using FFBWheelProperties.Utils;

namespace FFBWheelProperties
{
    public partial class MainForm : Form
    {
        private HIDCommunicationService? _hidService;
        private ProfileManager? _profileManager;
        private DeviceProfile _currentProfile;
        private System.Windows.Forms.Timer? _connectionTimer;
        private bool _isClosing = false;
        
        // UI Controls
        private TabControl? tabControl;
        private StatusStrip? statusStrip;
        private ToolStripStatusLabel? statusLabel;
        private MenuStrip? menuStrip;
        private ComboBox? profileComboBox;
        private Button? saveProfileButton;
        private Button? newProfileButton;
        private Button? deleteProfileButton;
        
        // Tabs
        private TabPage? tabFFB;
        private TabPage? tabCalibration;
        private TabPage? tabButtons;
        private TabPage? tabAbout;
        
        public MainForm()
        {
            Logger.Log("Application starting");
            _currentProfile = new DeviceProfile();
            
            InitializeComponent();
            InitializeServices();
            SetupUI();
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = "FFB Racing Wheel Properties";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(600, 400);
            this.Icon = SystemIcons.Application;
            
            // Create menu strip
            menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");
            var helpMenu = new ToolStripMenuItem("Help");
            
            var exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += (s, e) => this.Close();
            fileMenu.DropDownItems.Add(exitMenuItem);
            
            var aboutMenuItem = new ToolStripMenuItem("About");
            aboutMenuItem.Click += ShowAboutDialog;
            helpMenu.DropDownItems.Add(aboutMenuItem);
            
            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, helpMenu });
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;
            
            // Create profile management panel
            var profilePanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Top,
                Padding = new Padding(10, 10, 10, 5)
            };
            
            var profileLabel = new Label
            {
                Text = "Profile:",
                AutoSize = true,
                Location = new Point(10, 15)
            };
            
            profileComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(60, 12),
                Width = 150
            };
            profileComboBox.SelectedIndexChanged += ProfileComboBox_SelectedIndexChanged;
            
            saveProfileButton = new Button
            {
                Text = "Save",
                Location = new Point(220, 11),
                Width = 60,
                Height = 25
            };
            saveProfileButton.Click += SaveProfileButton_Click;
            
            newProfileButton = new Button
            {
                Text = "New",
                Location = new Point(290, 11),
                Width = 60,
                Height = 25
            };
            newProfileButton.Click += NewProfileButton_Click;
            
            deleteProfileButton = new Button
            {
                Text = "Delete",
                Location = new Point(360, 11),
                Width = 60,
                Height = 25
            };
            deleteProfileButton.Click += DeleteProfileButton_Click;
            
            profilePanel.Controls.AddRange(new Control[] 
            { 
                profileLabel, profileComboBox, saveProfileButton, 
                newProfileButton, deleteProfileButton 
            });
            this.Controls.Add(profilePanel);
            
            // Create tab control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };
            
            // Create tabs
            tabFFB = new TabPage("Force Feedback");
            tabCalibration = new TabPage("Calibration");
            tabButtons = new TabPage("Button Mapping");
            tabAbout = new TabPage("About");
            
            // Add simple content to tabs for now
            AddFFBControls(tabFFB);
            AddCalibrationControls(tabCalibration);
            AddButtonControls(tabButtons);
            AddAboutControls(tabAbout);
            
            tabControl.TabPages.AddRange(new TabPage[] 
            { 
                tabFFB, tabCalibration, tabButtons, tabAbout 
            });
            
            this.Controls.Add(tabControl);
            
            // Create status strip
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Initializing...");
            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        
        private void AddFFBControls(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            var globalGroup = new GroupBox
            {
                Text = "Global FFB Settings",
                Location = new Point(10, 10),
                Size = new Size(300, 100)
            };
            
            var strengthLabel = new Label
            {
                Text = "Global Strength:",
                Location = new Point(10, 25),
                AutoSize = true
            };
            
            var strengthTrackBar = new TrackBar
            {
                Location = new Point(10, 45),
                Size = new Size(200, 45),
                Minimum = 0,
                Maximum = 100,
                Value = 100,
                TickFrequency = 10
            };
            
            var strengthValueLabel = new Label
            {
                Text = "100%",
                Location = new Point(220, 50),
                AutoSize = true
            };
            
            strengthTrackBar.ValueChanged += (s, e) =>
            {
                strengthValueLabel.Text = $"{strengthTrackBar.Value}%";
                _currentProfile.FFBGlobalStrength = strengthTrackBar.Value;
            };
            
            var testButton = new Button
            {
                Text = "Test Constant Force",
                Location = new Point(10, 150),
                Size = new Size(150, 30)
            };
            
            testButton.Click += (s, e) =>
            {
                if (_hidService?.IsConnected == true)
                {
                    var effect = FFBEffect.CreateConstantForce(127, 127);
                    _hidService.SendFFBEffect(effect);
                    
                    // Stop effect after 1 second
                    var timer = new System.Windows.Forms.Timer { Interval = 1000 };
                    timer.Tick += (ts, te) =>
                    {
                        var stopEffect = FFBEffect.CreateConstantForce(0, 127);
                        _hidService.SendFFBEffect(stopEffect);
                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();
                }
                else
                {
                    MessageBox.Show("Device not connected!", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            
            globalGroup.Controls.AddRange(new Control[] 
            { 
                strengthLabel, strengthTrackBar, strengthValueLabel 
            });
            
            panel.Controls.AddRange(new Control[] { globalGroup, testButton });
            tab.Controls.Add(panel);
        }
        
        private void AddCalibrationControls(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            var positionLabel = new Label
            {
                Text = "Wheel Position: 0",
                Location = new Point(10, 10),
                Size = new Size(200, 20),
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold)
            };
            
            var rotationGroup = new GroupBox
            {
                Text = "Rotation Range",
                Location = new Point(10, 50),
                Size = new Size(300, 80)
            };
            
            var rotationCombo = new ComboBox
            {
                Location = new Point(10, 25),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            rotationCombo.Items.AddRange(new object[] { 180, 270, 360, 540, 720, 900 });
            rotationCombo.SelectedItem = 900;
            
            var calibrateButton = new Button
            {
                Text = "Calibrate Center",
                Location = new Point(10, 150),
                Size = new Size(120, 30)
            };
            
            calibrateButton.Click += (s, e) =>
            {
                MessageBox.Show("Turn wheel to center position and click OK to calibrate.", 
                    "Calibration", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _currentProfile.WheelCenterPosition = 0; // Would use actual wheel position
            };
            
            rotationGroup.Controls.Add(rotationCombo);
            panel.Controls.AddRange(new Control[] { positionLabel, rotationGroup, calibrateButton });
            tab.Controls.Add(panel);
        }
        
        private void AddButtonControls(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            var buttonsGroup = new GroupBox
            {
                Text = "Button States",
                Location = new Point(10, 10),
                Size = new Size(400, 200)
            };
            
            // Create 16 button indicators
            for (int i = 0; i < 16; i++)
            {
                var buttonPanel = new Panel
                {
                    Location = new Point(10 + (i % 4) * 90, 25 + (i / 4) * 40),
                    Size = new Size(80, 30),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.LightGray
                };
                
                var buttonLabel = new Label
                {
                    Text = $"Btn {i + 1}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Tag = i // Store button index
                };
                
                buttonPanel.Controls.Add(buttonLabel);
                buttonsGroup.Controls.Add(buttonPanel);
            }
            
            panel.Controls.Add(buttonsGroup);
            tab.Controls.Add(panel);
        }
        
        private void AddAboutControls(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            var titleLabel = new Label
            {
                Text = "FFB Racing Wheel Properties",
                Font = new Font(this.Font.FontFamily, 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            
            var versionLabel = new Label
            {
                Text = "Version 1.0.0",
                Location = new Point(20, 60),
                AutoSize = true
            };
            
            var descriptionLabel = new Label
            {
                Text = "Configuration tool for OpenFFB Racing Wheels\n\n" +
                       "This application allows you to configure force feedback settings,\n" +
                       "calibrate your wheel, and manage button mappings for your\n" +
                       "OpenFFB compatible racing wheel.",
                Location = new Point(20, 90),
                Size = new Size(500, 100)
            };
            
            var logButton = new Button
            {
                Text = "View Log File",
                Location = new Point(20, 220),
                Size = new Size(100, 30)
            };
            
            logButton.Click += (s, e) =>
            {
                var logPath = Logger.GetLogPath();
                if (File.Exists(logPath))
                {
                    System.Diagnostics.Process.Start("notepad.exe", logPath);
                }
                else
                {
                    MessageBox.Show("Log file not found.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            
            panel.Controls.AddRange(new Control[] 
            { 
                titleLabel, versionLabel, descriptionLabel, logButton 
            });
            tab.Controls.Add(panel);
        }
        
        private void InitializeServices()
        {
            try
            {
                _profileManager = new ProfileManager();
                _currentProfile = _profileManager.GetProfile("Default");
                
                _hidService = new HIDCommunicationService();
                _hidService.DeviceDataReceived += OnDeviceDataReceived;
                _hidService.DeviceDisconnected += OnDeviceDisconnected;
                _hidService.DeviceConnected += OnDeviceConnected;
                
                // Try to connect immediately
                TryConnect();
                
                // Setup reconnection timer
                _connectionTimer = new System.Windows.Forms.Timer();
                _connectionTimer.Interval = 2000; // Check every 2 seconds
                _connectionTimer.Tick += (s, e) => TryConnect();
                _connectionTimer.Start();
                
                Logger.Log("Services initialized successfully");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "InitializeServices");
                MessageBox.Show($"Failed to initialize services: {ex.Message}", 
                    "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void SetupUI()
        {
            try
            {
                UpdateProfileList();
                UpdateConnectionStatus(false);
                LoadProfileToUI();
                
                Logger.Log("UI setup completed");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "SetupUI");
            }
        }
        
        private void TryConnect()
        {
            if (_isClosing || _hidService?.IsConnected == true) return;
            
            if (_hidService?.Connect() == true)
            {
                UpdateConnectionStatus(true);
                _connectionTimer?.Stop();
            }
        }
        
        private void UpdateConnectionStatus(bool connected)
        {
            if (_isClosing) return;
            
            try
            {
                if (statusLabel != null)
                {
                    statusLabel.Text = connected ? "Connected to OpenFFB Wheel" : "Disconnected - Searching...";
                    statusLabel.ForeColor = connected ? Color.Green : Color.Red;
                }
                
                // Enable/disable controls based on connection
                if (tabControl != null)
                {
                    tabControl.Enabled = connected;
                }
                
                Logger.Log($"Connection status updated: {(connected ? "Connected" : "Disconnected")}");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "UpdateConnectionStatus");
            }
        }
        
        private void OnDeviceDataReceived(object? sender, FFBDevice deviceData)
        {
            if (_isClosing) return;
            
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<object?, FFBDevice>(OnDeviceDataReceived), sender, deviceData);
                    return;
                }
                
                // Update wheel position in calibration tab
                if (tabCalibration?.Controls.Count > 0)
                {
                    var panel = tabCalibration.Controls[0];
                    var positionLabel = panel.Controls.OfType<Label>().FirstOrDefault();
                    if (positionLabel != null)
                    {
                        positionLabel.Text = $"Wheel Position: {deviceData.WheelPosition}";
                    }
                }
                
                // Update button states in button tab
                if (tabButtons?.Controls.Count > 0)
                {
                    var panel = tabButtons.Controls[0];
                    var buttonsGroup = panel.Controls.OfType<GroupBox>().FirstOrDefault();
                    if (buttonsGroup != null)
                    {
                        foreach (Panel buttonPanel in buttonsGroup.Controls.OfType<Panel>())
                        {
                            var label = buttonPanel.Controls.OfType<Label>().FirstOrDefault();
                            if (label?.Tag is int buttonIndex && buttonIndex < deviceData.ButtonStates.Length)
                            {
                                buttonPanel.BackColor = deviceData.ButtonStates[buttonIndex] ? 
                                    Color.LightGreen : Color.LightGray;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "OnDeviceDataReceived");
            }
        }
        
        private void OnDeviceDisconnected(object? sender, EventArgs e)
        {
            if (_isClosing) return;
            
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<object?, EventArgs>(OnDeviceDisconnected), sender, e);
                    return;
                }
                
                UpdateConnectionStatus(false);
                _connectionTimer?.Start(); // Resume trying to reconnect
                
                Logger.Log("Device disconnected event handled");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "OnDeviceDisconnected");
            }
        }
        
        private void OnDeviceConnected(object? sender, EventArgs e)
        {
            if (_isClosing) return;
            
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<object?, EventArgs>(OnDeviceConnected), sender, e);
                    return;
                }
                
                UpdateConnectionStatus(true);
                Logger.Log("Device connected event handled");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "OnDeviceConnected");
            }
        }
        
        private void UpdateProfileList()
        {
            try
            {
                if (profileComboBox != null && _profileManager != null)
                {
                    profileComboBox.Items.Clear();
                    foreach (var profileName in _profileManager.GetProfileNames())
                    {
                        profileComboBox.Items.Add(profileName);
                    }
                    profileComboBox.SelectedItem = "Default";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "UpdateProfileList");
            }
        }
        
        private void ProfileComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                if (profileComboBox?.SelectedItem != null && _profileManager != null)
                {
                    var profileName = profileComboBox.SelectedItem.ToString();
                    if (!string.IsNullOrEmpty(profileName))
                    {
                        _currentProfile = _profileManager.GetProfile(profileName);
                        LoadProfileToUI();
                        Logger.Log($"Switched to profile: {profileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "ProfileComboBox_SelectedIndexChanged");
            }
        }
        
        private void LoadProfileToUI()
        {
            try
            {
                // Load FFB settings
                if (tabFFB?.Controls.Count > 0)
                {
                    var panel = tabFFB.Controls[0];
                    var globalGroup = panel.Controls.OfType<GroupBox>().FirstOrDefault();
                    if (globalGroup != null)
                    {
                        var trackBar = globalGroup.Controls.OfType<TrackBar>().FirstOrDefault();
                        var valueLabel = globalGroup.Controls.OfType<Label>().LastOrDefault();
                        
                        if (trackBar != null)
                        {
                            trackBar.Value = _currentProfile.FFBGlobalStrength;
                        }
                        if (valueLabel != null)
                        {
                            valueLabel.Text = $"{_currentProfile.FFBGlobalStrength}%";
                        }
                    }
                }
                
                Logger.Log($"Loaded profile '{_currentProfile.Name}' to UI");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "LoadProfileToUI");
            }
        }
        
        private void SaveProfileButton_Click(object? sender, EventArgs e)
        {
            try
            {
                SaveCurrentProfile();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "SaveProfileButton_Click");
                MessageBox.Show($"Failed to save profile: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void SaveCurrentProfile()
        {
            try
            {
                // Save current UI values to profile
                if (tabFFB?.Controls.Count > 0)
                {
                    var panel = tabFFB.Controls[0];
                    var globalGroup = panel.Controls.OfType<GroupBox>().FirstOrDefault();
                    if (globalGroup != null)
                    {
                        var trackBar = globalGroup.Controls.OfType<TrackBar>().FirstOrDefault();
                        if (trackBar != null)
                        {
                            _currentProfile.FFBGlobalStrength = trackBar.Value;
                        }
                    }
                }
                
                _profileManager?.SaveProfile(_currentProfile.Name, _currentProfile);
                MessageBox.Show($"Profile '{_currentProfile.Name}' saved successfully!", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                Logger.Log($"Profile '{_currentProfile.Name}' saved by user");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "SaveCurrentProfile");
                throw;
            }
        }
        
        private void NewProfileButton_Click(object? sender, EventArgs e)
        {
            try
            {
                using (var dialog = new TextInputDialog("Enter profile name:", "New Profile"))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        var profileName = dialog.InputText.Trim();
                        if (string.IsNullOrEmpty(profileName))
                        {
                            MessageBox.Show("Profile name cannot be empty.", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        
                        if (_profileManager?.GetProfileNames().Contains(profileName) == true)
                        {
                            MessageBox.Show("Profile with this name already exists.", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        
                        var newProfile = _profileManager?.CreateNewProfile(profileName);
                        if (newProfile != null)
                        {
                            _currentProfile = newProfile;
                            UpdateProfileList();
                            profileComboBox!.SelectedItem = profileName;
                            LoadProfileToUI();
                            
                            Logger.Log($"Created new profile: {profileName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "NewProfileButton_Click");
                MessageBox.Show($"Failed to create profile: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void DeleteProfileButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_currentProfile.Name == "Default")
                {
                    MessageBox.Show("Cannot delete the default profile.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                var result = MessageBox.Show($"Are you sure you want to delete profile '{_currentProfile.Name}'?", 
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    if (_profileManager?.DeleteProfile(_currentProfile.Name) == true)
                    {
                        _currentProfile = _profileManager.GetProfile("Default");
                        UpdateProfileList();
                        profileComboBox!.SelectedItem = "Default";
                        LoadProfileToUI();
                        
                        MessageBox.Show("Profile deleted successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        Logger.Log($"Deleted profile: {_currentProfile.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "DeleteProfileButton_Click");
                MessageBox.Show($"Failed to delete profile: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void ShowAboutDialog(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "FFB Racing Wheel Properties v1.0.0\n\n" +
                "Configuration tool for OpenFFB Racing Wheels\n" +
                "© 2024 MWI Engineering\n\n" +
                "For support and updates, visit:\n" +
                "https://github.com/yourusername/FFBWheelProperties",
                "About FFB Racing Wheel Properties",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                _isClosing = true;
                
                _connectionTimer?.Stop();
                _connectionTimer?.Dispose();
                
                _hidService?.Dispose();
                
                Logger.Log("Application closing");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "OnFormClosing");
            }
            
            base.OnFormClosing(e);
        }
    }
    
    // Simple text input dialog
    public partial class TextInputDialog : Form
    {
        public string InputText => textBox.Text;
        
        private TextBox textBox;
        private Button okButton;
        private Button cancelButton;
        
        public TextInputDialog(string prompt, string title)
        {
            InitializeComponent(prompt, title);
        }
        
        private void InitializeComponent(string prompt, string title)
        {
            this.Text = title;
            this.Size = new Size(300, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            var promptLabel = new Label
            {
                Text = prompt,
                Location = new Point(10, 15),
                Size = new Size(270, 20)
            };
            
            textBox = new TextBox
            {
                Location = new Point(10, 40),
                Size = new Size(270, 25)
            };
            
            okButton = new Button
            {
                Text = "OK",
                Location = new Point(125, 80),
                Size = new Size(75, 25),
                DialogResult = DialogResult.OK
            };
            
            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(205, 80),
                Size = new Size(75, 25),
                DialogResult = DialogResult.Cancel
            };
            
            this.Controls.AddRange(new Control[] 
            { 
                promptLabel, textBox, okButton, cancelButton 
            });
            
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }
    }
}