using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components
{
    public static class OpCodes
    {
        public static readonly IOpCode ADC = new ADC();
        public static readonly IOpCode AND = new AND();
        public static readonly IOpCode ASL = new ASL();
        public static readonly IOpCode BCC = new BCC();
        public static readonly IOpCode BCS = new BCS();
        public static readonly IOpCode BEQ = new BEQ();
        public static readonly IOpCode BIT = new BIT();
        public static readonly IOpCode BMI = new BMI();
        public static readonly IOpCode BNE = new BNE();
        public static readonly IOpCode BPL = new BPL();
        public static readonly IOpCode BRK = new BRK();
        public static readonly IOpCode BVC = new BVC();
        public static readonly IOpCode BVS = new BVS();
        public static readonly IOpCode CLC = new CLC();
        public static readonly IOpCode CLD = new CLD();
        public static readonly IOpCode CLI = new CLI();
        public static readonly IOpCode CLV = new CLV();
        public static readonly IOpCode CMP = new CMP();
        public static readonly IOpCode CPX = new CPX();
        public static readonly IOpCode CPY = new CPY();
        public static readonly IOpCode DEC = new DEC();
        public static readonly IOpCode DEX = new DEX();
        public static readonly IOpCode DEY = new DEY();
        public static readonly IOpCode EOR = new EOR();
        public static readonly IOpCode INC = new INC();
        public static readonly IOpCode INX = new INX();
        public static readonly IOpCode INY = new INY();
        public static readonly IOpCode JMP = new JMP();
        public static readonly IOpCode JSR = new JSR();
        public static readonly IOpCode LDA = new LDA();
        public static readonly IOpCode LDX = new LDX();
        public static readonly IOpCode LDY = new LDY();
        public static readonly IOpCode LSR = new LSR();
        public static readonly IOpCode NOP = new NOP();
        public static readonly IOpCode ORA = new ORA();
        public static readonly IOpCode PHA = new PHA();
        public static readonly IOpCode PHP = new PHP();
        public static readonly IOpCode PLA = new PLA();
        public static readonly IOpCode PLP = new PLP();
        public static readonly IOpCode ROL = new ROL();
        public static readonly IOpCode ROR = new ROR();
        public static readonly IOpCode SBC = new SBC();
        public static readonly IOpCode SEC = new SEC();
        public static readonly IOpCode SED = new SED();
        public static readonly IOpCode SEI = new SEI();
        public static readonly IOpCode STA = new STA();
        public static readonly IOpCode STX = new STX();
        public static readonly IOpCode STY = new STY();
        public static readonly IOpCode TAX = new TAX();
        public static readonly IOpCode TAY = new TAY();
        public static readonly IOpCode TSX = new TSX();
        public static readonly IOpCode TXA = new TXA();
        public static readonly IOpCode TXS = new TXS();
        public static readonly IOpCode TYA = new TYA();
        public static readonly IOpCode RTI = new RTI();
        public static readonly IOpCode RTS = new RTS();
        public static readonly IOpCode XXX = new XXX();
    }
    
    public class SBC : OpCodeBase
    {
        public override string Name => "SBC";
        public override string Description => "Subtract with Carry - Subtracts the value at the memory location from the accumulator with carry";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int value = fetchedValue ^ 0x00FF;
            int temp = cpu.AccumulatorRegister + value + (cpu.GetStatusFlag(Flags6502.C) ? 1 : 0);
            cpu.SetStatusFlag(Flags6502.C, temp > 0xFF);
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            cpu.SetStatusFlag(Flags6502.V, (~(cpu.AccumulatorRegister ^ fetchedValue) & (cpu.AccumulatorRegister ^ temp) & 0x80) == 0x80);
            cpu.AccumulatorRegister = temp & 0x00FF;
            return 1;
        }
    }

    public class ADC : OpCodeBase
    {
        public override string Name => "ADC";
        public override string Description => "Add with Carry - Adds the value at the memory location to the accumulator with carry";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = cpu.AccumulatorRegister + fetchedValue + (cpu.GetStatusFlag(Flags6502.C) ? 1 : 0);
            cpu.SetStatusFlag(Flags6502.C, temp > 0xFF);
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            cpu.SetStatusFlag(Flags6502.V, (~(cpu.AccumulatorRegister ^ fetchedValue) & (cpu.AccumulatorRegister ^ temp) & 0x80) == 0x80);
            cpu.AccumulatorRegister = temp & 0x00FF;
            return 1;
        }
    }

    public class AND : OpCodeBase
    {
        public override string Name => "AND";
        public override string Description => "Logical AND - Performs a bitwise AND on the accumulator and the value at the memory location";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            cpu.AccumulatorRegister = cpu.AccumulatorRegister & fetchedValue;
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 1;
        }
    }

    public class ASL : OpCodeBase
    {
        public override string Name => "ASL";
        public override string Description => "Arithmetic Shift Left - Shifts all bits of the accumulator or memory location one bit left";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = fetchedValue << 1;
            cpu.SetStatusFlag(Flags6502.C, (temp & 0xFF00) != 0);
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);

            if (cpu.CurrentInstruction.AddressingMode == Modes.Implied)
            {
                cpu.AccumulatorRegister = temp & 0x00FF;
            }
            else
            {
                cpu.WriteMemory(cpu.AbsoluteAddress, temp & 0x00FF);
            }

            return 0;
        }
    }

    public class BCC : OpCodeBase
    {
        public override string Name => "BCC";
        public override string Description => "Branch if Carry Clear - Branch if the carry flag is clear (C = 0)";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.C);

            if (flag)
                return 0;

            cpu.Cycles++;

            int absoluteAddress = cpu.ProgramCounter + cpu.RelativeAddress;
            cpu.AbsoluteAddress =  absoluteAddress;
            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.Cycles++;
            }

            cpu.ProgramCounter = absoluteAddress;
            return 0;
        }
    }

    public class BCS : OpCodeBase
    {
        public override string Name => "BCS";
        public override string Description => "Branch if Carry Set - Branch if the carry flag is set (C = 1)";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.C);

            if (!flag)
                return 0;

            cpu.Cycles++;
            int absoluteAddress = cpu.ProgramCounter + cpu.RelativeAddress;
            cpu.AbsoluteAddress =  absoluteAddress;
            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.Cycles++;
            }

            cpu.ProgramCounter = absoluteAddress;
            return 0;
        }
    }

    public class BEQ : OpCodeBase
    {
        public override string Name => "BEQ";
        public override string Description => "Branch if Equal - Branch if the zero flag is set (Z = 1)";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.Z);

            if (!flag)
                return 0;

            cpu.Cycles++;
            int absoluteAddress = cpu.ProgramCounter + cpu.RelativeAddress;
            cpu.AbsoluteAddress =  absoluteAddress;
            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.Cycles++;
            }

            cpu.ProgramCounter = absoluteAddress;
            return 0;
        }
    }

    public class BIT : OpCodeBase
    {
        public override string Name => "BIT";
        public override string Description => "Bit Test - Tests if one or more bits are set in a target memory location";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = cpu.AccumulatorRegister & fetchedValue;
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (fetchedValue & (1 << 7)) != 0);
            cpu.SetStatusFlag(Flags6502.V, (fetchedValue & (1 << 6)) != 0);
            return 0;
        }
    }

    public class BMI : OpCodeBase
    {
        public override string Name => "BMI";
        public override string Description => "Branch if Minus - Branch if the negative flag is set (N = 1)";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.N);

            if (!flag)
                return 0;

            cpu.Cycles++;
            int absoluteAddress = cpu.ProgramCounter + cpu.RelativeAddress;
            cpu.AbsoluteAddress =  absoluteAddress;
            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.Cycles++;
            }

            cpu.ProgramCounter = absoluteAddress;
            return 0;
        }
    }

    public class BNE : OpCodeBase
    {
        public override string Name => "BNE";
        public override string Description => "Branch if Not Equal - Branch if the zero flag is clear (Z = 0)";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.Z);

            if (flag)
                return 0;

            cpu.Cycles++;
            int absoluteAddress = cpu.ProgramCounter + cpu.RelativeAddress;
            cpu.AbsoluteAddress =  absoluteAddress;
            
            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.Cycles++;
            }

            cpu.ProgramCounter = absoluteAddress;
            return 0;
        }
    }

    public class BPL : OpCodeBase
    {
        public override string Name => "BPL";
        public override string Description => "Branch if Positive - Branch if the negative flag is clear (N = 0)";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.N);

            if (flag)
                return 0;

            cpu.Cycles++;
            int absoluteAddress = cpu.ProgramCounter + cpu.RelativeAddress;
            cpu.AbsoluteAddress =  absoluteAddress;
            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.Cycles++;
            }

            cpu.ProgramCounter = absoluteAddress;
            return 0;
        }
    }

    public class BRK : OpCodeBase
    {
        public override string Name => "BRK";
        public override string Description => "Force Break - Forces the generation of an interrupt request";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.I, true);
            cpu.WriteMemory(0x0100 + cpu.StackPointer, ((cpu.ProgramCounter + 1) >> 8) & 0x00FF);
            cpu.StackPointer--;
            cpu.WriteMemory(0x0100 + cpu.StackPointer, (cpu.ProgramCounter + 1) & 0x00FF);
            cpu.SetStatusFlag(Flags6502.B, true);
            cpu.Status = (int) (Flags6502.C | Flags6502.Z | Flags6502.I | Flags6502.D | Flags6502.B | Flags6502.U | Flags6502.V | Flags6502.N);
            cpu.StackPointer--;
            cpu.WriteMemory(0x0100 + cpu.StackPointer, cpu.Status);
            cpu.StackPointer--;
            cpu.SetStatusFlag(Flags6502.I, false);
            int low = cpu.ReadMemory(0xFFFE);
            int high = cpu.ReadMemory(0xFFFF);
            cpu.ProgramCounter = low | (high << 8);
            return 0;
        }
    }

    public class BVC : OpCodeBase
    {
        public override string Name => "BVC";
        public override string Description => "Branch if Overflow Clear - Branch if the overflow flag is clear (V = 0)";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.V);

            if (flag)
                return 0;

            cpu.Cycles++;
            int absoluteAddress = cpu.ProgramCounter + cpu.RelativeAddress;
            cpu.AbsoluteAddress =  absoluteAddress;
            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.Cycles++;
            }

            cpu.ProgramCounter = absoluteAddress;
            return 0;
        }
    }

    public class BVS : OpCodeBase
    {
        public override string Name => "BVS";
        public override string Description => "Branch if Overflow Set - Branch if the overflow flag is set (V = 1)";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.V);

            if (!flag)
                return 0;

            cpu.Cycles++;
            int absoluteAddress = cpu.ProgramCounter + cpu.RelativeAddress;
            cpu.AbsoluteAddress =  absoluteAddress;
            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.Cycles++;
            }

            cpu.ProgramCounter = absoluteAddress;
            return 0;
        }
    }

    public class CLC : OpCodeBase
    {
        public override string Name => "CLC";
        public override string Description => "Clear Carry Flag - Clears the carry flag to zero";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.C, false);
            return 0;
        }
    }

    public class CLD : OpCodeBase
    {
        public override string Name => "CLD";
        public override string Description => "Clear Decimal Mode - Clears the decimal mode flag to zero";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.D, false);
            return 0;
        }
    }

    public class CLI : OpCodeBase
    {
        public override string Name => "CLI";
        public override string Description => "Clear Interrupt Disable - Clears the interrupt disable flag to zero";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.I, false);
            return 0;
        }
    }

    public class CLV : OpCodeBase
    {
        public override string Name => "CLV";
        public override string Description => "Clear Overflow Flag - Clears the overflow flag to zero";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.V, false);
            return 0;
        }
    }

    public class CMP : OpCodeBase
    {
        public override string Name => "CMP";
        public override string Description => "Compare - Compares the contents of the accumulator with another memory value";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = cpu.AccumulatorRegister - fetchedValue;
            cpu.SetStatusFlag(Flags6502.C, cpu.AccumulatorRegister >= fetchedValue);
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            return 1;
        }
    }

    public class CPX : OpCodeBase
    {
        public override string Name => "CPX";
        public override string Description => "Compare X Register - Compares the contents of the X register with another memory value";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = cpu.XRegister - fetchedValue;
            cpu.SetStatusFlag(Flags6502.C, cpu.XRegister >= fetchedValue);
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            return 1;
        }
    }

    public class CPY : OpCodeBase
    {
        public override string Name => "CPY";
        public override string Description => "Compare Y Register - Compares the contents of the Y register with another memory value";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = cpu.YRegister - fetchedValue;
            cpu.SetStatusFlag(Flags6502.C, cpu.YRegister >= fetchedValue);
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            return 1;
        }
    }

    public class DEC : OpCodeBase
    {
        public override string Name => "DEC";
        public override string Description => "Decrement Memory - Subtracts one from the value held at a specified memory location";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = fetchedValue - 1;
            cpu.WriteMemory(cpu.AbsoluteAddress, temp & 0x00FF);
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            return 0;
        }
    }

    public class DEX : OpCodeBase
    {
        public override string Name => "DEX";
        public override string Description => "Decrement X Register - Subtracts one from the X register";

        public override int Execute(ICpu cpu)
        {
            cpu.XRegister = (cpu.XRegister - 1) & 0x00FF;
            cpu.SetStatusFlag(Flags6502.Z, cpu.XRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.XRegister & 0x80) == 0x80);
            return 0;
        }
    }

    public class DEY : OpCodeBase
    {
        public override string Name => "DEY";
        public override string Description => "Decrement Y Register - Subtracts one from the Y register";

        public override int Execute(ICpu cpu)
        {
            cpu.YRegister = (cpu.YRegister - 1) & 0x00FF;
            cpu.SetStatusFlag(Flags6502.Z, cpu.YRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.YRegister & 0x80) == 0x80);
            return 0;
        }
    }

    public class EOR : OpCodeBase
    {
        public override string Name => "EOR";
        public override string Description => "Exclusive OR - Performs a logical exclusive-OR with the accumulator";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            cpu.AccumulatorRegister = cpu.AccumulatorRegister ^ fetchedValue;
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 1;
        }
    }

    public class INC : OpCodeBase
    {
        public override string Name => "INC";
        public override string Description => "Increment Memory - Adds one to the value held at a specified memory location";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = fetchedValue + 1;
            cpu.WriteMemory(cpu.AbsoluteAddress, temp & 0x00FF);
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            return 0;
        }
    }

    public class INX : OpCodeBase
    {
        public override string Name => "INX";
        public override string Description => "Increment X Register - Adds one to the X register";

        public override int Execute(ICpu cpu)
        {
            cpu.XRegister = (cpu.XRegister + 1) & 0x00FF;
            cpu.SetStatusFlag(Flags6502.Z, cpu.XRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.XRegister & 0x80) == 0x80);
            return 0;
        }
    }

    public class INY : OpCodeBase
    {
        public override string Name => "INY";
        public override string Description => "Increment Y Register - Adds one to the Y register";

        public override int Execute(ICpu cpu)
        {
            cpu.YRegister = (cpu.YRegister + 1) & 0x00FF;
            cpu.SetStatusFlag(Flags6502.Z, cpu.YRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.YRegister & 0x80) == 0x80);
            return 0;
        }
    }

    public class JMP : OpCodeBase
    {
        public override string Name => "JMP";
        public override string Description => "Jump - Sets the program counter to the address specified by the operand";

        public override int Execute(ICpu cpu)
        {
            cpu.ProgramCounter = cpu.AbsoluteAddress;
            return 0;
        }
    }

    public class JSR : OpCodeBase
    {
        public override string Name => "JSR";
        public override string Description => "Jump to Subroutine - Pushes the return address onto the stack and jumps to the target address";

        public override int Execute(ICpu cpu)
        {
            cpu.ProgramCounter--;
            cpu.WriteMemory(0x0100 + cpu.StackPointer, (cpu.ProgramCounter >> 8) & 0x00FF);
            cpu.StackPointer--;
            cpu.WriteMemory(0x0100 + cpu.StackPointer, cpu.ProgramCounter & 0x00FF);
            cpu.StackPointer--;
            cpu.ProgramCounter = cpu.AbsoluteAddress;
            return 0;
        }
    }

    public class LDA : OpCodeBase
    {
        public override string Name => "LDA";
        public override string Description => "Load Accumulator - Loads a byte of memory into the accumulator";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            cpu.AccumulatorRegister = fetchedValue;
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 1;
        }
    }

    public class LDX : OpCodeBase
    {
        public override string Name => "LDX";
        public override string Description => "Load X Register - Loads a byte of memory into the X register";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            cpu.XRegister = fetchedValue;
            cpu.SetStatusFlag(Flags6502.Z, cpu.XRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.XRegister & 0x80) == 0x80);
            return 1;
        }
    }

    public class LDY : OpCodeBase
    {
        public override string Name => "LDY";
        public override string Description => "Load Y Register - Loads a byte of memory into the Y register";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            cpu.YRegister = fetchedValue;
            cpu.SetStatusFlag(Flags6502.Z, cpu.YRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.YRegister & 0x80) == 0x80);
            return 1;
        }
    }

    public class LSR : OpCodeBase
    {
        public override string Name => "LSR";
        public override string Description => "Logical Shift Right - Shifts all bits of the accumulator or memory location one bit right";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            cpu.SetStatusFlag(Flags6502.C, (fetchedValue & 0x01) == 0x01);
            int temp = fetchedValue >> 1;
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            if (cpu.CurrentInstruction.AddressingMode == Modes.Implied)
            {
                cpu.AccumulatorRegister = temp & 0x00FF;
            }
            else
            {
                cpu.WriteMemory(cpu.AbsoluteAddress, temp & 0x00FF);
            }

            return 0;
        }
    }

    public class NOP : OpCodeBase
    {
        public override string Name => "NOP";
        public override string Description => "No Operation - Causes no changes to the processor other than the normal incrementing of the program counter";

        public override int Execute(ICpu cpu)
        {
            switch (cpu.OpCode)
            {
                case 0x1C: // NOP (SLO)
                case 0x3C: // NOP (ANC)
                case 0x5C: // NOP (SLO)
                case 0x7C: // NOP (ANC)
                case 0xDC: // NOP (SLO)
                case 0xFC: // NOP (ANC)
                    return 1; // These are not true NOPs, they have side effects.
            }
            return 0;
        }
    }

    public class ORA : OpCodeBase
    {
        public override string Name => "ORA";
        public override string Description => "Logical Inclusive OR - Performs a logical OR with the accumulator";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            cpu.AccumulatorRegister = cpu.AccumulatorRegister | fetchedValue;
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 1;
        }
    }

    public class PHA : OpCodeBase
    {
        public override string Name => "PHA";
        public override string Description => "Push Accumulator - Pushes a copy of the accumulator on to the stack";

        public override int Execute(ICpu cpu)
        {
            cpu.WriteMemory(0x0100 + cpu.StackPointer, cpu.AccumulatorRegister);
            cpu.StackPointer--;
            return 0;
        }
    }

    public class PHP : OpCodeBase
    {
        public override string Name => "PHP";
        public override string Description => "Push Processor Status - Pushes a copy of the status flags on to the stack";

        public override int Execute(ICpu cpu)
        {
            cpu.WriteMemory(0x0100 + cpu.StackPointer, cpu.Status | (int)Flags6502.B | (int)Flags6502.U);
            cpu.SetStatusFlag(Flags6502.B, false);
            cpu.SetStatusFlag(Flags6502.U, false);
            cpu.StackPointer--;
            return 0;
        }
    }

    public class PLA : OpCodeBase
    {
        public override string Name => "PLA";
        public override string Description => "Pull Accumulator - Pulls an 8-bit value from the stack and into the accumulator";

        public override int Execute(ICpu cpu)
        {
            cpu.StackPointer++;
            cpu.AccumulatorRegister = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 0;
        }
    }

    public class PLP : OpCodeBase
    {
        public override string Name => "PLP";
        public override string Description => "Pull Processor Status - Pulls an 8-bit value from the stack and into the processor flags";

        public override int Execute(ICpu cpu)
        {
            cpu.StackPointer++;
            cpu.Status = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.SetStatusFlag(Flags6502.U, true);
            return 0;
        }
    }

    public class ROL : OpCodeBase
    {
        public override string Name => "ROL";
        public override string Description => "Rotate Left - Rotates all bits of the accumulator or memory location one bit left";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = (fetchedValue << 1) | (cpu.GetStatusFlag(Flags6502.C) ? 1 : 0);
            cpu.SetStatusFlag(Flags6502.C, (temp & 0xFF00) != 0);
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);

            if (cpu.CurrentInstruction.AddressingMode == Modes.Implied)
            {
                cpu.AccumulatorRegister = temp & 0x00FF;
            }
            else
            {
                cpu.WriteMemory(cpu.AbsoluteAddress, temp & 0x00FF);
            }

            return 0;
        }
    }

    public class ROR : OpCodeBase
    {
        public override string Name => "ROR";
        public override string Description => "Rotate Right - Rotates all bits of the accumulator or memory location one bit right";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = ((cpu.GetStatusFlag(Flags6502.C) ? 1 : 0) << 7) | (fetchedValue >> 1);
            cpu.SetStatusFlag(Flags6502.C, (fetchedValue & 0x01) == 0x01);
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);

            if (cpu.CurrentInstruction.AddressingMode == Modes.Implied)
            {
                cpu.AccumulatorRegister = temp & 0x00FF;
            }
            else
            {
                cpu.WriteMemory(cpu.AbsoluteAddress, temp & 0x00FF);
            }

            return 0;
        }
    }

    public class RTI : OpCodeBase
    {
        public override string Name => "RTI";
        public override string Description => "Return from Interrupt - Returns from an interrupt by pulling the processor flags and program counter from the stack";

        public override int Execute(ICpu cpu)
        {
            cpu.StackPointer++;
            cpu.Status = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.Status &= ~(int)Flags6502.B;
            cpu.Status &= ~(int)Flags6502.U;

            cpu.StackPointer++;
            int low = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.StackPointer++;
            int high = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.ProgramCounter = low | (high << 8);
            return 0;
        }
    }

    public class RTS : OpCodeBase
    {
        public override string Name => "RTS";
        public override string Description => "Return from Subroutine - Returns from a subroutine by pulling the program counter from the stack";

        public override int Execute(ICpu cpu)
        {
            cpu.StackPointer++;
            int low = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.StackPointer++;
            int high = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.ProgramCounter = low | (high << 8);
            cpu.ProgramCounter++;
            return 0;
        }
    }

    public class SEC : OpCodeBase
    {
        public override string Name => "SEC";
        public override string Description => "Set Carry Flag - Sets the carry flag to one";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.C, true);
            return 0;
        }
    }

    public class SED : OpCodeBase
    {
        public override string Name => "SED";
        public override string Description => "Set Decimal Flag - Sets the decimal mode flag to one";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.D, true);
            return 0;
        }
    }

    public class SEI : OpCodeBase
    {
        public override string Name => "SEI";
        public override string Description => "Set Interrupt Disable - Sets the interrupt disable flag to one";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.I, true);
            return 0;
        }
    }

    public class STA : OpCodeBase
    {
        public override string Name => "STA";
        public override string Description => "Store Accumulator - Stores the contents of the accumulator into memory";

        public override int Execute(ICpu cpu)
        {
            cpu.WriteMemory(cpu.AbsoluteAddress, cpu.AccumulatorRegister);
            return 0;
        }
    }

    public class STX : OpCodeBase
    {
        public override string Name => "STX";
        public override string Description => "Store X Register - Stores the contents of the X register into memory";

        public override int Execute(ICpu cpu)
        {
            cpu.WriteMemory(cpu.AbsoluteAddress, cpu.XRegister);
            return 0;
        }
    }

    public class STY : OpCodeBase
    {
        public override string Name => "STY";
        public override string Description => "Store Y Register - Stores the contents of the Y register into memory";

        public override int Execute(ICpu cpu)
        {
            cpu.WriteMemory(cpu.AbsoluteAddress, cpu.YRegister);
            return 0;
        }
    }

    public class TAX : OpCodeBase
    {
        public override string Name => "TAX";
        public override string Description => "Transfer Accumulator to X - Copies the current contents of the accumulator into the X register";

        public override int Execute(ICpu cpu)
        {
            cpu.XRegister = cpu.AccumulatorRegister;
            cpu.SetStatusFlag(Flags6502.Z, cpu.XRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.XRegister & 0x80) == 0x80);
            return 0;
        }
    }

    public class TAY : OpCodeBase
    {
        public override string Name => "TAY";
        public override string Description => "Transfer Accumulator to Y - Copies the current contents of the accumulator into the Y register";

        public override int Execute(ICpu cpu)
        {
            cpu.YRegister = cpu.AccumulatorRegister;
            cpu.SetStatusFlag(Flags6502.Z, cpu.YRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.YRegister & 0x80) == 0x80);
            return 0;
        }
    }

    public class TSX : OpCodeBase
    {
        public override string Name => "TSX";
        public override string Description => "Transfer Stack Pointer to X - Copies the current contents of the stack pointer into the X register";

        public override int Execute(ICpu cpu)
        {
            cpu.XRegister = cpu.StackPointer;
            cpu.SetStatusFlag(Flags6502.Z, cpu.XRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.XRegister & 0x80) == 0x80);
            return 0;
        }
    }

    public class TXA : OpCodeBase
    {
        public override string Name => "TXA";
        public override string Description => "Transfer X to Accumulator - Copies the current contents of the X register into the accumulator";

        public override int Execute(ICpu cpu)
        {
            cpu.AccumulatorRegister = cpu.XRegister;
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 0;
        }
    }

    public class TXS : OpCodeBase
    {
        public override string Name => "TXS";
        public override string Description => "Transfer X to Stack Pointer - Copies the current contents of the X register into the stack pointer";

        public override int Execute(ICpu cpu)
        {
            cpu.StackPointer = cpu.XRegister;
            return 0;
        }
    }

    public class TYA : OpCodeBase
    {
        public override string Name => "TYA";
        public override string Description => "Transfer Y to Accumulator - Copies the current contents of the Y register into the accumulator";

        public override int Execute(ICpu cpu)
        {
            cpu.AccumulatorRegister = cpu.YRegister;
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 0;
        }
    }

    public class XXX : OpCodeBase
    {
        public override string Name => "XXX";
        public override string Description => "Illegal Opcode - This is an illegal or undefined opcode that performs no operation";

        public override int Execute(ICpu _cpu)
        {
            // This is an illegal opcode. It does nothing.
            return 0;
        }
    }
    
    public abstract class OpCodeBase : IOpCode
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        public abstract int Execute(ICpu cpu);

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object? obj)
        {
            if (obj is IOpCode other)
            {
                return Name == other.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}