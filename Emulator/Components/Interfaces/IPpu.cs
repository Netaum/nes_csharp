using System.Drawing;
using CustomTypes;

namespace Emulator.Components.Interfaces
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

        BaseBitmap GetScreen();
        BaseBitmap GetNameTable(int i);
        BaseBitmap GetPatternTable(int i, int palette);
        bool FrameComplete { get; set; }
        Color GetColorFromPaletteRam(int palette, int pixel);
    }
}