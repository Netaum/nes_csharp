using CustomTypes;
using Emulator.Components.Interfaces;
using Emulator.Primitives;
using System.Drawing;

namespace Emulator.Components
{
    public class Ocl2C02 : IPpu
    {
        private const int MEMORY_MASK = 0x3FFF;
        private ICartridge? cartridge;

        private int[,] nameTable = new int[2, 1024];
        private byte[] paletteTable = new byte[32];
        private int[,] patternTable = new int[2, 4096];

        private BaseBitmap spriteScreen;
        private BaseBitmap[] spriteNameTables;
        private BaseBitmap[] spritePatternTables;
        private Color[] screenPalette;

        private bool frameComplete;
        private int scanLine;
        private int cycle;

        private ControlRegister controlRegister;
        private MaskRegister maskRegister;
        private StatusRegister statusRegister;

        private byte addressLatch = 0x00;
        private byte ppuDataBuffer = 0x00;
        private int ppuAddress = 0x0000;

        public Ocl2C02()
        {
            spriteScreen = BaseBitmap.Create(256, 240);
            spriteNameTables =
            [
                BaseBitmap.Create(256, 240),
                BaseBitmap.Create(256, 240)
            ];
            spritePatternTables =
            [
                BaseBitmap.Create(128, 128),
                BaseBitmap.Create(128, 128)
            ];

            screenPalette = new Color[0x40];

            controlRegister = new ControlRegister();
            maskRegister = new MaskRegister();
            statusRegister = new StatusRegister();
            InitPallete();
        }

        public ICartridge Cartridge
        {
            get
            {
                if (cartridge is null)
                    throw new NullReferenceException("Cartridge is null");

                return cartridge;
            }
        }

        public void Clock()
        {
            var i = new Random().Next(0, 2);
            spriteScreen.SetPixel(int.Clamp(cycle, 0, 255), int.Clamp(scanLine, 0, 239), i == 0 ? screenPalette[0x3F] : screenPalette[0x30]);
            cycle++;
            if (cycle >= 341)
            {
                cycle = 0;
                scanLine++;

                if (scanLine >= 261)
                {
                    scanLine = -1;
                    frameComplete = true;
                }
            }
        }

        public byte CpuRead(int address, bool readOnly = false)
        {
            byte data = 0x00;

            // Implement CPU read logic
            switch (address)
            {
                case 0x0000: // Control 
                    data = controlRegister.Value;
                    break;
                case 0x0001: // Mask
                    data = maskRegister.Value;
                    break;
                case 0x0002: // Status
                    statusRegister.VerticalBlank = true;
                    data = (byte)((statusRegister.Value & 0xE0) | (ppuDataBuffer & 0x1F));
                    statusRegister.VerticalBlank = false;
                    addressLatch = 0;
                    break;
                case 0x0003: // OAM Address
                    break;
                case 0x0004: // OAM Data
                    break;
                case 0x0005: // Scroll
                    break;
                case 0x0006: // PPU Address
                    break;
                case 0x0007: // PPU Data
                    data = ppuDataBuffer;
                    ppuDataBuffer = PpuRead(ppuAddress);

                    if (ppuAddress > 0x3F00) data = ppuDataBuffer;
                    ppuAddress++;

                    break;
            }

            return data;
        }

        public void CpuWrite(int address, byte value)
        {
            switch (address)
            {
                case 0x0000: // Control 
                    controlRegister.Value = value;
                    break;
                case 0x0001: // Mask
                    maskRegister.Value = value;
                    break;
                case 0x0002: // Status
                    break;
                case 0x0003: // OAM Address
                    break;
                case 0x0004: // OAM Data
                    break;
                case 0x0005: // Scroll
                    break;
                case 0x0006: // PPU Address
                    if (addressLatch == 0)
                    {
                        ppuAddress = (ppuAddress & 0x00FF) | (value << 8);
                        addressLatch = 1;
                    }
                    else
                    {
                        ppuAddress = (ppuAddress & 0xFF00) | value;
                        addressLatch = 0;
                    }
                    break;
                case 0x0007: // PPU Data
                    PpuWrite(ppuAddress, value);
                    ppuAddress++;
                    break;
            }
        }

