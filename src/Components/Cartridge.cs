using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Interfaces;
using Mappers;

namespace Components
{
    public class Cartridge : ICartridge
    {
        private byte[] programMemory;
        private byte[] characterMemory;
        private IMapper mapper;

        private Cartridge(IMapper mapper, byte[] programMemory, byte[] characterMemory)
        {
            this.mapper = mapper;
            this.programMemory = programMemory;
            this.characterMemory = characterMemory;
        }

        public static ICartridge CreateCartridge(byte[] cartridgeData)
        {
            var headerData = cartridgeData.Take(16);
            var cartridgeHeader = CartridgeHeader.FromBytes(headerData.ToArray());

            int startData = (cartridgeHeader.MapperType1 & 0x04) > 0 ?
                            512 + 16 :
                            16;

            int mapperType = ((cartridgeHeader.MapperType2 >> 4) << 4) | (cartridgeHeader.MapperType1 >> 4);
            IMapper mapper;

            switch (mapperType)
            {
                case 0x00:
                    mapper = new Mapper000(cartridgeHeader.programRoomChunks, cartridgeHeader.characterRoomChunks);
                    break;
                default:
                    throw new NotImplementedException($"Mapper type {mapperType} not supported");
            }

            int fileType = 1;
            byte[] programMemory = Array.Empty<byte>();
            byte[] characterMemory = Array.Empty<byte>();

            if (fileType == 1)
            {
                const int PRG_ROM_UNIT = 16384;
                const int CHR_ROM_UNIT = 8192;

                int programBank = cartridgeHeader.programRoomChunks;
                int characterBank = cartridgeHeader.characterRoomChunks;
                programMemory = new byte[programBank * PRG_ROM_UNIT];
                characterMemory = new byte[characterBank * CHR_ROM_UNIT];

                Array.Copy(cartridgeData, startData, programMemory, 0, programMemory.Length);
                Array.Copy(cartridgeData, startData + programMemory.Length, characterMemory, 0, characterMemory.Length);
            }

            return new Cartridge(mapper, programMemory, characterMemory);
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