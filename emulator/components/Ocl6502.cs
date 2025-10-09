namespace emulator.components
{
    public interface ICpu
    {
        void ConnectBus(IBus bus);

        void Clock();
        void Reset();
        void Interrupt();
        void NonMaskableInterrupt();
        int ReadMemory(int address);
        int ReadMemory();

        void LoadProgram(byte[] program, int startAddress);

        Dictionary<int, string> Disassemble(int start, int stop);

        bool GetStatusFlag(Flags6502 flag);

        Instruction[] Instructions { get; }

        int Fetch();

        int ProgramCounter { get; }
        int A { get; }
        int X { get; }
        int Y { get; }
        int SP { get; }

        int AbsoluteAddress { get; }

        void StepProgramCounter(int offset);
        void StepProgramCounter();

        void SetAbsoluteAddress(int address);
        void SetRelativeAddress(int address);
        void SetFetchedValue(int value);
    }

    public enum Flags6502
    {
        C = 1 << 0, // Carry
        Z = 1 << 1, // Zero
        I = 1 << 2, // Interrupt Disable
        D = 1 << 3, // Decimal Mode
        B = 1 << 4, // Break Command
        U = 1 << 5, // Unused
        V = 1 << 6, // Overflow
        N = 1 << 7  // Negative
    }

    public partial class Ocl6502 : ICpu
    {
        private IBus? bus;

        private int fetchedValue = 0x00;
        private int absoluteAddress = 0x00;
        private int relativeAddress = 0x00;
        private int opcode = 0x00;
        private int cycles = 0;

        private readonly Instruction[] lookupInstructionsTable;

        private int accumulatorRegister;
        private int statusRegister;
        private int xRegister;
        private int yRegister;

        private int programCounter;
        private int stackPointer;

        public int ProgramCounter => programCounter;
        public int A => accumulatorRegister;
        public int X => xRegister;
        public int Y => yRegister;
        public int SP => stackPointer;

        public int AbsoluteAddress => absoluteAddress;

        public Ocl6502()
        {
            lookupInstructionsTable = BuildInstructionSet(this);
        }

        public Instruction[] Instructions => lookupInstructionsTable;

        private IBus Bus
        {
            get
            {
                if (bus == null)
                {
                    throw new InvalidOperationException("Bus is not connected.");
                }
                return bus;
            }
        }


        public void ConnectBus(IBus bus)
        {
            this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }


        public int ReadMemory(int address)
        {
            return Bus.Read(address);
        }

        public int ReadMemory()
        {
            return Bus.Read(programCounter);
        }

        public void WriteMemory(int address, int value, bool readOnly = false)
        {
            Bus.Write(address, value, readOnly);
        }

        public bool GetStatusFlag(Flags6502 flag)
        {
            return (statusRegister & (int)flag) > 0;
        }

        public void SetStatusFlag(Flags6502 flag, bool set)
        {
            if (set)
            {
                // Set the flag
                statusRegister |= (int)flag;
            }
            else
            {
                // Clear the flag
                statusRegister &= ~(int)flag;
            }
        }

        public void Clock()
        {
            if (cycles == 0)
            {
                opcode = ReadMemory(programCounter);
                programCounter++;
                Instruction instruction = lookupInstructionsTable[opcode];

                cycles = instruction.Cycles;
                int additionalCycle1 = instruction.AddressingMode.Execute(this);
                int additionalCycle2 = instruction.Operation.Invoke();

                cycles += additionalCycle1 & additionalCycle2;
            }

            cycles--;
        }

        public void Reset()
        {
            accumulatorRegister = 0x00;
            xRegister = 0x00;
            yRegister = 0x00;
            stackPointer = 0xFD;
            statusRegister = 0x00 | (int)Flags6502.U;
            absoluteAddress = 0xFFFC;
            int lowByte = ReadMemory(absoluteAddress);
            int highByte = ReadMemory(absoluteAddress + 1);
            programCounter = (highByte << 8) | lowByte;
            absoluteAddress = 0x0000;
            relativeAddress = 0x0000;
            fetchedValue = 0x00;
            cycles = 8;
        }

        public void Interrupt()
        {
            if (GetStatusFlag(Flags6502.I))
            {
                return;
            }

            WriteMemory(0x0100 + stackPointer, (programCounter >> 8) & 0x00FF);
            stackPointer--;
            WriteMemory(0x0100 + stackPointer, programCounter & 0x00FF);
            stackPointer--;

            SetStatusFlag(Flags6502.B, false);
            SetStatusFlag(Flags6502.I, true);
            SetStatusFlag(Flags6502.U, true);
            WriteMemory(0x0100 + stackPointer, statusRegister);
            stackPointer--;

            absoluteAddress = 0xFFFE;
            int lowByte = ReadMemory(absoluteAddress);
            int highByte = ReadMemory(absoluteAddress + 1);
            programCounter = (highByte << 8) | lowByte;

            cycles = 7;
        }

        public void NonMaskableInterrupt()
        {
            WriteMemory(0x0100 + stackPointer, (programCounter >> 8) & 0x00FF);
            stackPointer--;
            WriteMemory(0x0100 + stackPointer, programCounter & 0x00FF);
            stackPointer--;

            SetStatusFlag(Flags6502.B, false);
            SetStatusFlag(Flags6502.I, true);
            SetStatusFlag(Flags6502.U, true);
            WriteMemory(0x0100 + stackPointer, statusRegister);
            stackPointer--;

            absoluteAddress = 0xFFFA;
            int lowByte = ReadMemory(absoluteAddress);
            int highByte = ReadMemory(absoluteAddress + 1);
            programCounter = (highByte << 8) | lowByte;

            cycles = 8;
        }

        public int Fetch()
        {
            var instruction = lookupInstructionsTable[opcode];

            if (instruction.AddressingMode != Modes.Implied)
            {
                fetchedValue = ReadMemory(absoluteAddress);
            }

            return fetchedValue;
        }


        private static Instruction[] BuildInstructionSet(IOpCodes opCodes)
        {
            var instructions = new Instruction[]
            {

                new Instruction( "BRK", opCodes.BRK, Modes.Immediate, 7 ),new Instruction( "ORA", opCodes.ORA, Modes.IndirectX, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 3 ),new Instruction( "ORA", opCodes.ORA, Modes.ZeroPage, 3 ),new Instruction( "ASL", opCodes.ASL, Modes.ZeroPage, 5 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 5 ),new Instruction( "PHP", opCodes.PHP, Modes.Implied, 3 ),new Instruction( "ORA", opCodes.ORA, Modes.Immediate, 2 ),new Instruction( "ASL", opCodes.ASL, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 4 ),new Instruction( "ORA", opCodes.ORA, Modes.Absolute, 4 ),new Instruction( "ASL", opCodes.ASL, Modes.Absolute, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 6 ),
                new Instruction( "BPL", opCodes.BPL, Modes.Relative, 2 ),new Instruction( "ORA", opCodes.ORA, Modes.IndirectY, 5 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 4 ),new Instruction( "ORA", opCodes.ORA, Modes.ZeroPageX, 4 ),new Instruction( "ASL", opCodes.ASL, Modes.ZeroPageX, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 6 ),new Instruction( "CLC", opCodes.CLC, Modes.Implied, 2 ),new Instruction( "ORA", opCodes.ORA, Modes.AbsoluteY, 4 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 7 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 4 ),new Instruction( "ORA", opCodes.ORA, Modes.AbsoluteX, 4 ),new Instruction( "ASL", opCodes.ASL, Modes.AbsoluteX, 7 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 7 ),
                new Instruction( "JSR", opCodes.JSR, Modes.Absolute, 6 ),new Instruction( "AND", opCodes.AND, Modes.IndirectX, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 8 ),new Instruction( "BIT", opCodes.BIT, Modes.ZeroPage, 3 ),new Instruction( "AND", opCodes.AND, Modes.ZeroPage, 3 ),new Instruction( "ROL", opCodes.ROL, Modes.ZeroPage, 5 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 5 ),new Instruction( "PLP", opCodes.PLP, Modes.Implied, 4 ),new Instruction( "AND", opCodes.AND, Modes.Immediate, 2 ),new Instruction( "ROL", opCodes.ROL, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "BIT", opCodes.BIT, Modes.Absolute, 4 ),new Instruction( "AND", opCodes.AND, Modes.Absolute, 4 ),new Instruction( "ROL", opCodes.ROL, Modes.Absolute, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 6 ),
                new Instruction( "BMI", opCodes.BMI, Modes.Relative, 2 ),new Instruction( "AND", opCodes.AND, Modes.IndirectY, 5 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 4 ),new Instruction( "AND", opCodes.AND, Modes.ZeroPageX, 4 ),new Instruction( "ROL", opCodes.ROL, Modes.ZeroPageX, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 6 ),new Instruction( "SEC", opCodes.SEC, Modes.Implied, 2 ),new Instruction( "AND", opCodes.AND, Modes.AbsoluteY, 4 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 7 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 4 ),new Instruction( "AND", opCodes.AND, Modes.AbsoluteX, 4 ),new Instruction( "ROL", opCodes.ROL, Modes.AbsoluteX, 7 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 7 ),
                new Instruction( "RTI", opCodes.RTI, Modes.Implied, 6 ),new Instruction( "EOR", opCodes.EOR, Modes.IndirectX, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 3 ),new Instruction( "EOR", opCodes.EOR, Modes.ZeroPage, 3 ),new Instruction( "LSR", opCodes.LSR, Modes.ZeroPage, 5 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 5 ),new Instruction( "PHA", opCodes.PHA, Modes.Implied, 3 ),new Instruction( "EOR", opCodes.EOR, Modes.Immediate, 2 ),new Instruction( "LSR", opCodes.LSR, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "JMP", opCodes.JMP, Modes.Absolute, 3 ),new Instruction( "EOR", opCodes.EOR, Modes.Absolute, 4 ),new Instruction( "LSR", opCodes.LSR, Modes.Absolute, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 6 ),
                new Instruction( "BVC", opCodes.BVC, Modes.Relative, 2 ),new Instruction( "EOR", opCodes.EOR, Modes.IndirectY, 5 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 4 ),new Instruction( "EOR", opCodes.EOR, Modes.ZeroPageX, 4 ),new Instruction( "LSR", opCodes.LSR, Modes.ZeroPageX, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 6 ),new Instruction( "CLI", opCodes.CLI, Modes.Implied, 2 ),new Instruction( "EOR", opCodes.EOR, Modes.AbsoluteY, 4 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 7 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 4 ),new Instruction( "EOR", opCodes.EOR, Modes.AbsoluteX, 4 ),new Instruction( "LSR", opCodes.LSR, Modes.AbsoluteX, 7 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 7 ),
                new Instruction( "RTS", opCodes.RTS, Modes.Implied, 6 ),new Instruction( "ADC", opCodes.ADC, Modes.IndirectX, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 3 ),new Instruction( "ADC", opCodes.ADC, Modes.ZeroPage, 3 ),new Instruction( "ROR", opCodes.ROR, Modes.ZeroPage, 5 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 5 ),new Instruction( "PLA", opCodes.PLA, Modes.Implied, 4 ),new Instruction( "ADC", opCodes.ADC, Modes.Immediate, 2 ),new Instruction( "ROR", opCodes.ROR, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "JMP", opCodes.JMP, Modes.Indirect, 5 ),new Instruction( "ADC", opCodes.ADC, Modes.Absolute, 4 ),new Instruction( "ROR", opCodes.ROR, Modes.Absolute, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 6 ),
                new Instruction( "BVS", opCodes.BVS, Modes.Relative, 2 ),new Instruction( "ADC", opCodes.ADC, Modes.IndirectY, 5 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 4 ),new Instruction( "ADC", opCodes.ADC, Modes.ZeroPageX, 4 ),new Instruction( "ROR", opCodes.ROR, Modes.ZeroPageX, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 6 ),new Instruction( "SEI", opCodes.SEI, Modes.Implied, 2 ),new Instruction( "ADC", opCodes.ADC, Modes.AbsoluteY, 4 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 7 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 4 ),new Instruction( "ADC", opCodes.ADC, Modes.AbsoluteX, 4 ),new Instruction( "ROR", opCodes.ROR, Modes.AbsoluteX, 7 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 7 ),
                new Instruction( "???", opCodes.NOP, Modes.Implied, 2 ),new Instruction( "STA", opCodes.STA, Modes.IndirectX, 6 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 6 ),new Instruction( "STY", opCodes.STY, Modes.ZeroPage, 3 ),new Instruction( "STA", opCodes.STA, Modes.ZeroPage, 3 ),new Instruction( "STX", opCodes.STX, Modes.ZeroPage, 3 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 3 ),new Instruction( "DEY", opCodes.DEY, Modes.Implied, 2 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 2 ),new Instruction( "TXA", opCodes.TXA, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "STY", opCodes.STY, Modes.Absolute, 4 ),new Instruction( "STA", opCodes.STA, Modes.Absolute, 4 ),new Instruction( "STX", opCodes.STX, Modes.Absolute, 4 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 4 ),
                new Instruction( "BCC", opCodes.BCC, Modes.Relative, 2 ),new Instruction( "STA", opCodes.STA, Modes.IndirectY, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 6 ),new Instruction( "STY", opCodes.STY, Modes.ZeroPageX, 4 ),new Instruction( "STA", opCodes.STA, Modes.ZeroPageX, 4 ),new Instruction( "STX", opCodes.STX, Modes.ZeroPageY, 4 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 4 ),new Instruction( "TYA", opCodes.TYA, Modes.Implied, 2 ),new Instruction( "STA", opCodes.STA, Modes.AbsoluteY, 5 ),new Instruction( "TXS", opCodes.TXS, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 5 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 5 ),new Instruction( "STA", opCodes.STA, Modes.AbsoluteX, 5 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 5 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 5 ),
                new Instruction( "LDY", opCodes.LDY, Modes.Immediate, 2 ),new Instruction( "LDA", opCodes.LDA, Modes.IndirectX, 6 ),new Instruction( "LDX", opCodes.LDX, Modes.Immediate, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 6 ),new Instruction( "LDY", opCodes.LDY, Modes.ZeroPage, 3 ),new Instruction( "LDA", opCodes.LDA, Modes.ZeroPage, 3 ),new Instruction( "LDX", opCodes.LDX, Modes.ZeroPage, 3 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 3 ),new Instruction( "TAY", opCodes.TAY, Modes.Implied, 2 ),new Instruction( "LDA", opCodes.LDA, Modes.Immediate, 2 ),new Instruction( "TAX", opCodes.TAX, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "LDY", opCodes.LDY, Modes.Absolute, 4 ),new Instruction( "LDA", opCodes.LDA, Modes.Absolute, 4 ),new Instruction( "LDX", opCodes.LDX, Modes.Absolute, 4 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 4 ),
                new Instruction( "BCS", opCodes.BCS, Modes.Relative, 2 ),new Instruction( "LDA", opCodes.LDA, Modes.IndirectY, 5 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 5 ),new Instruction( "LDY", opCodes.LDY, Modes.ZeroPageX, 4 ),new Instruction( "LDA", opCodes.LDA, Modes.ZeroPageX, 4 ),new Instruction( "LDX", opCodes.LDX, Modes.ZeroPageY, 4 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 4 ),new Instruction( "CLV", opCodes.CLV, Modes.Implied, 2 ),new Instruction( "LDA", opCodes.LDA, Modes.AbsoluteY, 4 ),new Instruction( "TSX", opCodes.TSX, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 4 ),new Instruction( "LDY", opCodes.LDY, Modes.AbsoluteX, 4 ),new Instruction( "LDA", opCodes.LDA, Modes.AbsoluteX, 4 ),new Instruction( "LDX", opCodes.LDX, Modes.AbsoluteY, 4 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 4 ),
                new Instruction( "CPY", opCodes.CPY, Modes.Immediate, 2 ),new Instruction( "CMP", opCodes.CMP, Modes.IndirectX, 6 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 8 ),new Instruction( "CPY", opCodes.CPY, Modes.ZeroPage, 3 ),new Instruction( "CMP", opCodes.CMP, Modes.ZeroPage, 3 ),new Instruction( "DEC", opCodes.DEC, Modes.ZeroPage, 5 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 5 ),new Instruction( "INY", opCodes.INY, Modes.Implied, 2 ),new Instruction( "CMP", opCodes.CMP, Modes.Immediate, 2 ),new Instruction( "DEX", opCodes.DEX, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "CPY", opCodes.CPY, Modes.Absolute, 4 ),new Instruction( "CMP", opCodes.CMP, Modes.Absolute, 4 ),new Instruction( "DEC", opCodes.DEC, Modes.Absolute, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 6 ),
                new Instruction( "BNE", opCodes.BNE, Modes.Relative, 2 ),new Instruction( "CMP", opCodes.CMP, Modes.IndirectY, 5 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 4 ),new Instruction( "CMP", opCodes.CMP, Modes.ZeroPageX, 4 ),new Instruction( "DEC", opCodes.DEC, Modes.ZeroPageX, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 6 ),new Instruction( "CLD", opCodes.CLD, Modes.Implied, 2 ),new Instruction( "CMP", opCodes.CMP, Modes.AbsoluteY, 4 ),new Instruction( "NOP", opCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 7 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 4 ),new Instruction( "CMP", opCodes.CMP, Modes.AbsoluteX, 4 ),new Instruction( "DEC", opCodes.DEC, Modes.AbsoluteX, 7 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 7 ),
                new Instruction( "CPX", opCodes.CPX, Modes.Immediate, 2 ),new Instruction( "SBC", opCodes.SBC, Modes.IndirectX, 6 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 8 ),new Instruction( "CPX", opCodes.CPX, Modes.ZeroPage, 3 ),new Instruction( "SBC", opCodes.SBC, Modes.ZeroPage, 3 ),new Instruction( "INC", opCodes.INC, Modes.ZeroPage, 5 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 5 ),new Instruction( "INX", opCodes.INX, Modes.Implied, 2 ),new Instruction( "SBC", opCodes.SBC, Modes.Immediate, 2 ),new Instruction( "NOP", opCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", opCodes.SBC, Modes.Implied, 2 ),new Instruction( "CPX", opCodes.CPX, Modes.Absolute, 4 ),new Instruction( "SBC", opCodes.SBC, Modes.Absolute, 4 ),new Instruction( "INC", opCodes.INC, Modes.Absolute, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 6 ),
                new Instruction( "BEQ", opCodes.BEQ, Modes.Relative, 2 ),new Instruction( "SBC", opCodes.SBC, Modes.IndirectY, 5 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 4 ),new Instruction( "SBC", opCodes.SBC, Modes.ZeroPageX, 4 ),new Instruction( "INC", opCodes.INC, Modes.ZeroPageX, 6 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 6 ),new Instruction( "SED", opCodes.SED, Modes.Implied, 2 ),new Instruction( "SBC", opCodes.SBC, Modes.AbsoluteY, 4 ),new Instruction( "NOP", opCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 7 ),new Instruction( "???", opCodes.NOP, Modes.Implied, 4 ),new Instruction( "SBC", opCodes.SBC, Modes.AbsoluteX, 4 ),new Instruction( "INC", opCodes.INC, Modes.AbsoluteX, 7 ),new Instruction( "???", opCodes.XXX, Modes.Implied, 7 ),
            };

            return instructions;
        }

        public Dictionary<int, string> Disassemble(int start, int stop)
        {
            int address = start;
            int value = 0x00;
            int lowByte = 0x00;
            int highByte = 0x00;
            int lineAddress = 0x0000;

            var disassembly = new Dictionary<int, string>();

            while (address < stop)
            {
                lineAddress = address;
                string sInst = $"${address:X4}: ";
                int opcode = ReadMemory(address);
                Instruction instruction = lookupInstructionsTable[opcode];

                address++;
                sInst += $"{instruction.Name} ";

                if (instruction.AddressingMode == Modes.Implied)
                {
                    sInst += " {IMP}";
                }
                else if (instruction.AddressingMode == Modes.Immediate)
                {
                    value = ReadMemory(address);
                    address++;
                    sInst += $"#${value:X2} {{IMM}}";
                }
                else if (instruction.AddressingMode == Modes.ZeroPage)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = 0x00;
                    sInst += $"${lowByte:X2} {{ZP0}}";
                }
                else if (instruction.AddressingMode == Modes.ZeroPageX)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = 0x00;
                    sInst += $"${lowByte:X2},X {{ZPX}}";
                }
                else if (instruction.AddressingMode == Modes.ZeroPageY)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = 0x00;
                    sInst += $"${lowByte:X2},Y {{ZPY}}";
                }
                else if (instruction.AddressingMode == Modes.IndirectX)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = 0x00;
                    sInst += $"(${lowByte:X2},X) {{IZX}}";
                }
                else if (instruction.AddressingMode == Modes.IndirectY)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = 0x00;
                    sInst += $"(${lowByte:X2},) Y {{IZY}}";
                }

                else if (instruction.AddressingMode == Modes.Absolute)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = ReadMemory(address);
                    address++;
                    sInst += $"${highByte << 8 | lowByte:X4} {{ABS}}";
                }
                else if (instruction.AddressingMode == Modes.AbsoluteX)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = ReadMemory(address);
                    address++;
                    sInst += $"${highByte << 8 | lowByte:X4}, X {{ABX}}";
                }
                else if (instruction.AddressingMode == Modes.AbsoluteY)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = ReadMemory(address);
                    address++;
                    sInst += $"${highByte << 8 | lowByte:X4}, Y {{ABY}}";
                }
                else if (instruction.AddressingMode == Modes.Indirect)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = ReadMemory(address);
                    address++;
                    sInst += $"(${highByte << 8 | lowByte:X4}) {{IND}}";
                }
                else if (instruction.AddressingMode == Modes.Relative)
                {
                    value = ReadMemory(address);
                    address++;
                    sInst += $"${value:X2} [$ {address + value:X4}] {{REL}}";
                }
                
                disassembly[lineAddress] = sInst;
            }       

            return disassembly;
        }

        public void LoadProgram(byte[] program, int startAddress)
        {
            for(int i = 0; i < program.Length; i++)
            {
                bus?.Write(startAddress + i, program[i], false);
            }
        }

        public void StepProgramCounter(int offset)
        {
            programCounter += offset;
        }

        public void StepProgramCounter()
        {
            StepProgramCounter(1);
        }

        public void SetAbsoluteAddress(int address)
        {
            absoluteAddress = address;
        }

        public void SetFetchedValue(int value)
        {
            fetchedValue = value;
        }

        public void SetRelativeAddress(int value)
        {
            relativeAddress = value;
        }
    }
}