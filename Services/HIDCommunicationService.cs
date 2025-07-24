using HidLibrary;
using FFBWheelProperties.Models;
using FFBWheelProperties.Utils;

namespace FFBWheelProperties.Services
{
    public class HIDCommunicationService : IDisposable
    {
        private HidDevice? _device;
        private bool _isListening;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly object _lockObject = new object();
        
        public event EventHandler<FFBDevice>? DeviceDataReceived;
        public event EventHandler? DeviceDisconnected;
        public event EventHandler? DeviceConnected;
        
        public bool IsConnected => _device?.IsOpen == true;
        
        public bool Connect()
        {
            try
            {
                lock (_lockObject)
                {
                    if (IsConnected) return true;
                    
                    var devices = HidDevices.Enumerate(FFBDevice.VendorId, FFBDevice.ProductId);
                    _device = devices.FirstOrDefault(d => d.Description.Contains(FFBDevice.ProductName));
                    
                    if (_device != null)
                    {
                        _device.OpenDevice();
                        if (_device.IsOpen)
                        {
                            Logger.Log("Successfully connected to FFB device");
                            StartListening();
                            DeviceConnected?.Invoke(this, EventArgs.Empty);
                            return true;
                        }
                    }
                    
                    Logger.Log("Failed to connect to FFB device - device not found or already in use");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Connection error: {ex.Message}");
                return false;
            }
        }
        
        public void Disconnect()
        {
            lock (_lockObject)
            {
                StopListening();
                
                if (_device != null)
                {
                    _device.CloseDevice();
                    _device.Dispose();
                    _device = null;
                    Logger.Log("Disconnected from FFB device");
                }
            }
        }
        
        public bool SendFFBCommand(FFBEffectType effectType, byte[] data)
        {
            lock (_lockObject)
            {
                if (_device == null || !_device.IsOpen || data.Length > 7)
                    return false;
                
                try
                {
                    var report = new byte[64]; // Report length from your HID descriptor
                    report[0] = (byte)effectType; // Report ID
                    Array.Copy(data, 0, report, 1, Math.Min(data.Length, 7));
                    
                    bool success = _device.WriteOutputReportViaInterruptTransfer(report);
                    if (success)
                    {
                        Logger.Log($"Sent FFB command: Type={effectType}, Data={BitConverter.ToString(data)}");
                    }
                    else
                    {
                        Logger.Log($"Failed to send FFB command: Type={effectType}");
                    }
                    
                    return success;
                }
                catch (Exception ex)
                {
                    Logger.Log($"Send command error: {ex.Message}");
                    return false;
                }
            }
        }
        
        public bool SendFFBEffect(FFBEffect effect)
        {
            return SendFFBCommand(effect.Type, effect.ToByteArray().Skip(1).ToArray());
        }
        
        private void StartListening()
        {
            if (_isListening) return;
            
            _isListening = true;
            _cancellationTokenSource = new CancellationTokenSource();
            
            Task.Run(async () =>
            {
                Logger.Log("Started listening for device data");
                
                while (_isListening && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (_device == null || !_device.IsOpen)
                        {
                            Logger.Log("Device disconnected during read operation");
                            DeviceDisconnected?.Invoke(this, EventArgs.Empty);
                            break;
                        }
                        
                        var inputReport = _device.ReadInputReport(100); // 100ms timeout
                        if (inputReport.ReadStatus == HidDeviceData.ReadStatus.Success)
                        {
                            ProcessInputReport(inputReport.Data);
                        }
                        else if (inputReport.ReadStatus == HidDeviceData.ReadStatus.NoDataRead)
                        {
                            // Normal timeout, continue
                        }
                        else
                        {
                            Logger.Log($"Read error: {inputReport.ReadStatus}");
                            DeviceDisconnected?.Invoke(this, EventArgs.Empty);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Read exception: {ex.Message}");
                        DeviceDisconnected?.Invoke(this, EventArgs.Empty);
                        break;
                    }
                    
                    await Task.Delay(10, _cancellationTokenSource.Token); // 100Hz polling
                }
                
                Logger.Log("Stopped listening for device data");
            }, _cancellationTokenSource.Token);
        }
        
        private void StopListening()
        {
            _isListening = false;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
        
        private void ProcessInputReport(byte[] data)
        {
            if (data.Length < 4) return;
            
            var deviceData = new FFBDevice();
            
            try
            {
                // Parse based on your HID descriptor
                if (data[0] == 1) // Wheel position + buttons report
                {
                    // Parse wheel position (16-bit signed integer)
                    deviceData.WheelPosition = BitConverter.ToInt16(data, 1);
                    
                    // Parse 16 buttons from 2 bytes starting at offset 3
                    if (data.Length >= 5)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            int byteIndex = 3 + (i / 8);
                            int bitIndex = i % 8;
                            if (byteIndex < data.Length)
                            {
                                deviceData.ButtonStates[i] = (data[byteIndex] & (1 << bitIndex)) != 0;
                            }
                        }
                    }
                }
                else if (data[0] == 2) // FFB status report
                {
                    deviceData.FFBStatus = data.Length > 1 ? data[1] : (byte)0;
                }
                
                DeviceDataReceived?.Invoke(this, deviceData);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error processing input report: {ex.Message}");
            }
        }
        
        public void Dispose()
        {
            Disconnect();
            _cancellationTokenSource?.Dispose();
        }
    }
}
