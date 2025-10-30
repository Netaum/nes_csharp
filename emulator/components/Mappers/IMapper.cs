namespace emulator.components.Mappers
{
    public interface IMapper
    {
        (bool, int) CpuRead(int address);
        (bool, int) CpuWrite(int address);
        (bool, int) PpuRead(int address);
        (bool, int) PpuWrite(int address);
    }
}