using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components
{
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
        public int AccumulatorRegister
        {
            get => accumulatorRegister;
            set => accumulatorRegister = value;
        }

        public int Status
        {
            get => statusRegister;
            set => statusRegister = value;
        }
        
        public int XRegister
        {
            get => xRegister;
            set => xRegister = value;
        }
        public int YRegister
        {
            get => yRegister;
            set => yRegister = value;
        }

        public int StackPointer
        {
            get => stackPointer;
            set => stackPointer = value;
        }

        public int Cycles
        {
            get => cycles;
            set => cycles = value;
        }

        public int FetchedValue
        {
            get => fetchedValue;
            set => fetchedValue = value;
        }

        public int AbsoluteAddress
        {
            get => absoluteAddress;
            set => absoluteAddress = value;
        }

        public int RelativeAddress
        {
            get => relativeAddress;
            set => relativeAddress = value;
        }

        public int ProgramCounter
        {
            get => programCounter;
            set => programCounter = value;
        }

        public int OpCode => opcode;
        public Instruction CurrentInstruction { get; private set; }
        public bool Complete => cycles == 0;


        public Ocl6502()
        {
            lookupInstructionsTable = BuildInstructionSet();
            CurrentInstruction = lookupInstructionsTable[0];
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
                CurrentInstruction = instruction;

                cycles = instruction.Cycles;
                int additionalCycle1 = instruction.AddressingMode.Execute(this);
                int additionalCycle2 = instruction.Operation.Execute(this);

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


        private static Instruction[] BuildInstructionSet()
        {
            var instructions = new Instruction[]
            {

                new Instruction( "BRK", OpCodes.BRK, Modes.Immediate, 7 ),new Instruction( "ORA", OpCodes.ORA, Modes.IndirectX, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 3 ),new Instruction( "ORA", OpCodes.ORA, Modes.ZeroPage, 3 ),new Instruction( "ASL", OpCodes.ASL, Modes.ZeroPage, 5 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 5 ),new Instruction( "PHP", OpCodes.PHP, Modes.Implied, 3 ),new Instruction( "ORA", OpCodes.ORA, Modes.Immediate, 2 ),new Instruction( "ASL", OpCodes.ASL, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 4 ),new Instruction( "ORA", OpCodes.ORA, Modes.Absolute, 4 ),new Instruction( "ASL", OpCodes.ASL, Modes.Absolute, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 6 ),
                new Instruction( "BPL", OpCodes.BPL, Modes.Relative, 2 ),new Instruction( "ORA", OpCodes.ORA, Modes.IndirectY, 5 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 4 ),new Instruction( "ORA", OpCodes.ORA, Modes.ZeroPageX, 4 ),new Instruction( "ASL", OpCodes.ASL, Modes.ZeroPageX, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 6 ),new Instruction( "CLC", OpCodes.CLC, Modes.Implied, 2 ),new Instruction( "ORA", OpCodes.ORA, Modes.AbsoluteY, 4 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 7 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 4 ),new Instruction( "ORA", OpCodes.ORA, Modes.AbsoluteX, 4 ),new Instruction( "ASL", OpCodes.ASL, Modes.AbsoluteX, 7 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 7 ),
                new Instruction( "JSR", OpCodes.JSR, Modes.Absolute, 6 ),new Instruction( "AND", OpCodes.AND, Modes.IndirectX, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 8 ),new Instruction( "BIT", OpCodes.BIT, Modes.ZeroPage, 3 ),new Instruction( "AND", OpCodes.AND, Modes.ZeroPage, 3 ),new Instruction( "ROL", OpCodes.ROL, Modes.ZeroPage, 5 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 5 ),new Instruction( "PLP", OpCodes.PLP, Modes.Implied, 4 ),new Instruction( "AND", OpCodes.AND, Modes.Immediate, 2 ),new Instruction( "ROL", OpCodes.ROL, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "BIT", OpCodes.BIT, Modes.Absolute, 4 ),new Instruction( "AND", OpCodes.AND, Modes.Absolute, 4 ),new Instruction( "ROL", OpCodes.ROL, Modes.Absolute, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 6 ),
                new Instruction( "BMI", OpCodes.BMI, Modes.Relative, 2 ),new Instruction( "AND", OpCodes.AND, Modes.IndirectY, 5 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 4 ),new Instruction( "AND", OpCodes.AND, Modes.ZeroPageX, 4 ),new Instruction( "ROL", OpCodes.ROL, Modes.ZeroPageX, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 6 ),new Instruction( "SEC", OpCodes.SEC, Modes.Implied, 2 ),new Instruction( "AND", OpCodes.AND, Modes.AbsoluteY, 4 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 7 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 4 ),new Instruction( "AND", OpCodes.AND, Modes.AbsoluteX, 4 ),new Instruction( "ROL", OpCodes.ROL, Modes.AbsoluteX, 7 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 7 ),
                new Instruction( "RTI", OpCodes.RTI, Modes.Implied, 6 ),new Instruction( "EOR", OpCodes.EOR, Modes.IndirectX, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 3 ),new Instruction( "EOR", OpCodes.EOR, Modes.ZeroPage, 3 ),new Instruction( "LSR", OpCodes.LSR, Modes.ZeroPage, 5 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 5 ),new Instruction( "PHA", OpCodes.PHA, Modes.Implied, 3 ),new Instruction( "EOR", OpCodes.EOR, Modes.Immediate, 2 ),new Instruction( "LSR", OpCodes.LSR, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "JMP", OpCodes.JMP, Modes.Absolute, 3 ),new Instruction( "EOR", OpCodes.EOR, Modes.Absolute, 4 ),new Instruction( "LSR", OpCodes.LSR, Modes.Absolute, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 6 ),
                new Instruction( "BVC", OpCodes.BVC, Modes.Relative, 2 ),new Instruction( "EOR", OpCodes.EOR, Modes.IndirectY, 5 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 4 ),new Instruction( "EOR", OpCodes.EOR, Modes.ZeroPageX, 4 ),new Instruction( "LSR", OpCodes.LSR, Modes.ZeroPageX, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 6 ),new Instruction( "CLI", OpCodes.CLI, Modes.Implied, 2 ),new Instruction( "EOR", OpCodes.EOR, Modes.AbsoluteY, 4 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 7 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 4 ),new Instruction( "EOR", OpCodes.EOR, Modes.AbsoluteX, 4 ),new Instruction( "LSR", OpCodes.LSR, Modes.AbsoluteX, 7 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 7 ),
                new Instruction( "RTS", OpCodes.RTS, Modes.Implied, 6 ),new Instruction( "ADC", OpCodes.ADC, Modes.IndirectX, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 3 ),new Instruction( "ADC", OpCodes.ADC, Modes.ZeroPage, 3 ),new Instruction( "ROR", OpCodes.ROR, Modes.ZeroPage, 5 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 5 ),new Instruction( "PLA", OpCodes.PLA, Modes.Implied, 4 ),new Instruction( "ADC", OpCodes.ADC, Modes.Immediate, 2 ),new Instruction( "ROR", OpCodes.ROR, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "JMP", OpCodes.JMP, Modes.Indirect, 5 ),new Instruction( "ADC", OpCodes.ADC, Modes.Absolute, 4 ),new Instruction( "ROR", OpCodes.ROR, Modes.Absolute, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 6 ),
                new Instruction( "BVS", OpCodes.BVS, Modes.Relative, 2 ),new Instruction( "ADC", OpCodes.ADC, Modes.IndirectY, 5 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 4 ),new Instruction( "ADC", OpCodes.ADC, Modes.ZeroPageX, 4 ),new Instruction( "ROR", OpCodes.ROR, Modes.ZeroPageX, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 6 ),new Instruction( "SEI", OpCodes.SEI, Modes.Implied, 2 ),new Instruction( "ADC", OpCodes.ADC, Modes.AbsoluteY, 4 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 7 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 4 ),new Instruction( "ADC", OpCodes.ADC, Modes.AbsoluteX, 4 ),new Instruction( "ROR", OpCodes.ROR, Modes.AbsoluteX, 7 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 7 ),
                new Instruction( "???", OpCodes.NOP, Modes.Implied, 2 ),new Instruction( "STA", OpCodes.STA, Modes.IndirectX, 6 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 6 ),new Instruction( "STY", OpCodes.STY, Modes.ZeroPage, 3 ),new Instruction( "STA", OpCodes.STA, Modes.ZeroPage, 3 ),new Instruction( "STX", OpCodes.STX, Modes.ZeroPage, 3 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 3 ),new Instruction( "DEY", OpCodes.DEY, Modes.Implied, 2 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 2 ),new Instruction( "TXA", OpCodes.TXA, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "STY", OpCodes.STY, Modes.Absolute, 4 ),new Instruction( "STA", OpCodes.STA, Modes.Absolute, 4 ),new Instruction( "STX", OpCodes.STX, Modes.Absolute, 4 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 4 ),
                new Instruction( "BCC", OpCodes.BCC, Modes.Relative, 2 ),new Instruction( "STA", OpCodes.STA, Modes.IndirectY, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 6 ),new Instruction( "STY", OpCodes.STY, Modes.ZeroPageX, 4 ),new Instruction( "STA", OpCodes.STA, Modes.ZeroPageX, 4 ),new Instruction( "STX", OpCodes.STX, Modes.ZeroPageY, 4 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 4 ),new Instruction( "TYA", OpCodes.TYA, Modes.Implied, 2 ),new Instruction( "STA", OpCodes.STA, Modes.AbsoluteY, 5 ),new Instruction( "TXS", OpCodes.TXS, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 5 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 5 ),new Instruction( "STA", OpCodes.STA, Modes.AbsoluteX, 5 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 5 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 5 ),
                new Instruction( "LDY", OpCodes.LDY, Modes.Immediate, 2 ),new Instruction( "LDA", OpCodes.LDA, Modes.IndirectX, 6 ),new Instruction( "LDX", OpCodes.LDX, Modes.Immediate, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 6 ),new Instruction( "LDY", OpCodes.LDY, Modes.ZeroPage, 3 ),new Instruction( "LDA", OpCodes.LDA, Modes.ZeroPage, 3 ),new Instruction( "LDX", OpCodes.LDX, Modes.ZeroPage, 3 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 3 ),new Instruction( "TAY", OpCodes.TAY, Modes.Implied, 2 ),new Instruction( "LDA", OpCodes.LDA, Modes.Immediate, 2 ),new Instruction( "TAX", OpCodes.TAX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "LDY", OpCodes.LDY, Modes.Absolute, 4 ),new Instruction( "LDA", OpCodes.LDA, Modes.Absolute, 4 ),new Instruction( "LDX", OpCodes.LDX, Modes.Absolute, 4 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 4 ),
                new Instruction( "BCS", OpCodes.BCS, Modes.Relative, 2 ),new Instruction( "LDA", OpCodes.LDA, Modes.IndirectY, 5 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 5 ),new Instruction( "LDY", OpCodes.LDY, Modes.ZeroPageX, 4 ),new Instruction( "LDA", OpCodes.LDA, Modes.ZeroPageX, 4 ),new Instruction( "LDX", OpCodes.LDX, Modes.ZeroPageY, 4 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 4 ),new Instruction( "CLV", OpCodes.CLV, Modes.Implied, 2 ),new Instruction( "LDA", OpCodes.LDA, Modes.AbsoluteY, 4 ),new Instruction( "TSX", OpCodes.TSX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 4 ),new Instruction( "LDY", OpCodes.LDY, Modes.AbsoluteX, 4 ),new Instruction( "LDA", OpCodes.LDA, Modes.AbsoluteX, 4 ),new Instruction( "LDX", OpCodes.LDX, Modes.AbsoluteY, 4 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 4 ),
                new Instruction( "CPY", OpCodes.CPY, Modes.Immediate, 2 ),new Instruction( "CMP", OpCodes.CMP, Modes.IndirectX, 6 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 8 ),new Instruction( "CPY", OpCodes.CPY, Modes.ZeroPage, 3 ),new Instruction( "CMP", OpCodes.CMP, Modes.ZeroPage, 3 ),new Instruction( "DEC", OpCodes.DEC, Modes.ZeroPage, 5 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 5 ),new Instruction( "INY", OpCodes.INY, Modes.Implied, 2 ),new Instruction( "CMP", OpCodes.CMP, Modes.Immediate, 2 ),new Instruction( "DEX", OpCodes.DEX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "CPY", OpCodes.CPY, Modes.Absolute, 4 ),new Instruction( "CMP", OpCodes.CMP, Modes.Absolute, 4 ),new Instruction( "DEC", OpCodes.DEC, Modes.Absolute, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 6 ),
                new Instruction( "BNE", OpCodes.BNE, Modes.Relative, 2 ),new Instruction( "CMP", OpCodes.CMP, Modes.IndirectY, 5 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 4 ),new Instruction( "CMP", OpCodes.CMP, Modes.ZeroPageX, 4 ),new Instruction( "DEC", OpCodes.DEC, Modes.ZeroPageX, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 6 ),new Instruction( "CLD", OpCodes.CLD, Modes.Implied, 2 ),new Instruction( "CMP", OpCodes.CMP, Modes.AbsoluteY, 4 ),new Instruction( "NOP", OpCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 7 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 4 ),new Instruction( "CMP", OpCodes.CMP, Modes.AbsoluteX, 4 ),new Instruction( "DEC", OpCodes.DEC, Modes.AbsoluteX, 7 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 7 ),
                new Instruction( "CPX", OpCodes.CPX, Modes.Immediate, 2 ),new Instruction( "SBC", OpCodes.SBC, Modes.IndirectX, 6 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 8 ),new Instruction( "CPX", OpCodes.CPX, Modes.ZeroPage, 3 ),new Instruction( "SBC", OpCodes.SBC, Modes.ZeroPage, 3 ),new Instruction( "INC", OpCodes.INC, Modes.ZeroPage, 5 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 5 ),new Instruction( "INX", OpCodes.INX, Modes.Implied, 2 ),new Instruction( "SBC", OpCodes.SBC, Modes.Immediate, 2 ),new Instruction( "NOP", OpCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", OpCodes.SBC, Modes.Implied, 2 ),new Instruction( "CPX", OpCodes.CPX, Modes.Absolute, 4 ),new Instruction( "SBC", OpCodes.SBC, Modes.Absolute, 4 ),new Instruction( "INC", OpCodes.INC, Modes.Absolute, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 6 ),
                new Instruction( "BEQ", OpCodes.BEQ, Modes.Relative, 2 ),new Instruction( "SBC", OpCodes.SBC, Modes.IndirectY, 5 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 8 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 4 ),new Instruction( "SBC", OpCodes.SBC, Modes.ZeroPageX, 4 ),new Instruction( "INC", OpCodes.INC, Modes.ZeroPageX, 6 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 6 ),new Instruction( "SED", OpCodes.SED, Modes.Implied, 2 ),new Instruction( "SBC", OpCodes.SBC, Modes.AbsoluteY, 4 ),new Instruction( "NOP", OpCodes.NOP, Modes.Implied, 2 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 7 ),new Instruction( "???", OpCodes.NOP, Modes.Implied, 4 ),new Instruction( "SBC", OpCodes.SBC, Modes.AbsoluteX, 4 ),new Instruction( "INC", OpCodes.INC, Modes.AbsoluteX, 7 ),new Instruction( "???", OpCodes.XXX, Modes.Implied, 7 ),
            };

            return instructions;
        }

        public Dictionary<int, string> Disassemble(int start, int stop)
        {
            int address = start;
            var disassembly = new Dictionary<int, string>();

            while (address < stop)
            {
                int lineAddress = address;
                string sInst = $"${address:X4}: ";
                int opcode = ReadMemory(address);
                Instruction instruction = lookupInstructionsTable[opcode];

                address++;
                sInst += $"{instruction.Name} ";

                int value;
                int lowByte;
                int highByte;
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
                    sInst += $"${value:X2} [${address + value:X4}] {{REL}}";
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
    }
}