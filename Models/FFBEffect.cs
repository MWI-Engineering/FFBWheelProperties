namespace FFBWheelProperties.Models
{
    public enum FFBEffectType : byte
    {
        ConstantForce = 3,
        Spring = 4,
        Damper = 5,
        Friction = 6,
        Inertia = 7,
        Periodic = 8,
        Custom = 9
    }
    
    public class FFBEffect
    {
        public FFBEffectType Type { get; set; }
        public byte Strength { get; set; }
        public byte Direction { get; set; }
        public byte Duration { get; set; }
        public byte[] Parameters { get; set; } = new byte[4];
        
        public byte[] ToByteArray()
        {
            var result = new byte[8];
            result[0] = (byte)Type;
            result[1] = Strength;
            result[2] = Direction;
            result[3] = Duration;
            Array.Copy(Parameters, 0, result, 4, Math.Min(Parameters.Length, 4));
            return result;
        }
        
        public static FFBEffect CreateConstantForce(int strength, int direction = 127)
        {
            return new FFBEffect
            {
                Type = FFBEffectType.ConstantForce,
                Strength = (byte)Math.Clamp(strength, 0, 255),
                Direction = (byte)Math.Clamp(direction, 0, 255),
                Duration = 0, // Continuous
                Parameters = new byte[4]
            };
        }
        
        public static FFBEffect CreateSpringEffect(int stiffness, int center = 127)
        {
            return new FFBEffect
            {
                Type = FFBEffectType.Spring,
                Strength = (byte)Math.Clamp(stiffness, 0, 255),
                Direction = (byte)Math.Clamp(center, 0, 255),
                Duration = 0,
                Parameters = new byte[4]
            };
        }
    }
}