using System.Drawing;
using Interfaces;
using System.Runtime.Versioning;

namespace Chips
{
    public class Ocl2C02 : IPpu
    {
        private const int MEMORY_MASK = 0x3FFF;

        private ICartridge? _cartridge;
        private int[,] _nameTable = new int[2, 1024];
        private byte[] _paletteTable = new byte[32];
        private int[,] _patternTable = new int[2, 4096];

        private Bitmap _spriteScreen;
        private Bitmap[] _spriteNameTables;
        private Bitmap[] _spritePatternTables;
        private Color[] _screenPalette;

        private bool _frameComplete;
        private int _scanLine;
        private int _cycle;


        [SupportedOSPlatform("windows")]
        public Ocl2C02()
        {
            _spriteScreen = new Bitmap(256, 240);
            _spriteNameTables = [new Bitmap(256, 240), new Bitmap(256, 240)];
            _spritePatternTables = [new Bitmap(128, 128), new Bitmap(128, 128)];
            _screenPalette = new Color[0x40];
            InitPallete();
        }

        private ICartridge cartridge => _cartridge ?? throw new Exception("No cartridge inserted");

        public bool FrameComplete
        {
            get => _frameComplete;
            set => _frameComplete = value;
        }
        

        public void Clock()
        {
            _cycle++;
            if (_cycle >= 341)
            {
                _cycle = 0;
                _scanLine++;

                if (_scanLine >= 261)
                {
                    _scanLine = -1;
                    _frameComplete = true;
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
                    break;
                case 0x0001: // Mask
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
                    break;
                case 0x0007: // PPU Data
                    break;
            }

            return data;
        }

        public void CpuWrite(int address, byte value)
        {
            switch (address)
            {
                case 0x0000: // Control 
                    break;
                case 0x0001: // Mask
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
                    break;
                case 0x0007: // PPU Data
                    break;
            }
        }

        public Bitmap GetNameTable(int i)
        {
            return _spriteNameTables[i];
        }

         private Color GetColorFromPaletteRam(int palette, int pixel)
        {
            var colorAddress = 0x3F00 + (palette << 2) + pixel;
            byte value = PpuRead(colorAddress);
            return _screenPalette[value];
        }

        [SupportedOSPlatform("windows")]
        public Bitmap GetPatternTable(int i, int palette)
        {
            for(int tileX = 0; i < 16; tileX++)
            {
                for(int tileY = 0; tileY < 16; tileY++)
                {
                    int offset = tileY * 256 + tileX * 16;

                    for(int row = 0; row < 8; row++)
                    {
                        int address = i * 0x1000 + offset + row;
                        byte tileLSB = PpuRead(address);
                        byte tileMSB = PpuRead(address + 8);

                        for(int col = 0; col < 8; col++)
                        {
                            int pixel = (tileLSB & 0x01) + (tileMSB & 0x01);
                            tileLSB >>= 1;
                            tileMSB >>= 1;

                            int pixelX = tileX * 8 + (7 - col);
                            int pixelY = tileY * 8 + row;
                            _spritePatternTables[i].SetPixel(pixelX, pixelY, GetColorFromPaletteRam(palette, pixel));
                        }
                    }
                }
            }

            return _spritePatternTables[i];
        }

        public Bitmap GetScreen()
        {
           return _spriteScreen;
        }

        public void InsertCartridge(ICartridge cartridge)
        {
            _cartridge = cartridge;
        }

        public byte PpuRead(int address, bool readOnly = false)
        {
            byte data = 0x00;
            address &= MEMORY_MASK;

            var (success, _mappedAddress) = cartridge.PpuRead(address);

            if (success)
            {
                return data;
            }
            else if (address >= 0x0000 && address <= 0x1FFF)
            {
                //data = patternTable[1, address];
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

                data = _paletteTable[address];
            }
            return data;
        }

        public void PpuWrite(int address, byte value)
        {
            address &= MEMORY_MASK;

            if (cartridge.PpuWrite(address, value))
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

                _paletteTable[address] = value;
            }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        private void InitPallete()
        {
            _screenPalette[0x00] = Color.FromArgb(84, 84, 84);
            _screenPalette[0x01] = Color.FromArgb(0, 30, 116);
            _screenPalette[0x02] = Color.FromArgb(8, 16, 144);
            _screenPalette[0x03] = Color.FromArgb(48, 0, 136);
            _screenPalette[0x04] = Color.FromArgb(68, 0, 100);
            _screenPalette[0x05] = Color.FromArgb(92, 0, 48);
            _screenPalette[0x06] = Color.FromArgb(84, 4, 0);
            _screenPalette[0x07] = Color.FromArgb(60, 24, 0);
            _screenPalette[0x08] = Color.FromArgb(32, 42, 0);
            _screenPalette[0x09] = Color.FromArgb(8, 58, 0);
            _screenPalette[0x0A] = Color.FromArgb(0, 64, 0);
            _screenPalette[0x0B] = Color.FromArgb(0, 60, 0);
            _screenPalette[0x0C] = Color.FromArgb(0, 50, 60);
            _screenPalette[0x0D] = Color.FromArgb(0, 0, 0);
            _screenPalette[0x0E] = Color.FromArgb(0, 0, 0);
            _screenPalette[0x0F] = Color.FromArgb(0, 0, 0);

            _screenPalette[0x10] = Color.FromArgb(152, 150, 152);
            _screenPalette[0x11] = Color.FromArgb(8, 76, 196);
            _screenPalette[0x12] = Color.FromArgb(48, 50, 236);
            _screenPalette[0x13] = Color.FromArgb(92, 30, 228);
            _screenPalette[0x14] = Color.FromArgb(136, 20, 176);
            _screenPalette[0x15] = Color.FromArgb(160, 20, 100);
            _screenPalette[0x16] = Color.FromArgb(152, 34, 32);
            _screenPalette[0x17] = Color.FromArgb(120, 60, 0);
            _screenPalette[0x18] = Color.FromArgb(84, 90, 0);
            _screenPalette[0x19] = Color.FromArgb(40, 114, 0);
            _screenPalette[0x1A] = Color.FromArgb(8, 124, 0);
            _screenPalette[0x1B] = Color.FromArgb(0, 118, 40);
            _screenPalette[0x1C] = Color.FromArgb(0, 102, 120);
            _screenPalette[0x1D] = Color.FromArgb(0, 0, 0);
            _screenPalette[0x1E] = Color.FromArgb(0, 0, 0);
            _screenPalette[0x1F] = Color.FromArgb(0, 0, 0);

            _screenPalette[0x20] = Color.FromArgb(236, 238, 236);
            _screenPalette[0x21] = Color.FromArgb(76, 154, 236);
            _screenPalette[0x22] = Color.FromArgb(120, 124, 236);
            _screenPalette[0x23] = Color.FromArgb(176, 98, 236);
            _screenPalette[0x24] = Color.FromArgb(228, 84, 236);
            _screenPalette[0x25] = Color.FromArgb(236, 88, 180);
            _screenPalette[0x26] = Color.FromArgb(236, 106, 100);
            _screenPalette[0x27] = Color.FromArgb(212, 136, 32);
            _screenPalette[0x28] = Color.FromArgb(160, 170, 0);
            _screenPalette[0x29] = Color.FromArgb(116, 196, 0);
            _screenPalette[0x2A] = Color.FromArgb(76, 208, 32);
            _screenPalette[0x2B] = Color.FromArgb(56, 204, 108);
            _screenPalette[0x2C] = Color.FromArgb(56, 180, 204);
            _screenPalette[0x2D] = Color.FromArgb(60, 60, 60);
            _screenPalette[0x2E] = Color.FromArgb(0, 0, 0);
            _screenPalette[0x2F] = Color.FromArgb(0, 0, 0);

            _screenPalette[0x30] = Color.FromArgb(236, 238, 236);
            _screenPalette[0x31] = Color.FromArgb(168, 204, 236);
            _screenPalette[0x32] = Color.FromArgb(188, 188, 236);
            _screenPalette[0x33] = Color.FromArgb(212, 178, 236);
            _screenPalette[0x34] = Color.FromArgb(236, 174, 236);
            _screenPalette[0x35] = Color.FromArgb(236, 174, 212);
            _screenPalette[0x36] = Color.FromArgb(236, 180, 176);
            _screenPalette[0x37] = Color.FromArgb(228, 196, 144);
            _screenPalette[0x38] = Color.FromArgb(204, 210, 120);
            _screenPalette[0x39] = Color.FromArgb(180, 222, 120);
            _screenPalette[0x3A] = Color.FromArgb(168, 226, 144);
            _screenPalette[0x3B] = Color.FromArgb(152, 226, 180);
            _screenPalette[0x3C] = Color.FromArgb(160, 214, 228);
            _screenPalette[0x3D] = Color.FromArgb(160, 162, 160);
            _screenPalette[0x3E] = Color.FromArgb(0, 0, 0);
            _screenPalette[0x3F] = Color.FromArgb(0, 0, 0);
        }
    }
}