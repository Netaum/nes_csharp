using Interfaces;

namespace Components
{
    public class CartridgeHeader: ICartridgeHeader
    {
        public string Name { get; set; }
        public int programRoomChunks { get; set; }
        public int characterRoomChunks { get; set; }
        public int MapperType1 { get; set; }
        public int MapperType2 { get; set; }
        public int ProgramRamSize { get; set; }
        public int TVSystem1 { get; set; }
        public int TVSystem2 { get; set; }

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
}