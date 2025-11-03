namespace emulator.components.Interfaces
{
    public interface IBus
    {
        byte CpuRead(int address, bool readOnly = false);
        void CpuWrite(int address, byte value);

        void InsertCartridge(ICartridge cartridge);
        void Reset();
        void Clock();

        ICpu Cpu { get; }
        IPpu Ppu { get; }
        ICartridge Cartridge { get; }
    }
}