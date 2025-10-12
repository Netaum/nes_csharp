using emulator.components.Interfaces;

namespace emulator.components
{
    public static class Modes
    {
        public static readonly IAddressingMode Implied = new ImpliedAddressingMode();
        public static readonly IAddressingMode Immediate = new ImmediateAddressingMode();
        public static readonly IAddressingMode ZeroPage = new ZeroPageAddressingMode();
        public static readonly IAddressingMode ZeroPageX = new ZeroPageXAddressingMode();
        public static readonly IAddressingMode ZeroPageY = new ZeroPageYAddressingMode();
        public static readonly IAddressingMode Relative = new RelativeAddressingMode();
        public static readonly IAddressingMode Absolute = new AbsoluteAddressingMode();
        public static readonly IAddressingMode AbsoluteX = new AbsoluteXAddressingMode();
        public static readonly IAddressingMode AbsoluteY = new AbsoluteYAddressingMode();
        public static readonly IAddressingMode Indirect = new IndirectAddressingMode();
        public static readonly IAddressingMode IndirectX = new IndirectXAddressingMode();
        public static readonly IAddressingMode IndirectY = new IndirectYAddressingMode();
    }

    public abstract class AddressingModeBase : IAddressingMode
    {
        public abstract string Name { get; }
        public abstract int Execute(ICpu cpu);

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object? obj)
        {
            if (obj is AddressingModeBase mode)
            {
                return mode.Name == Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
    public class AbsoluteAddressingMode : AddressingModeBase
    {
        public override string Name => "ABS";

        public override int Execute(ICpu cpu)
        {
            int lowByte = cpu.ReadMemory();
            cpu.StepProgramCounter();
            int highByte = cpu.ReadMemory();
            cpu.StepProgramCounter();

            int address = (highByte << 8) | lowByte;

            cpu.AbsoluteAddress = address;
            return 0;
        }
    }

    public class AbsoluteXAddressingMode : AddressingModeBase
    {
        public override string Name => "ABX";

        public override int Execute(ICpu cpu)
        {
            int lowByte = cpu.ReadMemory();
            cpu.StepProgramCounter();
            int highByte = cpu.ReadMemory();
            cpu.StepProgramCounter();

            int address = (highByte << 8) | lowByte;
            address += cpu.XRegister;

            cpu.AbsoluteAddress = address;

            if ((cpu.AbsoluteAddress & 0xFF00) != (highByte << 8))
            {
                return 1;
            }

            return 0;
        }
    }

    public class AbsoluteYAddressingMode : AddressingModeBase
    {
        public override string Name => "ABY";

        public override int Execute(ICpu cpu)
        {
            int lowByte = cpu.ReadMemory();
            cpu.StepProgramCounter();
            int highByte = cpu.ReadMemory();
            cpu.StepProgramCounter();

            int address = (highByte << 8) | lowByte;
            address += cpu.YRegister;

            cpu.AbsoluteAddress = address;

            if ((cpu.AbsoluteAddress & 0xFF00) != (highByte << 8))
            {
                return 1;
            }

            return 0;
        }
    }

    public class ImmediateAddressingMode : AddressingModeBase
    {
        public override string Name => "IMM";

        public override int Execute(ICpu cpu)
        {
            cpu.AbsoluteAddress = cpu.ProgramCounter;
            cpu.StepProgramCounter();
            return 0;
        }
    }

    public class ImpliedAddressingMode : AddressingModeBase
    {
        public override string Name => "IMP";

        public override int Execute(ICpu cpu)
        {
            cpu.FetchedValue = cpu.AccumulatorRegister;
            return 0;
        }
    }

    public class IndirectAddressingMode : AddressingModeBase
    {
        public override string Name => "IND";

        public override int Execute(ICpu cpu)
        {
            int ptrLow = cpu.ReadMemory();
            cpu.StepProgramCounter();
            int ptrHigh = cpu.ReadMemory();
            cpu.StepProgramCounter();
            int pointer = (ptrHigh << 8) | ptrLow;
            
            int lowByte = cpu.ReadMemory(pointer);
            int highByte = ptrLow == 0x00FF ?
                                cpu.ReadMemory(pointer & 0xFF00) : 
                                cpu.ReadMemory(pointer + 1);

            cpu.AbsoluteAddress = (highByte << 8) | lowByte;

            return 0;
        }
    }

    public class IndirectXAddressingMode : AddressingModeBase
    {
        public override string Name => "IZX";

        public override int Execute(ICpu cpu)
        {
            int pointer = cpu.ReadMemory();
            cpu.StepProgramCounter();
            int lowByte = cpu.ReadMemory((pointer + cpu.XRegister) & 0x00FF);
            int highByte = cpu.ReadMemory((pointer + cpu.XRegister + 1) & 0x00FF);
            cpu.AbsoluteAddress = (highByte << 8) | lowByte;
            return 0;
        }
    }

    public class IndirectYAddressingMode : AddressingModeBase
    {
        public override string Name => "IZY";

        public override int Execute(ICpu cpu)
        {
            int pointer = cpu.ReadMemory();
            cpu.StepProgramCounter();
            int lowByte = cpu.ReadMemory(pointer & 0x00FF);
            int highByte = cpu.ReadMemory((pointer + 1) & 0x00FF);
            int address = (highByte << 8) | lowByte;
            address += cpu.YRegister;
            cpu.AbsoluteAddress = address;

            if ((cpu.AbsoluteAddress & 0xFF00) != (highByte << 8))
            {
                return 1;
            }

            return 0;
        }
    }

    public class RelativeAddressingMode : AddressingModeBase
    {
        public override string Name => "REL";

        public override int Execute(ICpu cpu)
        {
            int offset = cpu.ReadMemory();
            cpu.StepProgramCounter();

            if (offset >= 0x80)
            {
                offset -= 0x100;
            }

            cpu.RelativeAddress = offset;
            return 0;
        }
    }

    public class ZeroPageAddressingMode : AddressingModeBase
    {
        public override string Name => "ZP0";

        public override int Execute(ICpu cpu)
        {
            int address = cpu.ReadMemory();
            cpu.StepProgramCounter();
            cpu.AbsoluteAddress = address & 0x00FF;
            return 0;
        }
    }

    public class ZeroPageXAddressingMode : AddressingModeBase
    {
        public override string Name => "ZPX";

        public override int Execute(ICpu cpu)
        {
            int address = cpu.ReadMemory();
            cpu.StepProgramCounter();
            address += cpu.XRegister;
            cpu.AbsoluteAddress = address & 0x00FF;
            return 0;
        }
    }

    public class ZeroPageYAddressingMode : AddressingModeBase
    {
        public override string Name => "ZPY";

        public override int Execute(ICpu cpu)
        {
            int address = cpu.ReadMemory();
            cpu.StepProgramCounter();
            address += cpu.YRegister;
            cpu.AbsoluteAddress = address & 0x00FF;
            return 0;
        }
    }
}