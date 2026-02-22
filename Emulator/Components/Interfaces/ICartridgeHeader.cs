namespace Emulator.Components.Interfaces
{
    public interface ICartridgeHeader
    {
        string Name { get; }
        int programRoomChunks { get; }
        int characterRoomChunks { get; }
        int MapperType1 { get; }
        int MapperType2 { get; }

        int ProgramRamSize { get; }
        int TVSystem1 { get; }
        int TVSystem2 { get; }
    }
}