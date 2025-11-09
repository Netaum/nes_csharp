namespace Emulator.Components.Mappers
{
    public abstract class Mapper : IMapper
    {
        protected int programBanks = 0;
        protected int characterBanks = 0;

        public Mapper(int programBanks, int characterBanks)
        {
            this.programBanks = programBanks;
            this.characterBanks = characterBanks;
        }

        public abstract (bool, int) CpuRead(int address);
        public abstract (bool, int) CpuWrite(int address);
        public abstract (bool, int) PpuRead(int address);
        public abstract (bool, int) PpuWrite(int address);

        (bool, int) IMapper.CpuRead(int address)
        {
            throw new NotImplementedException();
        }

        (bool, int) IMapper.CpuWrite(int address)
        {
            throw new NotImplementedException();
        }

        (bool, int) IMapper.PpuRead(int address)
        {
            throw new NotImplementedException();
        }

        (bool, int) IMapper.PpuWrite(int address)
        {
            throw new NotImplementedException();
        }
    }
}