namespace emulator.components.Interfaces
{
    public interface IBus
    {
        // Define methods and properties that the Bus should implement
        int Read(int address);
        void Write(int address, int value, bool readOnly = false);
    }
}