namespace Emulator.Components.Mappers
{
    public class Mapper000 : Mapper
    {
        public Mapper000(int programBanks, int characterBanks)
            : base(programBanks, characterBanks)
        {
        }
        
        public override (bool, int) CpuRead(int address)
        {
            int mappedAddress = 0x00;

            if (address >= 0x8000 && address <= 0xFFFF)
            {
                mappedAddress = address & (programBanks > 1 ? 0x7FFF : 0x3FFF);
                return (true, mappedAddress);
            }

            return (false, mappedAddress);
        }

        public override (bool, int) CpuWrite(int address)
        {
            int mappedAddress = 0x00;

            if (address >= 0x8000 && address <= 0xFFFF)
            {
                mappedAddress = address & (programBanks > 1 ? 0x7FFF : 0x3FFF);
                return (true, mappedAddress);
            }
            return (false, mappedAddress);
        }

        public override (bool, int) PpuRead(int address)
        {
            int mappedAddress = 0x00;

            if (address >= 0x0000 && address <= 0x1FFF)
            {
                return (true, address);
            }

            return (false, mappedAddress);
        }

        public override (bool, int) PpuWrite(int address)
        {
            return (false, 0x00);
        }
    }
}