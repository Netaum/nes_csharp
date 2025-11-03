using Emulator.Components.AddressingModes;
using Emulator.Components.Enums;
using Emulator.Components.Interfaces;

namespace Emulator.Components.OpCodes.Implementations
{
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