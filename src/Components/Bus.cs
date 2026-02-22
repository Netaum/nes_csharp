using Interfaces;

namespace Components
{
    public class Bus : IBus
    {
        private readonly ICpu _cpu;
        private readonly IPpu _ppu;
        private ICartridge? _cartridge;

        private const int MAX_ADDRESS = 0x1FFF; // Maximum address for the bus
        private const int MIN_ADDRESS = 0x0000; // Minimum address for the bus
        private const int BUS_SIZE = 2048; // Size of the CPU memory
        private const int MIRROR_MASK = 0x07FF; // Mask for mirroring addresses in the CPU memory
        private const int PPU_MASK = 0x0007; // Mask for mirroring addresses in the PPU registers
        private int clockCounter = 0x00; // Counter to keep track of the number of clock cycles
        private byte[] cpuMemory; // Array to represent the CPU memory

        public Bus(ICpu cpu, IPpu ppu)
        {
            _cpu = cpu;
            _ppu = ppu;

            cpuMemory = new byte[BUS_SIZE];
            for (int i = 0; i < BUS_SIZE; i++)
            {
                cpuMemory[i] = 0x00;
            }
        }

        public ICpu Cpu => _cpu;

        public IPpu Ppu => _ppu;

        public ICartridge Cartridge => _cartridge ?? throw new InvalidOperationException("Cartridge not inserted");

        public void Clock()
        {
            Ppu.Clock();
            if (clockCounter % 3 == 0)
                Cpu.Clock();
            clockCounter++;
        }

        public byte CpuRead(int address, bool readOnly = false)
        {
            var (cartridgeSuccess, cartridgeData) = Cartridge.CpuRead(address);

            if (cartridgeSuccess)
            {
                return cartridgeData;
            }

            if (address >= MIN_ADDRESS && address <= MAX_ADDRESS)
            {
                return cpuMemory[address & MIRROR_MASK];
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
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
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                Ppu.CpuWrite(address & PPU_MASK, value);
            }
        }

        public void InsertCartridge(ICartridge cartridge)
        {
            _cartridge = cartridge;
            Ppu.InsertCartridge(cartridge);
        }

        public void Reset()
        {
            clockCounter = 0;
            Cpu.Reset();
        }
    }
}