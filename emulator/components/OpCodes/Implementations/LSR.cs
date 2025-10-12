using emulator.components.AddressingModes;
using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
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
            if (cpu.CurrentInstruction.AddressingMode == InstructionAddressingModes.Implied)
            {
                cpu.AccumulatorRegister = temp & 0x00FF;
            }
            else
            {
                cpu.WriteMemory(cpu.AbsoluteAddress, temp.ToByte());
            }

            return 0;
        }
    }
}