        public void InsertCartridge(ICartridge cartridge)
        {
            this.cartridge = cartridge;
        }

        public byte PpuRead(int address, bool readOnly = false)
        {
            byte data = 0x00;
            address &= MEMORY_MASK;

            var (success, mappedData) = Cartridge.PpuRead(address);

            if (success)
            {
                return mappedData;
            }
            else if (address >= 0x0000 && address <= 0x1FFF)
            {
                //data = patternTable[]
            }
            else if (address >= 0x2000 && address <= 0x3EFF)
            {

            }
            else if (address >= 0x3F00 && address <= 0x3FFF)
            {
                address &= 0x001F;
                if (address == 0x0010) address = 0x0000;
                if (address == 0x0014) address = 0x0004;
                if (address == 0x0018) address = 0x0008;
                if (address == 0x001C) address = 0x000C;

                data = paletteTable[address];
            }
            return data;
        }

        public void PpuWrite(int address, byte value)
        {
            address &= MEMORY_MASK;

            if (Cartridge.PpuWrite(address, value))
            {
                return;
            }
            else if (address >= 0x0000 && address <= 0x1FFF)
            {

            }
            else if (address >= 0x2000 && address <= 0x3EFF)
            {

            }
            else if (address >= 0x3F00 && address <= 0x3FFF)
            {
                address &= 0x001F;
                if (address == 0x0010) address = 0x0000;
                if (address == 0x0014) address = 0x0004;
                if (address == 0x0018) address = 0x0008;
                if (address == 0x001C) address = 0x000C;

                paletteTable[address] = value;
            }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public BaseBitmap GetScreen()
        {
            return spriteScreen;
        }

        public BaseBitmap GetNameTable(int i)
        {
            return spriteNameTables[i];
        }

        public Color GetColorFromPaletteRam(int palette, int pixel)
        {
            var colorAddress = 0x3F00 + (palette << 2) + pixel;
            var ppuValue = PpuRead(colorAddress);
            int value = ppuValue & 0x3F;
            return screenPalette[value];
        }

        public BaseBitmap GetPatternTable(int patternTableIndex, int palette)
        {

            for(int tileX = 0; tileX < 16; tileX++)
            {
                for(int tileY = 0; tileY < 16; tileY++)
                {
                    int offset = tileY * 256 + tileX * 16;

                    for(int row = 0; row < 8; row++)
                    {
                        int address = (patternTableIndex * 0x1000) + offset + row;

                        byte tileLSB = PpuRead(address);
                        byte tileMSB = PpuRead(address + 0x0008);

                        for(int col = 0; col < 8; col++)
                        {
                            int pixel = (tileLSB & 0x01) + (tileMSB & 0x01);
                            tileLSB >>= 1;
                            tileMSB >>= 1;

                            int pixelX = tileX * 8 + (7 - col);
                            int pixelY = tileY * 8 + row;
                            spritePatternTables[patternTableIndex].SetPixel(pixelX, pixelY, GetColorFromPaletteRam(palette, pixel));
                        }
                    }
                }
            }

            return spritePatternTables[patternTableIndex];
        }

        public bool FrameComplete
        {
            get => frameComplete;
            set => frameComplete = value;
        }


        private void InitPallete()
        {
            screenPalette[0x00] = Color.FromArgb(84, 84, 84);
            screenPalette[0x01] = Color.FromArgb(0, 30, 116);
            screenPalette[0x02] = Color.FromArgb(8, 16, 144);
            screenPalette[0x03] = Color.FromArgb(48, 0, 136);
            screenPalette[0x04] = Color.FromArgb(68, 0, 100);
            screenPalette[0x05] = Color.FromArgb(92, 0, 48);
            screenPalette[0x06] = Color.FromArgb(84, 4, 0);
            screenPalette[0x07] = Color.FromArgb(60, 24, 0);
            screenPalette[0x08] = Color.FromArgb(32, 42, 0);
            screenPalette[0x09] = Color.FromArgb(8, 58, 0);
            screenPalette[0x0A] = Color.FromArgb(0, 64, 0);
            screenPalette[0x0B] = Color.FromArgb(0, 60, 0);
            screenPalette[0x0C] = Color.FromArgb(0, 50, 60);
            screenPalette[0x0D] = Color.FromArgb(0, 0, 0);
            screenPalette[0x0E] = Color.FromArgb(0, 0, 0);
            screenPalette[0x0F] = Color.FromArgb(0, 0, 0);

            screenPalette[0x10] = Color.FromArgb(152, 150, 152);
            screenPalette[0x11] = Color.FromArgb(8, 76, 196);
            screenPalette[0x12] = Color.FromArgb(48, 50, 236);
            screenPalette[0x13] = Color.FromArgb(92, 30, 228);
            screenPalette[0x14] = Color.FromArgb(136, 20, 176);
            screenPalette[0x15] = Color.FromArgb(160, 20, 100);
            screenPalette[0x16] = Color.FromArgb(152, 34, 32);
            screenPalette[0x17] = Color.FromArgb(120, 60, 0);
            screenPalette[0x18] = Color.FromArgb(84, 90, 0);
            screenPalette[0x19] = Color.FromArgb(40, 114, 0);
            screenPalette[0x1A] = Color.FromArgb(8, 124, 0);
            screenPalette[0x1B] = Color.FromArgb(0, 118, 40);
            screenPalette[0x1C] = Color.FromArgb(0, 102, 120);
            screenPalette[0x1D] = Color.FromArgb(0, 0, 0);
            screenPalette[0x1E] = Color.FromArgb(0, 0, 0);
            screenPalette[0x1F] = Color.FromArgb(0, 0, 0);

            screenPalette[0x20] = Color.FromArgb(236, 238, 236);
            screenPalette[0x21] = Color.FromArgb(76, 154, 236);
            screenPalette[0x22] = Color.FromArgb(120, 124, 236);
            screenPalette[0x23] = Color.FromArgb(176, 98, 236);
            screenPalette[0x24] = Color.FromArgb(228, 84, 236);
            screenPalette[0x25] = Color.FromArgb(236, 88, 180);
            screenPalette[0x26] = Color.FromArgb(236, 106, 100);
            screenPalette[0x27] = Color.FromArgb(212, 136, 32);
            screenPalette[0x28] = Color.FromArgb(160, 170, 0);
            screenPalette[0x29] = Color.FromArgb(116, 196, 0);
            screenPalette[0x2A] = Color.FromArgb(76, 208, 32);
            screenPalette[0x2B] = Color.FromArgb(56, 204, 108);
            screenPalette[0x2C] = Color.FromArgb(56, 180, 204);
            screenPalette[0x2D] = Color.FromArgb(60, 60, 60);
            screenPalette[0x2E] = Color.FromArgb(0, 0, 0);
            screenPalette[0x2F] = Color.FromArgb(0, 0, 0);

            screenPalette[0x30] = Color.FromArgb(236, 238, 236);
            screenPalette[0x31] = Color.FromArgb(168, 204, 236);
            screenPalette[0x32] = Color.FromArgb(188, 188, 236);
            screenPalette[0x33] = Color.FromArgb(212, 178, 236);
            screenPalette[0x34] = Color.FromArgb(236, 174, 236);
            screenPalette[0x35] = Color.FromArgb(236, 174, 212);
            screenPalette[0x36] = Color.FromArgb(236, 180, 176);
            screenPalette[0x37] = Color.FromArgb(228, 196, 144);
            screenPalette[0x38] = Color.FromArgb(204, 210, 120);
            screenPalette[0x39] = Color.FromArgb(180, 222, 120);
            screenPalette[0x3A] = Color.FromArgb(168, 226, 144);
            screenPalette[0x3B] = Color.FromArgb(152, 226, 180);
            screenPalette[0x3C] = Color.FromArgb(160, 214, 228);
            screenPalette[0x3D] = Color.FromArgb(160, 162, 160);
            screenPalette[0x3E] = Color.FromArgb(0, 0, 0);
            screenPalette[0x3F] = Color.FromArgb(0, 0, 0);
        }
    }
}