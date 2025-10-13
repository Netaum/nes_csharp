using System.Drawing;

namespace emulator.components.Interfaces
{
    public interface IPpu
    {
        byte CpuRead(int address, bool readOnly = false);
        void CpuWrite(int address, byte value);


        byte PpuRead(int address, bool readOnly = false);
        void PpuWrite(int address, byte value);

        void InsertCartridge(ICartridge cartridge);
        void Reset();
        void Clock();

        Bitmap GetScreen();
        Bitmap GetNameTable(int i);
        Bitmap GetPatternTable(int i);
        bool FrameComplete { get; set; }
    }
}