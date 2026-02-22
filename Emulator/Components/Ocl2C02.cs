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

        private byte[,] nameTable = new byte[2, 1024];
        private byte[] paletteTable = new byte[32];
        private int[,] patternTable = new int[2, 4096];

        private BaseBitmap spriteScreen;
        private BaseBitmap[] spriteNameTables;
        private BaseBitmap[] spritePatternTables;
        private Color[] screenPalette;

        private bool _frameComplete;
        private int _scanLine;
        private int _cycle;

        private ControlRegister _controlRegister;
        private MaskRegister _maskRegister;
        private StatusRegister _statusRegister;

        private byte _addressLatch = 0x00;
        private byte _ppuDataBuffer = 0x00;
        private LoopyRegister _vramAddress = new LoopyRegister();
        private LoopyRegister _tramAddress = new LoopyRegister();
        private byte _fineXScroll = 0x00;


        private byte _bgNextTileId = 0x00;
        private byte _bgNextTileAttribute = 0x00;
        private byte _bgNextTileLsb = 0x00;
        private byte _bgNextTileMsb = 0x00;


        private int _bgShifterPatternLo = 0x0000;
        private int _bgShifterPatternHi = 0x0000;
        private int _bgShifterAttributeLo = 0x0000;
        private int _bgShifterAttributeHi = 0x0000;

        public bool NonMaskableInterrupt { get; set; }

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

            _controlRegister = new ControlRegister();
            _maskRegister = new MaskRegister();
            _statusRegister = new StatusRegister();
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

        private void IncrementScrollX()
        {
            if (_maskRegister.ShowBackground || _maskRegister.ShowSprites)
            {
                if (_vramAddress.CoarseX == 31)
                {
                    _vramAddress.CoarseX = 0;
                    _vramAddress.NameTableX = ~_vramAddress.NameTableX;
                }
                else
                {
                    _vramAddress.CoarseX++;
                }
            }
        }

        private void IncrementScrollY()
        {
            if (_maskRegister.ShowBackground || _maskRegister.ShowSprites)
            {
                if (_vramAddress.FineY < 7)
                {
                    _vramAddress.FineY++;
                }
                else
                {
                    _vramAddress.FineY = 0;
                    if (_vramAddress.CoarseY == 29)
                    {
                        _vramAddress.CoarseY = 0;
                        _vramAddress.NameTableY = ~_vramAddress.NameTableY;
                    }
                    else if (_vramAddress.CoarseY == 31)
                    {
                        _vramAddress.CoarseY = 0;
                    }
                    else
                    {
                        _vramAddress.CoarseY++;
                    }
                }
            }
        }

        private void TransferAddressX()
        {
            if (_maskRegister.ShowBackground || _maskRegister.ShowSprites)
            {
                _vramAddress.NameTableX = _tramAddress.NameTableX;
                _vramAddress.CoarseX = _tramAddress.CoarseX;
            }
        }

        private void TransferAddressY()
        {
            if (_maskRegister.ShowBackground || _maskRegister.ShowSprites)
            {
                _vramAddress.FineY = _tramAddress.FineY;
                _vramAddress.NameTableY = _tramAddress.NameTableY;
                _vramAddress.CoarseY = _tramAddress.CoarseY;
            }
        }

        private void LoadBackgroundShifters()
        {
            _bgShifterPatternLo = (_bgShifterAttributeLo & 0xFF00) | _bgNextTileLsb;
            _bgShifterPatternHi = (_bgShifterAttributeHi & 0xFF00) | _bgNextTileMsb;

            _bgShifterAttributeLo = (_bgShifterAttributeLo & 0xFF00) | ((_bgNextTileAttribute & 0b01) > 0 ? 0xFF : 0x00);
            _bgShifterAttributeHi = (_bgShifterAttributeHi & 0xFF00) | ((_bgNextTileAttribute & 0b10) > 0 ? 0xFF : 0x00);
        }
        
        private void UpdateShifters()
        {
            if (_maskRegister.ShowBackground)
            {
                _bgShifterPatternLo <<= 1;
                _bgShifterPatternHi <<= 1;
                _bgShifterAttributeLo <<= 1;
                _bgShifterAttributeHi <<= 1;
            }
        }

        public void Clock()
        {
            if (_scanLine >= -1 && _scanLine < 240)
            {
                if (_scanLine == -1 && _cycle == 1)
                {
                    _statusRegister.VerticalBlank = false;
                }

                if ((_cycle >= 2 && _cycle < 258) || (_cycle >= 321 && _cycle < 338))
                {
                    UpdateShifters();
                    int address;
                    switch ((_cycle - 1) % 8)
                    {
                        case 0:
                            LoadBackgroundShifters();
                            _bgNextTileId = PpuRead(0x2000 | (_vramAddress.Value & 0x0FFF));
                            break;
                        case 2:
                            address = 0x23C0 |
                                            (_vramAddress.NameTableY << 11) |
                                            (_vramAddress.NameTableX << 10) |
                                            ((_vramAddress.CoarseY >> 2) << 3) |
                                            (_vramAddress.CoarseX >> 2);
                            _bgNextTileAttribute = PpuRead(address);

                            if ((_vramAddress.CoarseY & 0x02) > 0)
                            {
                                _bgNextTileAttribute >>= 4;
                            }

                            if ((_vramAddress.CoarseX & 0x02) > 0)
                            {
                                _bgNextTileAttribute >>= 2;
                            }

                            _bgNextTileAttribute &= 0x03;

                            break;
                        case 4:

                            address = (_controlRegister.BackgroundPatternTableAddress << 12)
                                      + (_bgNextTileId << 4)
                                      + _vramAddress.FineY
                                      + 0;
                            _bgNextTileLsb = PpuRead(address);

                            break;
                        case 6:
                            address = (_controlRegister.BackgroundPatternTableAddress << 12)
                                      + (_bgNextTileId << 4)
                                      + _vramAddress.FineY
                                      + 8;
                            _bgNextTileLsb = PpuRead(address);

                            break;
                        case 7:

                            IncrementScrollX();

                            break;
                    }
                }

                if (_cycle == 256)
                {
                    IncrementScrollY();
                }

                if (_cycle == 257)
                {
                    TransferAddressX();
                }

                if (_scanLine == -1 && _cycle >= 280 && _cycle < 305)
                {
                    TransferAddressY();
                }
            }
            
            if(_scanLine == 240)
            {
                // Post Render Scanline
            }

            if (_scanLine == 241 && _cycle == 1)
            {
                _statusRegister.VerticalBlank = true;
                if (_controlRegister.EnableNMI)
                    NonMaskableInterrupt = true;
            }

            byte bgPixel = 0x00;
            byte bgPalette = 0x00;

            if(_maskRegister.ShowBackground)
            {
                int bitMux = 0x8000 >> _fineXScroll;

                int p0Pixel = (_bgShifterPatternLo & bitMux) > 0 ? 1 : 0;
                int p1Pixel = (_bgShifterPatternHi & bitMux) > 0 ? 1 : 0;
                bgPixel = (byte)((p1Pixel << 1) | p0Pixel);

                int bgPal0 = (_bgShifterAttributeLo & bitMux) > 0 ? 1 : 0;
                int bgPal1 = (_bgShifterAttributeHi & bitMux) > 0 ? 1 : 0;
                bgPalette = (byte)((bgPal1 << 1) | bgPal0);

                
            }
            spriteScreen.SetPixel(_cycle-1, _scanLine, GetColorFromPaletteRam(bgPalette, bgPixel));
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
                    data = _controlRegister.Value;
                    break;
                case 0x0001: // Mask
                    data = _maskRegister.Value;
                    break;
                case 0x0002: // Status
                    data = (byte)((_statusRegister.Value & 0xE0) | (_ppuDataBuffer & 0x1F));
                    _statusRegister.VerticalBlank = false;
                    _addressLatch = 0;
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
                    data = _ppuDataBuffer;
                    _ppuDataBuffer = PpuRead(_vramAddress.Value);

                    if (_vramAddress.Value > 0x3F00) data = _ppuDataBuffer;
                    _vramAddress.Value += _controlRegister.IncrementMode ? 32 : 1;

                    break;
            }

            return data;
        }

        public void CpuWrite(int address, byte value)
        {
            switch (address)
            {
                case 0x0000: // Control 
                    _controlRegister.Value = value;
                    _tramAddress.NameTableX = _controlRegister.NametableX;
                    _tramAddress.NameTableY = _controlRegister.NametableY;
                    break;
                case 0x0001: // Mask
                    _maskRegister.Value = value;
                    break;
                case 0x0002: // Status
                    break;
                case 0x0003: // OAM Address
                    break;
                case 0x0004: // OAM Data
                    break;
                case 0x0005: // Scroll
                    if (_addressLatch == 0)
                    {
                        _fineXScroll = (byte)(value & 0x07);
                        _tramAddress.CoarseX = value >> 3;
                        _addressLatch = 1;
                    }
                    else
                    {
                        _tramAddress.FineY = (byte)(value & 0x07);
                        _tramAddress.CoarseY = value >> 3;
                        _addressLatch = 0;
                    }
                    break;
                case 0x0006: // PPU Address
                    if (_addressLatch == 0)
                    {
                        _tramAddress.Value = (_tramAddress.Value & 0x00FF) | (value << 8);
                        _addressLatch = 1;
                    }
                    else
                    {
                        _tramAddress.Value = (_tramAddress.Value & 0xFF00) | value;
                        _vramAddress.Value = _tramAddress.Value;
                        _addressLatch = 0;
                    }
                    break;
                case 0x0007: // PPU Data
                    PpuWrite(_vramAddress.Value, value);
                    _vramAddress.Value += _controlRegister.IncrementMode ? 32 : 1;
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
                if (cartridge?.Mirror == Enums.MirrorMode.Vertical)
                {
                    if (address >= 0x0000 && address <= 0x03FF)
                        data = nameTable[0, address & 0x03FF];
                    if (address >= 0x0400 && address <= 0x07FF)
                        data = nameTable[1, address & 0x03FF];
                    if (address >= 0x0800 && address <= 0x0BFF)
                        data = nameTable[0, address & 0x03FF];
                    if (address >= 0x0C00 && address <= 0x0FFF)
                        data = nameTable[1, address & 0x03FF];
                }
                else if (cartridge?.Mirror == Enums.MirrorMode.Horizontal)
                {
                    if (address >= 0x0000 && address <= 0x03FF)
                        data = nameTable[0, address & 0x03FF];
                    if (address >= 0x0400 && address <= 0x07FF)
                        data = nameTable[0, address & 0x03FF];
                    if (address >= 0x0800 && address <= 0x0BFF)
                        data = nameTable[1, address & 0x03FF];
                    if (address >= 0x0C00 && address <= 0x0FFF)
                        data = nameTable[1, address & 0x03FF];
                }
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
                if (cartridge?.Mirror == Enums.MirrorMode.Vertical)
                {
                    if (address >= 0x0000 && address <= 0x03FF)
                        nameTable[0, address & 0x03FF] = value;
                    if (address >= 0x0400 && address <= 0x07FF)
                        nameTable[1, address & 0x03FF] = value;
                    if (address >= 0x0800 && address <= 0x0BFF)
                        nameTable[0, address & 0x03FF] = value;
                    if (address >= 0x0C00 && address <= 0x0FFF)
                        nameTable[1, address & 0x03FF] = value;
                }
                else if (cartridge?.Mirror == Enums.MirrorMode.Horizontal)
                {
                    if (address >= 0x0000 && address <= 0x03FF)
                        nameTable[0, address & 0x03FF] = value;
                    if (address >= 0x0400 && address <= 0x07FF)
                        nameTable[0, address & 0x03FF] = value;
                    if (address >= 0x0800 && address <= 0x0BFF)
                        nameTable[1, address & 0x03FF] = value;
                    if (address >= 0x0C00 && address <= 0x0FFF)
                        nameTable[1, address & 0x03FF] = value;
                }
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

            for (int tileX = 0; tileX < 16; tileX++)
            {
                for (int tileY = 0; tileY < 16; tileY++)
                {
                    int offset = tileY * 256 + tileX * 16;

                    for (int row = 0; row < 8; row++)
                    {
                        int address = (patternTableIndex * 0x1000) + offset + row;

                        byte tileLSB = PpuRead(address);
                        byte tileMSB = PpuRead(address + 0x0008);

                        for (int col = 0; col < 8; col++)
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
            get => _frameComplete;
            set => _frameComplete = value;
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