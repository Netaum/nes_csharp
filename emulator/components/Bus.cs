using emulator.components.Interfaces;

namespace emulator.components
{
    public class Bus : IBus
    {
        private const int MAX_ADDRESS = 0x1FFF; // Maximum address for the bus
        private const int MIN_ADDRESS = 0x0000; // Minimum address for the bus
        private const int BUS_SIZE = 2048;
        private const int MIRROR_MASK = 0x07FF;
        private const int PPU_MASK = 0x0007;

        private int clockCounter = 0x00;

        private byte[] cpuMemory;

        private IPpu? ppu;
        private ICartridge? cartridge;
        private ICpu? cpu;

        public ICpu Cpu
        {
            get
            {
                if (cpu is null)
                    throw new NullReferenceException("CPU is null");

                return cpu;
            }
        }

        public IPpu Ppu
        {
            get
            {
                if (ppu is null)
                    throw new NullReferenceException("PPU is null");

                return ppu;
            }
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

        public Bus()
        {
            cpuMemory = new byte[BUS_SIZE];
            for(int i = 0; i < BUS_SIZE; i++)
            {
                cpuMemory[i] = 0x00;
            }
        }

        public void Clock()
        {
            Ppu.Clock();
            if (clockCounter % 3 == 0)
                Cpu.Clock();
            clockCounter++;
        }

        // Implement the methods and properties defined in the IBus interface
        public byte CpuRead(int address, bool readOnly = false)
        {
            var (cartridgeSuccess, _) = Cartridge.CpuRead(address);
            
            if (cartridgeSuccess)
            {
                return 0x00;
            }

            if (address >= MIN_ADDRESS && address <= MAX_ADDRESS)
            {
                return cpuMemory[address & MIRROR_MASK];
            }
            else if(address >= 0x2000 && address <= 0x3FFF)
            {
                Ppu.CpuRead(address & PPU_MASK);
            }
            
            return 0x00;
        }

        public void CpuWrite(int address, byte value)
        {
            if (Cartridge.CpuWrite(address, value))
            {
                return;
            }
            
            if (address >= MIN_ADDRESS && address <= MAX_ADDRESS)
            {
                cpuMemory[address & MIRROR_MASK] = value;
            }
            else if(address >= 0x2000 && address <= 0x3FFF)
            {
                Ppu.CpuWrite(address & PPU_MASK, value);
            }
        }

        public void InsertCartridge(ICartridge cartridge)
        {
            this.cartridge = cartridge;
            Ppu.InsertCartridge(cartridge);
        }

        public void ConnectCpu(ICpu cpu)
        {
            this.cpu = cpu;
            this.cpu.ConnectBus(this);
        }

        public void ConnectPpu(IPpu ppu)
        {
            this.ppu = ppu;
        }

        public void Reset()
        {
            clockCounter = 0;
            Cpu.Reset();
        }
    }
}