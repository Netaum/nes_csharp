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

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.C);

            if (flag)
                return 0;

            cpu.IncCycles();

            int absoluteAddress = cpu.ProgramCounter + cpu.GetRelativeAddress();

            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.IncCycles();
            }

            cpu.SetProgramCounter(absoluteAddress);
            return 0;
        }
    }

    public class BCS : OpCodeBase
    {
        public override string Name => "BCS";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.C);

            if (!flag)
                return 0;

            cpu.IncCycles();
            int absoluteAddress = cpu.ProgramCounter + cpu.GetRelativeAddress();

            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.IncCycles();
            }

            cpu.SetProgramCounter(absoluteAddress);
            return 0;
        }
    }

    public class BEQ : OpCodeBase
    {
        public override string Name => "BEQ";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.Z);

            if (!flag)
                return 0;

            cpu.IncCycles();
            int absoluteAddress = cpu.ProgramCounter + cpu.GetRelativeAddress();

            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.IncCycles();
            }

            cpu.SetProgramCounter(absoluteAddress);
            return 0;
        }
    }

    public class BIT : OpCodeBase
    {
        public override string Name => "BIT";

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

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.N);

            if (!flag)
                return 0;

            cpu.IncCycles();
            int absoluteAddress = cpu.ProgramCounter + cpu.GetRelativeAddress();

            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.IncCycles();
            }

            cpu.SetProgramCounter(absoluteAddress);
            return 0;
        }
    }

    public class BNE : OpCodeBase
    {
        public override string Name => "BNE";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.Z);

            if (flag)
                return 0;

            cpu.IncCycles();
            int absoluteAddress = cpu.ProgramCounter + cpu.GetRelativeAddress();

            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.IncCycles();
            }

            cpu.SetProgramCounter(absoluteAddress);
            return 0;
        }
    }

    public class BPL : OpCodeBase
    {
        public override string Name => "BPL";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.N);

            if (flag)
                return 0;

            cpu.IncCycles();
            int absoluteAddress = cpu.ProgramCounter + cpu.GetRelativeAddress();

            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.IncCycles();
            }

            cpu.SetProgramCounter(absoluteAddress);
            return 0;
        }
    }

    public class BRK : OpCodeBase
    {
        public override string Name => "BRK";

        public override int Execute(ICpu cpu)
        {
            cpu.StepProgramCounter();
            cpu.WriteMemory(0x0100 + cpu.StackPointer, (cpu.ProgramCounter >> 8) & 0x00FF);
            cpu.StepProgramCounter(-1);
            cpu.WriteMemory(0x0100 + cpu.StackPointer, cpu.ProgramCounter & 0x00FF);
            cpu.StepProgramCounter(-1);
            cpu.SetStatusFlag(Flags6502.B, true);
            cpu.WriteMemory(0x0100 + cpu.StackPointer, cpu.Status);
            cpu.StepProgramCounter(-1);
            cpu.SetStatusFlag(Flags6502.B, false);
            int high = cpu.ReadMemory(0xFFFE);
            int low = cpu.ReadMemory(0xFFFF);
            cpu.SetProgramCounter(high | (low << 8));
            return 0;
        }
    }

    public class BVC : OpCodeBase
    {
        public override string Name => "BVC";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.V);

            if (flag)
                return 0;

            cpu.IncCycles();
            int absoluteAddress = cpu.ProgramCounter + cpu.GetRelativeAddress();

            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.IncCycles();
            }

            cpu.SetProgramCounter(absoluteAddress);
            return 0;
        }
    }

    public class BVS : OpCodeBase
    {
        public override string Name => "BVS";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.V);

            if (!flag)
                return 0;

            cpu.IncCycles();
            int absoluteAddress = cpu.ProgramCounter + cpu.GetRelativeAddress();

            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.IncCycles();
            }

            cpu.SetProgramCounter(absoluteAddress);
            return 0;
        }
    }

    public class CLC : OpCodeBase
    {
        public override string Name => "CLC";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.C, false);
            return 0;
        }
    }

    public class CLD : OpCodeBase
    {
        public override string Name => "CLD";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.D, false);
            return 0;
        }
    }

    public class CLI : OpCodeBase
    {
        public override string Name => "CLI";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.I, false);
            return 0;
        }
    }

    public class CLV : OpCodeBase
    {
        public override string Name => "CLV";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.V, false);
            return 0;
        }
    }

    public class CMP : OpCodeBase
    {
        public override string Name => "CMP";

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

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.Z, (cpu.XRegister - 1 & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, ((cpu.XRegister - 1) & 0x80) == 0x80);
            cpu.XRegister = cpu.XRegister - 1;
            return 0;
        }
    }

    public class DEY : OpCodeBase
    {
        public override string Name => "DEY";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.Z, (cpu.YRegister - 1 & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, ((cpu.YRegister - 1) & 0x80) == 0x80);
            cpu.YRegister = cpu.YRegister - 1;
            return 0;
        }
    }

    public class EOR : OpCodeBase
    {
        public override string Name => "EOR";

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

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.Z, cpu.XRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.XRegister & 0x80) == 0x80);
            cpu.XRegister = cpu.XRegister + 1;
            return 0;
        }
    }

    public class INY : OpCodeBase
    {
        public override string Name => "INY";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.Z, cpu.YRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.YRegister & 0x80) == 0x80);
            cpu.YRegister = cpu.YRegister + 1;
            return 0;
        }
    }

    public class JMP : OpCodeBase
    {
        public override string Name => "JMP";

        public override int Execute(ICpu cpu)
        {
            cpu.SetProgramCounter(cpu.AbsoluteAddress);
            return 0;
        }
    }

    public class JSR : OpCodeBase
    {
        public override string Name => "JSR";

        public override int Execute(ICpu cpu)
        {
            cpu.StepProgramCounter(-1);
            cpu.WriteMemory(0x0100 + cpu.StackPointer, (cpu.ProgramCounter >> 8) & 0x00FF);
            cpu.StepProgramCounter(-1);
            cpu.WriteMemory(0x0100 + cpu.StackPointer, cpu.ProgramCounter & 0x00FF);
            cpu.StepProgramCounter(-1);
            cpu.SetProgramCounter(cpu.AbsoluteAddress);
            return 0;
        }
    }

    public class LDA : OpCodeBase
    {
        public override string Name => "LDA";

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

        public override int Execute(ICpu cpu)
        {
            cpu.WriteMemory(0x0100 + cpu.StackPointer, cpu.AccumulatorRegister);
            cpu.PopStackPointer();
            return 0;
        }
    }

    public class PHP : OpCodeBase
    {
        public override string Name => "PHP";

        public override int Execute(ICpu cpu)
        {
            cpu.WriteMemory(0x0100 + cpu.StackPointer, cpu.Status | (int)Flags6502.B | (int)Flags6502.U);
            cpu.SetStatusFlag(Flags6502.B, false);
            cpu.SetStatusFlag(Flags6502.U, false);
            cpu.PopStackPointer();
            return 0;
        }
    }

    public class PLA : OpCodeBase
    {
        public override string Name => "PLA";

        public override int Execute(ICpu cpu)
        {
            cpu.PushStackPointer();
            cpu.AccumulatorRegister = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 0;
        }
    }

    public class PLP : OpCodeBase
    {
        public override string Name => "PLP";

        public override int Execute(ICpu cpu)
        {
            cpu.PushStackPointer();
            cpu.Status = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.SetStatusFlag(Flags6502.U, true);
            return 0;
        }
    }

    public class ROL : OpCodeBase
    {
        public override string Name => "ROL";

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

        public override int Execute(ICpu cpu)
        {
            cpu.PushStackPointer();
            cpu.Status = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.Status &= ~(int)Flags6502.B;
            cpu.Status &= ~(int)Flags6502.U;

            cpu.PushStackPointer();
            int temp = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.SetProgramCounter(temp);
            cpu.PushStackPointer();
            temp = cpu.ProgramCounter | (cpu.ReadMemory(0x0100 + cpu.StackPointer) << 8);
            cpu.SetProgramCounter(temp);
            return 0;
        }
    }

    public class RTS : OpCodeBase
    {
        public override string Name => "RTS";

        public override int Execute(ICpu cpu)
        {
            cpu.PushStackPointer();
            int temp = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.SetProgramCounter(temp);
            cpu.PushStackPointer();
            temp = cpu.ProgramCounter | (cpu.ReadMemory(0x0100 + cpu.StackPointer) << 8);
            temp += 1;
            cpu.SetProgramCounter(temp);
            return 0;
        }
    }

    public class SEC : OpCodeBase
    {
        public override string Name => "SEC";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.C, true);
            return 0;
        }
    }

    public class SED : OpCodeBase
    {
        public override string Name => "SED";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.D, true);
            return 0;
        }
    }

    public class SEI : OpCodeBase
    {
        public override string Name => "SEI";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.I, true);
            return 0;
        }
    }

    public class STA : OpCodeBase
    {
        public override string Name => "STA";

        public override int Execute(ICpu cpu)
        {
            cpu.WriteMemory(cpu.AbsoluteAddress, cpu.AccumulatorRegister);
            return 0;
        }
    }

    public class STX : OpCodeBase
    {
        public override string Name => "STX";

        public override int Execute(ICpu cpu)
        {
            cpu.WriteMemory(cpu.AbsoluteAddress, cpu.XRegister);
            return 0;
        }
    }

    public class STY : OpCodeBase
    {
        public override string Name => "STY";

        public override int Execute(ICpu cpu)
        {
            cpu.WriteMemory(cpu.AbsoluteAddress, cpu.YRegister);
            return 0;
        }
    }

    public class TAX : OpCodeBase
    {
        public override string Name => "TAX";

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

        public override int Execute(ICpu cpu)
        {
            cpu.SetStackPointer(cpu.XRegister);
            return 0;
        }
    }

    public class TYA : OpCodeBase
    {
        public override string Name => "TYA";

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

        public override int Execute(ICpu _cpu)
        {
            // This is an illegal opcode. It does nothing.
            return 0;
        }
    }
    
    public abstract class OpCodeBase : IOpCode
    {
        public abstract string Name { get; }

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