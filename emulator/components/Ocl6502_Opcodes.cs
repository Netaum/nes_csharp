namespace emulator.components
{
    public partial class Ocl6502 : IOpCodes
    {
        public int SBC()
        {
            Fetch();
            int value = fetchedValue ^ 0x00FF;
            int temp = accumulatorRegister + value + (GetStatusFlag(Flags6502.C) ? 1 : 0);
            SetStatusFlag(Flags6502.C, temp > 0xFF);
            SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            SetStatusFlag(Flags6502.V, (~(accumulatorRegister ^ fetchedValue) & (accumulatorRegister ^ temp) & 0x80) == 0x80);
            accumulatorRegister = temp & 0x00FF;
            return 1;
        }

        public int ADC()
        {
            Fetch();
            int temp = accumulatorRegister + fetchedValue + (GetStatusFlag(Flags6502.C) ? 1 : 0);
            SetStatusFlag(Flags6502.C, temp > 0xFF);
            SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            SetStatusFlag(Flags6502.V, (~(accumulatorRegister ^ fetchedValue) & (accumulatorRegister ^ temp) & 0x80) == 0x80);
            accumulatorRegister = temp & 0x00FF;
            return 1;
        }

        public int AND()
        {
            Fetch();
            accumulatorRegister &= fetchedValue;
            SetStatusFlag(Flags6502.Z, accumulatorRegister == 0x00);
            SetStatusFlag(Flags6502.N, (accumulatorRegister & 0x80) == 1);
            return 1;
        }

        public int ASL()
        {
            Fetch();
            int temp = fetchedValue << 1;
            SetStatusFlag(Flags6502.C, (temp & 0xFF00) != 0);
            SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);

            if (lookupInstructionsTable[opcode].AddressingMode == Modes.Implied)
            {
                accumulatorRegister = temp & 0x00FF;
            }
            else
            {
                WriteMemory(absoluteAddress, temp & 0x00FF);
            }

            return 0;
        }

        public int BCC()
        {
            bool flag = GetStatusFlag(Flags6502.C);

            if (flag)
                return 0;

            cycles++;
            absoluteAddress = programCounter + relativeAddress;

            if ((absoluteAddress & 0xFF00) != (programCounter & 0xFF00))
            {
                cycles++;
            }
            
            programCounter = absoluteAddress;
            return 0;
        }

        public int BCS()
        {
            bool flag = GetStatusFlag(Flags6502.C);

            if (!flag)
                return 0;

            cycles++;
            absoluteAddress = programCounter + relativeAddress;

            if ((absoluteAddress & 0xFF00) != (programCounter & 0xFF00))
            {
                cycles++;
            }
            
            programCounter = absoluteAddress;
            return 0;
        }

        public int BEQ()
        {
            bool flag = GetStatusFlag(Flags6502.Z);

            if (!flag)
                return 0;

            cycles++;
            absoluteAddress = programCounter + relativeAddress;

            if ((absoluteAddress & 0xFF00) != (programCounter & 0xFF00))
            {
                cycles++;
            }
            
            programCounter = absoluteAddress;
            return 0;
        }

        public int BIT()
        {
            Fetch();
            int temp = accumulatorRegister & fetchedValue;
            SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            SetStatusFlag(Flags6502.N, (fetchedValue & (1 << 7)) != 0);
            SetStatusFlag(Flags6502.V, (fetchedValue & (1 << 6)) != 0);
            return 0;
        }

        public int BMI()
        {
            bool flag = GetStatusFlag(Flags6502.N);

            if (!flag)
                return 0;

            cycles++;
            absoluteAddress = programCounter + relativeAddress;

            if ((absoluteAddress & 0xFF00) != (programCounter & 0xFF00))
            {
                cycles++;
            }
            
            programCounter = absoluteAddress;
            return 0;
        }

        public int BNE()
        {
            bool flag = GetStatusFlag(Flags6502.Z);

            if (flag)
                return 0;

            cycles++;
            absoluteAddress = programCounter + relativeAddress;

            if ((absoluteAddress & 0xFF00) != (programCounter & 0xFF00))
            {
                cycles++;
            }
            
            programCounter = absoluteAddress;
            return 0;
        }

        public int BPL()
        {
            bool flag = GetStatusFlag(Flags6502.N);

            if (flag)
                return 0;

            cycles++;
            absoluteAddress = programCounter + relativeAddress;

            if ((absoluteAddress & 0xFF00) != (programCounter & 0xFF00))
            {
                cycles++;
            }
            
            programCounter = absoluteAddress;
            return 0;
        }

        public int BRK()
        {
            programCounter++;
            WriteMemory(0x0100 + stackPointer, (programCounter >> 8) & 0x00FF);
            stackPointer--;
            WriteMemory(0x0100 + stackPointer, programCounter & 0x00FF);
            stackPointer--;
            SetStatusFlag(Flags6502.B, true);
            WriteMemory(0x0100 + stackPointer, statusRegister);
            stackPointer--;
            SetStatusFlag(Flags6502.B, false);
            programCounter = ReadMemory(0xFFFE) | (ReadMemory(0xFFFF) << 8);
            return 0;
        }

        public int BVC()
        {
            bool flag = GetStatusFlag(Flags6502.V);

            if (flag)
                return 0;

            cycles++;
            absoluteAddress = programCounter + relativeAddress;

            if ((absoluteAddress & 0xFF00) != (programCounter & 0xFF00))
            {
                cycles++;
            }
            
            programCounter = absoluteAddress;
            return 0;
        }

        public int BVS()
        {
            bool flag = GetStatusFlag(Flags6502.V);

            if (!flag)
                return 0;

            cycles++;
            absoluteAddress = programCounter + relativeAddress;

            if ((absoluteAddress & 0xFF00) != (programCounter & 0xFF00))
            {
                cycles++;
            }
            
            programCounter = absoluteAddress;
            return 0;
        }

        public int CLC()
        {
            SetStatusFlag(Flags6502.C, false);
            return 0;
        }

        public int CLD()
        {
            SetStatusFlag(Flags6502.D, false);
            return 0;
        }

        public int CLI()
        {
            SetStatusFlag(Flags6502.I, false);
            return 0;
        }

        public int CLV()
        {
            SetStatusFlag(Flags6502.V, false);
            return 0;
        }

        public int CMP()
        {
            Fetch();
            int temp = accumulatorRegister - fetchedValue;
            SetStatusFlag(Flags6502.C, accumulatorRegister >= fetchedValue);
            SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            return 1;
        }

        public int CPX()
        {
            Fetch();
            int temp = xRegister - fetchedValue;
            SetStatusFlag(Flags6502.C, xRegister >= fetchedValue);
            SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            return 0;
        }

        public int CPY()
        {
            Fetch();
            int temp = yRegister - fetchedValue;
            SetStatusFlag(Flags6502.C, yRegister >= fetchedValue);
            SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            return 0;
        }

        public int DEC()
        {
            Fetch();
            int temp = fetchedValue - 1;
            WriteMemory(absoluteAddress, temp & 0x00FF);
            SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            return 0;
        }

        public int DEX()
        {
            xRegister--;
            SetStatusFlag(Flags6502.Z, xRegister == 0x00);
            SetStatusFlag(Flags6502.N, (xRegister & 0x80) == 0x80);
            return 0;
        }

        public int DEY()
        {
            yRegister--;
            SetStatusFlag(Flags6502.Z, yRegister == 0x00);
            SetStatusFlag(Flags6502.N, (yRegister & 0x80) == 0x80);
            return 0;
        }

        public int EOR()
        {
            Fetch();
            accumulatorRegister ^= fetchedValue;
            SetStatusFlag(Flags6502.Z, accumulatorRegister == 0x00);
            SetStatusFlag(Flags6502.N, (accumulatorRegister & 0x80) == 0x80);
            return 1;
        }

        public int INC()
        {
            Fetch();
            int temp = fetchedValue + 1;
            WriteMemory(absoluteAddress, temp & 0x00FF);
            SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            return 0;
        }

        public int INX()
        {
            xRegister++;
            SetStatusFlag(Flags6502.Z, xRegister == 0x00);
            SetStatusFlag(Flags6502.N, (xRegister & 0x80) == 0x80);
            return 0;
        }

        public int INY()
        {
            yRegister++;
            SetStatusFlag(Flags6502.Z, yRegister == 0x00);
            SetStatusFlag(Flags6502.N, (yRegister & 0x80) == 0x80);
            return 0;
        }

        public int JMP()
        {
            programCounter = absoluteAddress;
            return 0;
        }

        public int JSR()
        {
            programCounter--;
            WriteMemory(0x0100 + stackPointer, (programCounter >> 8) & 0x00FF);
            stackPointer--;
            WriteMemory(0x0100 + stackPointer, programCounter & 0x00FF);
            stackPointer--;
            programCounter = absoluteAddress;
            return 0;
        }

        public int LDA()
        {
            Fetch();
            accumulatorRegister = fetchedValue;
            SetStatusFlag(Flags6502.Z, accumulatorRegister == 0x00);
            SetStatusFlag(Flags6502.N, (accumulatorRegister & 0x80) == 0x80);
            return 1;
        }

        public int LDX()
        {
            Fetch();
            xRegister = fetchedValue;
            SetStatusFlag(Flags6502.Z, xRegister == 0x00);
            SetStatusFlag(Flags6502.N, (xRegister & 0x80) == 0x80);
            return 1;
        }

        public int LDY()
        {
            Fetch();
            yRegister = fetchedValue;
            SetStatusFlag(Flags6502.Z, yRegister == 0x00);
            SetStatusFlag(Flags6502.N, (yRegister & 0x80) == 0x80);
            return 1;
        }

        public int LSR()
        {
            Fetch();
            SetStatusFlag(Flags6502.C, (fetchedValue & 0x01) == 0x01);
            int temp = fetchedValue >> 1;
            SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            if (lookupInstructionsTable[opcode].AddressingMode == Modes.Implied)
            {
                accumulatorRegister = temp & 0x00FF;
            }
            else
            {
                WriteMemory(absoluteAddress, temp & 0x00FF);
            }

            return 0;
        }

        public int NOP()
        {
            switch (opcode)
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

        public int ORA()
        {
            Fetch();
            accumulatorRegister |= fetchedValue;
            SetStatusFlag(Flags6502.Z, accumulatorRegister == 0x00);
            SetStatusFlag(Flags6502.N, (accumulatorRegister & 0x80) == 0x80);
            return 1;
        }

        public int PHA()
        {
            WriteMemory(0x0100 + stackPointer, accumulatorRegister);
            stackPointer--;
            return 0;
        }

        public int PHP()
        {
            WriteMemory(0x0100 + stackPointer, statusRegister | (int)Flags6502.B | (int)Flags6502.U);
            SetStatusFlag(Flags6502.B, false);
            SetStatusFlag(Flags6502.U, false);
            stackPointer--;
            return 0;
        }

        public int PLA()
        {
            stackPointer++;
            accumulatorRegister = ReadMemory(0x0100 + stackPointer);
            SetStatusFlag(Flags6502.Z, accumulatorRegister == 0x00);
            SetStatusFlag(Flags6502.N, (accumulatorRegister & 0x80) == 0x80);
            return 0;
        }

        public int PLP()
        {
            stackPointer++;
            statusRegister = ReadMemory(0x0100 + stackPointer);
            SetStatusFlag(Flags6502.U, true);
            return 0;
        }

        public int ROL()
        {
            Fetch();
            int temp = (fetchedValue << 1) | (GetStatusFlag(Flags6502.C) ? 1 : 0);
            SetStatusFlag(Flags6502.C, (temp & 0xFF00) != 0);
            SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);

            if (lookupInstructionsTable[opcode].AddressingMode == Modes.Implied)
            {
                accumulatorRegister = temp & 0x00FF;
            }
            else
            {
                WriteMemory(absoluteAddress, temp & 0x00FF);
            }

            return 0;
        }

        public int ROR()
        {
            Fetch();
            int temp = ((GetStatusFlag(Flags6502.C) ? 1 : 0) << 7) | (fetchedValue >> 1);
            SetStatusFlag(Flags6502.C, (fetchedValue & 0x01) == 0x01);
            SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);

            if (lookupInstructionsTable[opcode].AddressingMode == Modes.Implied)
            {
                accumulatorRegister = temp & 0x00FF;
            }
            else
            {
                WriteMemory(absoluteAddress, temp & 0x00FF);
            }
            return 0;
        }

        public int RTI()
        {
            stackPointer++;
            statusRegister = ReadMemory(0x0100 + stackPointer);
            statusRegister &= ~(int)Flags6502.B;
            statusRegister &= ~(int)Flags6502.U;

            stackPointer++;
            programCounter = ReadMemory(0x0100 + stackPointer);
            stackPointer++;
            programCounter |= ReadMemory(0x0100 + stackPointer) << 8;
            return 0;
        }

        public int RTS()
        {
            stackPointer++;
            programCounter = ReadMemory(0x0100 + stackPointer);
            stackPointer++;
            programCounter |= ReadMemory(0x0100 + stackPointer) << 8;
            programCounter++;
            return 0;
        }

        public int SEC()
        {
            SetStatusFlag(Flags6502.C, true);
            return 0;
        }

        public int SED()
        {
            SetStatusFlag(Flags6502.D, true);
            return 0;
        }

        public int SEI()
        {
            SetStatusFlag(Flags6502.I, true);
            return 0;
        }

        public int STA()
        {
            WriteMemory(absoluteAddress, accumulatorRegister);
            return 0;
        }

        public int STX()
        {
            WriteMemory(absoluteAddress, xRegister);
            return 0;
        }

        public int STY()
        {
            WriteMemory(absoluteAddress, yRegister);
            return 0;
        }

        public int TAX()
        {
            xRegister = accumulatorRegister;
            SetStatusFlag(Flags6502.Z, xRegister == 0x00);
            SetStatusFlag(Flags6502.N, (xRegister & 0x80) == 0x80);
            return 0;
        }

        public int TAY()
        {
            yRegister = accumulatorRegister;
            SetStatusFlag(Flags6502.Z, yRegister == 0x00);
            SetStatusFlag(Flags6502.N, (yRegister & 0x80) == 0x80);
            return 0;
        }

        public int TSX()
        {
            xRegister = stackPointer;
            SetStatusFlag(Flags6502.Z, xRegister == 0x00);
            SetStatusFlag(Flags6502.N, (xRegister & 0x80) == 0x80);
            return 0;
        }

        public int TXA()
        {
            accumulatorRegister = xRegister;
            SetStatusFlag(Flags6502.Z, accumulatorRegister == 0x00);
            SetStatusFlag(Flags6502.N, (accumulatorRegister & 0x80) == 0x80);
            return 0;
        }

        public int TXS()
        {
            stackPointer = xRegister;
            return 0;
        }

        public int TYA()
        {
            accumulatorRegister = yRegister;
            SetStatusFlag(Flags6502.Z, accumulatorRegister == 0x00);
            SetStatusFlag(Flags6502.N, (accumulatorRegister & 0x80) == 0x80);
            return 0;
        }

        public int XXX()
        {
            return 0;
        }
    }
}