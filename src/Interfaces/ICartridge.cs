namespace Interfaces
{
     public interface ICartridge
    {
        (bool, byte) CpuRead(int address);
        bool CpuWrite(int address, byte value);
        (bool, byte) PpuRead(int address);
        bool PpuWrite(int address, byte value);
    }
}