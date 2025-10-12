using emulator.components.Interfaces;
using emulator.components.Mappers;

namespace emulator.components
{
    public class CartridgeHeader : ICartridgeHeader
    {
        public string Name { get; private set; }
        public int programRoomChunks { get; private set; }
        public int characterRoomChunks { get; private set; }
        public int MapperType1 { get; private set; }
        public int MapperType2 { get; private set; }

        public int ProgramRamSize { get; private set; }
        public int TVSystem1 { get; private set; }
        public int TVSystem2 { get; private set; }

        private CartridgeHeader(string name, int prgChunks, int chrChunks, int mapper1, int mapper2, int prgRamSize, int tvSys1, int tvSys2)
        {
            Name = name;
            programRoomChunks = prgChunks;
            characterRoomChunks = chrChunks;
            MapperType1 = mapper1;
            MapperType2 = mapper2;
            ProgramRamSize = prgRamSize;
            TVSystem1 = tvSys1;
            TVSystem2 = tvSys2;
        }

        public static CartridgeHeader FromBytes(byte[] headerBytes)
        {
            if (headerBytes.Length < 16 || headerBytes[0] != 0x4E || headerBytes[1] != 0x45 || headerBytes[2] != 0x53 || headerBytes[3] != 0x1A)
            {
                throw new ArgumentException("Invalid NES file header");
            }

            string name = "Unknown";
            int prgChunks = headerBytes[4];
            int chrChunks = headerBytes[5];
            int mapper1 = headerBytes[6];
            int mapper2 = headerBytes[7];
            int prgRamSize = headerBytes[8] == 0 ? 1 : headerBytes[8]; // in 8KB units
            int tvSys1 = headerBytes[9];
            int tvSys2 = headerBytes[10];

            return new CartridgeHeader(name, prgChunks, chrChunks, mapper1, mapper2, prgRamSize, tvSys1, tvSys2);
        }
    }
    public class Cartridge : ICartridge
    {
        private byte[] programMemory;
        private byte[] characterMemory;
        private int mapperType = 0x00;
        private int programBank = 0x00;
        private int characterBank = 0x00;
        private Mapper mapper;

        public void LoadCartridge(byte[] cartridgeData)
        {
            var headerData = cartridgeData.Take(16);
            var cartridgeHeader = CartridgeHeader.FromBytes(headerData.ToArray());

            int startData = (cartridgeHeader.MapperType1 & 0x04) > 0 ?
                            512 + 16 :
                            16;

            mapperType = ((cartridgeHeader.MapperType2 >> 4) << 4) | (cartridgeHeader.MapperType1 >> 4);

            switch (mapperType)
            {
                case 0x00:
                    mapper = new Mapper000(cartridgeHeader.programRoomChunks, cartridgeHeader.characterRoomChunks);
                    break;
                default:
                    throw new NotImplementedException($"Mapper type {mapperType} not supported");
            }

            int fileType = 1;

            if (fileType == 0)
            {

            }

            if (fileType == 1)
            {
                const int PRG_ROM_UNIT = 16384;
                const int CHR_ROM_UNIT = 8192;

                programBank = cartridgeHeader.programRoomChunks;
                characterBank = cartridgeHeader.characterRoomChunks;
                programMemory = new byte[programBank * PRG_ROM_UNIT];
                characterMemory = new byte[characterBank * CHR_ROM_UNIT];

                Array.Copy(cartridgeData, startData, programMemory, 0, programMemory.Length);
                Array.Copy(cartridgeData, startData + programMemory.Length, characterMemory, 0, characterMemory.Length);

            }

        }

        public (bool, byte) CpuRead(int address)
        {
            var (success, mappedAddress) = mapper.CpuRead(address);
            byte data = 0x00;
            if (success)
            {
                data = programMemory[mappedAddress];
            }
            return (success, data);
        }

        public bool CpuWrite(int address, byte value)
        {
            var (success, mappedAddress) = mapper.CpuWrite(address);
            if (success)
                programMemory[mappedAddress] = value;

            return success;
        }

        public (bool, byte) PpuRead(int address)
        {
            var (success, mappedAddress) = mapper.PpuRead(address);
            byte data = 0x00;
            if (success)
            {
                data = characterMemory[mappedAddress];
            }
            return (success, data);
        }

        public bool PpuWrite(int address, byte value)
        {
            var (success, mappedAddress) = mapper.PpuWrite(address);
            if (success)
                characterMemory[mappedAddress] = value;

            return success;
        }
    }
}