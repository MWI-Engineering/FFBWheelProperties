namespace FFBWheelProperties.Utils
{
    public static class HIDConstants
    {
        // Report IDs from your HID descriptor
        public const byte REPORT_WHEEL_STATE = 1;
        public const byte REPORT_FFB_STATUS = 2;
        public const byte REPORT_CONSTANT_FORCE = 3;
        public const byte REPORT_SPRING_EFFECT = 4;
        public const byte REPORT_DAMPER_EFFECT = 5;
        public const byte REPORT_FRICTION_EFFECT = 6;
        public const byte REPORT_INERTIA_EFFECT = 7;
        public const byte REPORT_PERIODIC_EFFECT = 8;
        public const byte REPORT_CUSTOM_EFFECT = 9;
        
        // Device identification
        public const int VENDOR_ID = 0x1209;
        public const int PRODUCT_ID = 0x4711;
        public const string MANUFACTURER = "MWI_Engineering";
        public const string PRODUCT_NAME = "OpenFFB Wheel";
        
        // Report sizes
        public const int INPUT_REPORT_SIZE = 64;
        public const int OUTPUT_REPORT_SIZE = 64;
        
        // Wheel parameters
        public const int MAX_WHEEL_ROTATION = 900; // degrees
        public const int MIN_WHEEL_ROTATION = 180;
        public const int MAX_WHEEL_POSITION = 32767;
        public const int MIN_WHEEL_POSITION = -32768;
        
        // FFB parameters
        public const int MAX_FFB_STRENGTH = 255;
        public const int MIN_FFB_STRENGTH = 0;
        public const int DEFAULT_FFB_STRENGTH = 127;
    }
}