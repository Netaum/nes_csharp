using emulator.components.Interfaces;

namespace emulator.components
{
    public class Ocl2C02: IPpu
    {
        private const int MEMORY_MASK = 0x3FFF;
        private ICartridge? cartridge;

        private int[,] nameTable = new int[2, 1024];
        private int[] paletteTable = new int[32];
        private int[,] patternTable = new int[2, 4096];

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
            throw new NotImplementedException();
        }

        public byte CpuRead(int address, bool readOnly = false)
        {
            byte data = 0x00;

            // Implement CPU read logic
            switch(address)
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
            switch(address)
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

        public void InsertCartridge(ICartridge cartridge)
        {
            this.cartridge = cartridge;
        }

        public byte PpuRead(int address, bool readOnly = false)
        {
            byte data = 0x00;
            address &= MEMORY_MASK;

            var (success, mappedAddress) = Cartridge.PpuRead(address);

            if(success)
            {
                return data;
            }

            return data;
        }

        public void PpuWrite(int address, byte value)
        {
            address &= MEMORY_MASK;

            if(Cartridge.PpuWrite(address, value))
            {
                return;
            }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}