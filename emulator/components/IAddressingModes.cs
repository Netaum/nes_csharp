namespace emulator.components
{
    public interface IAddressingModes
    {
        int Absolute();
        int AbsoluteX();
        int AbsoluteY();
        int Immediate();
        int Implied();
        int Indirect();
        int IndirectX();
        int IndirectY();
        int Relative();
        int ZeroPage();
        int ZeroPageX();
        int ZeroPageY();
    }
}