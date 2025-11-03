namespace emulator.components
{
    public static class Helper
    {
        public static byte ToByte(this int value)
        {
            return (byte)(value & 0x00FF);
        }
    }
}