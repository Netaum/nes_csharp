using emulator.components.Enums;
using emulator.components.Interfaces;
using emulator.components.OpCodes;
using emulator.components.AddressingModes;
namespace emulator.components
{
    public class Ocl6502 : ICpu
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


        public byte ReadMemory(int address, bool readOnly = false)
        {
            return Bus.CpuRead(address, readOnly);
        }

        public byte ReadMemory()
        {
            return Bus.CpuRead(programCounter);
        }

        public void WriteMemory(int address, byte value)
        {
            Bus.CpuWrite(address, value);
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

            WriteMemory(0x0100 + stackPointer, (programCounter >> 8).ToByte());
            stackPointer--;
            WriteMemory(0x0100 + stackPointer, programCounter.ToByte());
            stackPointer--;

            SetStatusFlag(Flags6502.B, false);
            SetStatusFlag(Flags6502.I, true);
            SetStatusFlag(Flags6502.U, true);
            WriteMemory(0x0100 + stackPointer, statusRegister.ToByte());
            stackPointer--;

            absoluteAddress = 0xFFFE;
            int lowByte = ReadMemory(absoluteAddress);
            int highByte = ReadMemory(absoluteAddress + 1);
            programCounter = (highByte << 8) | lowByte;

            cycles = 7;
        }

        public void NonMaskableInterrupt()
        {
            WriteMemory(0x0100 + stackPointer, (programCounter >> 8).ToByte());
            stackPointer--;
            WriteMemory(0x0100 + stackPointer, programCounter.ToByte());
            stackPointer--;

            SetStatusFlag(Flags6502.B, false);
            SetStatusFlag(Flags6502.I, true);
            SetStatusFlag(Flags6502.U, true);
            WriteMemory(0x0100 + stackPointer, statusRegister.ToByte());
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

            if (instruction.AddressingMode != InstructionAddressingModes.Implied)
            {
                fetchedValue = ReadMemory(absoluteAddress);
            }

            return fetchedValue;
        }


        private static Instruction[] BuildInstructionSet()
        {
            var instructions = new Instruction[]
            {

                new Instruction( "BRK", ProcessorInstructions.BRK, InstructionAddressingModes.Immediate, 7 ),new Instruction( "ORA", ProcessorInstructions.ORA, InstructionAddressingModes.IndirectX, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 8 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 3 ),new Instruction( "ORA", ProcessorInstructions.ORA, InstructionAddressingModes.ZeroPage, 3 ),new Instruction( "ASL", ProcessorInstructions.ASL, InstructionAddressingModes.ZeroPage, 5 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 5 ),new Instruction( "PHP", ProcessorInstructions.PHP, InstructionAddressingModes.Implied, 3 ),new Instruction( "ORA", ProcessorInstructions.ORA, InstructionAddressingModes.Immediate, 2 ),new Instruction( "ASL", ProcessorInstructions.ASL, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 4 ),new Instruction( "ORA", ProcessorInstructions.ORA, InstructionAddressingModes.Absolute, 4 ),new Instruction( "ASL", ProcessorInstructions.ASL, InstructionAddressingModes.Absolute, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 6 ),
                new Instruction( "BPL", ProcessorInstructions.BPL, InstructionAddressingModes.Relative, 2 ),new Instruction( "ORA", ProcessorInstructions.ORA, InstructionAddressingModes.IndirectY, 5 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 8 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 4 ),new Instruction( "ORA", ProcessorInstructions.ORA, InstructionAddressingModes.ZeroPageX, 4 ),new Instruction( "ASL", ProcessorInstructions.ASL, InstructionAddressingModes.ZeroPageX, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 6 ),new Instruction( "CLC", ProcessorInstructions.CLC, InstructionAddressingModes.Implied, 2 ),new Instruction( "ORA", ProcessorInstructions.ORA, InstructionAddressingModes.AbsoluteY, 4 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 7 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 4 ),new Instruction( "ORA", ProcessorInstructions.ORA, InstructionAddressingModes.AbsoluteX, 4 ),new Instruction( "ASL", ProcessorInstructions.ASL, InstructionAddressingModes.AbsoluteX, 7 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 7 ),
                new Instruction( "JSR", ProcessorInstructions.JSR, InstructionAddressingModes.Absolute, 6 ),new Instruction( "AND", ProcessorInstructions.AND, InstructionAddressingModes.IndirectX, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 8 ),new Instruction( "BIT", ProcessorInstructions.BIT, InstructionAddressingModes.ZeroPage, 3 ),new Instruction( "AND", ProcessorInstructions.AND, InstructionAddressingModes.ZeroPage, 3 ),new Instruction( "ROL", ProcessorInstructions.ROL, InstructionAddressingModes.ZeroPage, 5 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 5 ),new Instruction( "PLP", ProcessorInstructions.PLP, InstructionAddressingModes.Implied, 4 ),new Instruction( "AND", ProcessorInstructions.AND, InstructionAddressingModes.Immediate, 2 ),new Instruction( "ROL", ProcessorInstructions.ROL, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "BIT", ProcessorInstructions.BIT, InstructionAddressingModes.Absolute, 4 ),new Instruction( "AND", ProcessorInstructions.AND, InstructionAddressingModes.Absolute, 4 ),new Instruction( "ROL", ProcessorInstructions.ROL, InstructionAddressingModes.Absolute, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 6 ),
                new Instruction( "BMI", ProcessorInstructions.BMI, InstructionAddressingModes.Relative, 2 ),new Instruction( "AND", ProcessorInstructions.AND, InstructionAddressingModes.IndirectY, 5 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 8 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 4 ),new Instruction( "AND", ProcessorInstructions.AND, InstructionAddressingModes.ZeroPageX, 4 ),new Instruction( "ROL", ProcessorInstructions.ROL, InstructionAddressingModes.ZeroPageX, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 6 ),new Instruction( "SEC", ProcessorInstructions.SEC, InstructionAddressingModes.Implied, 2 ),new Instruction( "AND", ProcessorInstructions.AND, InstructionAddressingModes.AbsoluteY, 4 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 7 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 4 ),new Instruction( "AND", ProcessorInstructions.AND, InstructionAddressingModes.AbsoluteX, 4 ),new Instruction( "ROL", ProcessorInstructions.ROL, InstructionAddressingModes.AbsoluteX, 7 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 7 ),
                new Instruction( "RTI", ProcessorInstructions.RTI, InstructionAddressingModes.Implied, 6 ),new Instruction( "EOR", ProcessorInstructions.EOR, InstructionAddressingModes.IndirectX, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 8 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 3 ),new Instruction( "EOR", ProcessorInstructions.EOR, InstructionAddressingModes.ZeroPage, 3 ),new Instruction( "LSR", ProcessorInstructions.LSR, InstructionAddressingModes.ZeroPage, 5 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 5 ),new Instruction( "PHA", ProcessorInstructions.PHA, InstructionAddressingModes.Implied, 3 ),new Instruction( "EOR", ProcessorInstructions.EOR, InstructionAddressingModes.Immediate, 2 ),new Instruction( "LSR", ProcessorInstructions.LSR, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "JMP", ProcessorInstructions.JMP, InstructionAddressingModes.Absolute, 3 ),new Instruction( "EOR", ProcessorInstructions.EOR, InstructionAddressingModes.Absolute, 4 ),new Instruction( "LSR", ProcessorInstructions.LSR, InstructionAddressingModes.Absolute, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 6 ),
                new Instruction( "BVC", ProcessorInstructions.BVC, InstructionAddressingModes.Relative, 2 ),new Instruction( "EOR", ProcessorInstructions.EOR, InstructionAddressingModes.IndirectY, 5 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 8 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 4 ),new Instruction( "EOR", ProcessorInstructions.EOR, InstructionAddressingModes.ZeroPageX, 4 ),new Instruction( "LSR", ProcessorInstructions.LSR, InstructionAddressingModes.ZeroPageX, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 6 ),new Instruction( "CLI", ProcessorInstructions.CLI, InstructionAddressingModes.Implied, 2 ),new Instruction( "EOR", ProcessorInstructions.EOR, InstructionAddressingModes.AbsoluteY, 4 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 7 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 4 ),new Instruction( "EOR", ProcessorInstructions.EOR, InstructionAddressingModes.AbsoluteX, 4 ),new Instruction( "LSR", ProcessorInstructions.LSR, InstructionAddressingModes.AbsoluteX, 7 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 7 ),
                new Instruction( "RTS", ProcessorInstructions.RTS, InstructionAddressingModes.Implied, 6 ),new Instruction( "ADC", ProcessorInstructions.ADC, InstructionAddressingModes.IndirectX, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 8 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 3 ),new Instruction( "ADC", ProcessorInstructions.ADC, InstructionAddressingModes.ZeroPage, 3 ),new Instruction( "ROR", ProcessorInstructions.ROR, InstructionAddressingModes.ZeroPage, 5 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 5 ),new Instruction( "PLA", ProcessorInstructions.PLA, InstructionAddressingModes.Implied, 4 ),new Instruction( "ADC", ProcessorInstructions.ADC, InstructionAddressingModes.Immediate, 2 ),new Instruction( "ROR", ProcessorInstructions.ROR, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "JMP", ProcessorInstructions.JMP, InstructionAddressingModes.Indirect, 5 ),new Instruction( "ADC", ProcessorInstructions.ADC, InstructionAddressingModes.Absolute, 4 ),new Instruction( "ROR", ProcessorInstructions.ROR, InstructionAddressingModes.Absolute, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 6 ),
                new Instruction( "BVS", ProcessorInstructions.BVS, InstructionAddressingModes.Relative, 2 ),new Instruction( "ADC", ProcessorInstructions.ADC, InstructionAddressingModes.IndirectY, 5 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 8 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 4 ),new Instruction( "ADC", ProcessorInstructions.ADC, InstructionAddressingModes.ZeroPageX, 4 ),new Instruction( "ROR", ProcessorInstructions.ROR, InstructionAddressingModes.ZeroPageX, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 6 ),new Instruction( "SEI", ProcessorInstructions.SEI, InstructionAddressingModes.Implied, 2 ),new Instruction( "ADC", ProcessorInstructions.ADC, InstructionAddressingModes.AbsoluteY, 4 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 7 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 4 ),new Instruction( "ADC", ProcessorInstructions.ADC, InstructionAddressingModes.AbsoluteX, 4 ),new Instruction( "ROR", ProcessorInstructions.ROR, InstructionAddressingModes.AbsoluteX, 7 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 7 ),
                new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 2 ),new Instruction( "STA", ProcessorInstructions.STA, InstructionAddressingModes.IndirectX, 6 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 6 ),new Instruction( "STY", ProcessorInstructions.STY, InstructionAddressingModes.ZeroPage, 3 ),new Instruction( "STA", ProcessorInstructions.STA, InstructionAddressingModes.ZeroPage, 3 ),new Instruction( "STX", ProcessorInstructions.STX, InstructionAddressingModes.ZeroPage, 3 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 3 ),new Instruction( "DEY", ProcessorInstructions.DEY, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 2 ),new Instruction( "TXA", ProcessorInstructions.TXA, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "STY", ProcessorInstructions.STY, InstructionAddressingModes.Absolute, 4 ),new Instruction( "STA", ProcessorInstructions.STA, InstructionAddressingModes.Absolute, 4 ),new Instruction( "STX", ProcessorInstructions.STX, InstructionAddressingModes.Absolute, 4 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 4 ),
                new Instruction( "BCC", ProcessorInstructions.BCC, InstructionAddressingModes.Relative, 2 ),new Instruction( "STA", ProcessorInstructions.STA, InstructionAddressingModes.IndirectY, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 6 ),new Instruction( "STY", ProcessorInstructions.STY, InstructionAddressingModes.ZeroPageX, 4 ),new Instruction( "STA", ProcessorInstructions.STA, InstructionAddressingModes.ZeroPageX, 4 ),new Instruction( "STX", ProcessorInstructions.STX, InstructionAddressingModes.ZeroPageY, 4 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 4 ),new Instruction( "TYA", ProcessorInstructions.TYA, InstructionAddressingModes.Implied, 2 ),new Instruction( "STA", ProcessorInstructions.STA, InstructionAddressingModes.AbsoluteY, 5 ),new Instruction( "TXS", ProcessorInstructions.TXS, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 5 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 5 ),new Instruction( "STA", ProcessorInstructions.STA, InstructionAddressingModes.AbsoluteX, 5 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 5 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 5 ),
                new Instruction( "LDY", ProcessorInstructions.LDY, InstructionAddressingModes.Immediate, 2 ),new Instruction( "LDA", ProcessorInstructions.LDA, InstructionAddressingModes.IndirectX, 6 ),new Instruction( "LDX", ProcessorInstructions.LDX, InstructionAddressingModes.Immediate, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 6 ),new Instruction( "LDY", ProcessorInstructions.LDY, InstructionAddressingModes.ZeroPage, 3 ),new Instruction( "LDA", ProcessorInstructions.LDA, InstructionAddressingModes.ZeroPage, 3 ),new Instruction( "LDX", ProcessorInstructions.LDX, InstructionAddressingModes.ZeroPage, 3 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 3 ),new Instruction( "TAY", ProcessorInstructions.TAY, InstructionAddressingModes.Implied, 2 ),new Instruction( "LDA", ProcessorInstructions.LDA, InstructionAddressingModes.Immediate, 2 ),new Instruction( "TAX", ProcessorInstructions.TAX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "LDY", ProcessorInstructions.LDY, InstructionAddressingModes.Absolute, 4 ),new Instruction( "LDA", ProcessorInstructions.LDA, InstructionAddressingModes.Absolute, 4 ),new Instruction( "LDX", ProcessorInstructions.LDX, InstructionAddressingModes.Absolute, 4 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 4 ),
                new Instruction( "BCS", ProcessorInstructions.BCS, InstructionAddressingModes.Relative, 2 ),new Instruction( "LDA", ProcessorInstructions.LDA, InstructionAddressingModes.IndirectY, 5 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 5 ),new Instruction( "LDY", ProcessorInstructions.LDY, InstructionAddressingModes.ZeroPageX, 4 ),new Instruction( "LDA", ProcessorInstructions.LDA, InstructionAddressingModes.ZeroPageX, 4 ),new Instruction( "LDX", ProcessorInstructions.LDX, InstructionAddressingModes.ZeroPageY, 4 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 4 ),new Instruction( "CLV", ProcessorInstructions.CLV, InstructionAddressingModes.Implied, 2 ),new Instruction( "LDA", ProcessorInstructions.LDA, InstructionAddressingModes.AbsoluteY, 4 ),new Instruction( "TSX", ProcessorInstructions.TSX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 4 ),new Instruction( "LDY", ProcessorInstructions.LDY, InstructionAddressingModes.AbsoluteX, 4 ),new Instruction( "LDA", ProcessorInstructions.LDA, InstructionAddressingModes.AbsoluteX, 4 ),new Instruction( "LDX", ProcessorInstructions.LDX, InstructionAddressingModes.AbsoluteY, 4 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 4 ),
                new Instruction( "CPY", ProcessorInstructions.CPY, InstructionAddressingModes.Immediate, 2 ),new Instruction( "CMP", ProcessorInstructions.CMP, InstructionAddressingModes.IndirectX, 6 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 8 ),new Instruction( "CPY", ProcessorInstructions.CPY, InstructionAddressingModes.ZeroPage, 3 ),new Instruction( "CMP", ProcessorInstructions.CMP, InstructionAddressingModes.ZeroPage, 3 ),new Instruction( "DEC", ProcessorInstructions.DEC, InstructionAddressingModes.ZeroPage, 5 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 5 ),new Instruction( "INY", ProcessorInstructions.INY, InstructionAddressingModes.Implied, 2 ),new Instruction( "CMP", ProcessorInstructions.CMP, InstructionAddressingModes.Immediate, 2 ),new Instruction( "DEX", ProcessorInstructions.DEX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "CPY", ProcessorInstructions.CPY, InstructionAddressingModes.Absolute, 4 ),new Instruction( "CMP", ProcessorInstructions.CMP, InstructionAddressingModes.Absolute, 4 ),new Instruction( "DEC", ProcessorInstructions.DEC, InstructionAddressingModes.Absolute, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 6 ),
                new Instruction( "BNE", ProcessorInstructions.BNE, InstructionAddressingModes.Relative, 2 ),new Instruction( "CMP", ProcessorInstructions.CMP, InstructionAddressingModes.IndirectY, 5 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 8 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 4 ),new Instruction( "CMP", ProcessorInstructions.CMP, InstructionAddressingModes.ZeroPageX, 4 ),new Instruction( "DEC", ProcessorInstructions.DEC, InstructionAddressingModes.ZeroPageX, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 6 ),new Instruction( "CLD", ProcessorInstructions.CLD, InstructionAddressingModes.Implied, 2 ),new Instruction( "CMP", ProcessorInstructions.CMP, InstructionAddressingModes.AbsoluteY, 4 ),new Instruction( "NOP", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 7 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 4 ),new Instruction( "CMP", ProcessorInstructions.CMP, InstructionAddressingModes.AbsoluteX, 4 ),new Instruction( "DEC", ProcessorInstructions.DEC, InstructionAddressingModes.AbsoluteX, 7 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 7 ),
                new Instruction( "CPX", ProcessorInstructions.CPX, InstructionAddressingModes.Immediate, 2 ),new Instruction( "SBC", ProcessorInstructions.SBC, InstructionAddressingModes.IndirectX, 6 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 8 ),new Instruction( "CPX", ProcessorInstructions.CPX, InstructionAddressingModes.ZeroPage, 3 ),new Instruction( "SBC", ProcessorInstructions.SBC, InstructionAddressingModes.ZeroPage, 3 ),new Instruction( "INC", ProcessorInstructions.INC, InstructionAddressingModes.ZeroPage, 5 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 5 ),new Instruction( "INX", ProcessorInstructions.INX, InstructionAddressingModes.Implied, 2 ),new Instruction( "SBC", ProcessorInstructions.SBC, InstructionAddressingModes.Immediate, 2 ),new Instruction( "NOP", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.SBC, InstructionAddressingModes.Implied, 2 ),new Instruction( "CPX", ProcessorInstructions.CPX, InstructionAddressingModes.Absolute, 4 ),new Instruction( "SBC", ProcessorInstructions.SBC, InstructionAddressingModes.Absolute, 4 ),new Instruction( "INC", ProcessorInstructions.INC, InstructionAddressingModes.Absolute, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 6 ),
                new Instruction( "BEQ", ProcessorInstructions.BEQ, InstructionAddressingModes.Relative, 2 ),new Instruction( "SBC", ProcessorInstructions.SBC, InstructionAddressingModes.IndirectY, 5 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 8 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 4 ),new Instruction( "SBC", ProcessorInstructions.SBC, InstructionAddressingModes.ZeroPageX, 4 ),new Instruction( "INC", ProcessorInstructions.INC, InstructionAddressingModes.ZeroPageX, 6 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 6 ),new Instruction( "SED", ProcessorInstructions.SED, InstructionAddressingModes.Implied, 2 ),new Instruction( "SBC", ProcessorInstructions.SBC, InstructionAddressingModes.AbsoluteY, 4 ),new Instruction( "NOP", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 2 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 7 ),new Instruction( "???", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 4 ),new Instruction( "SBC", ProcessorInstructions.SBC, InstructionAddressingModes.AbsoluteX, 4 ),new Instruction( "INC", ProcessorInstructions.INC, InstructionAddressingModes.AbsoluteX, 7 ),new Instruction( "???", ProcessorInstructions.XXX, InstructionAddressingModes.Implied, 7 ),
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
                if (instruction.AddressingMode == InstructionAddressingModes.Implied)
                {
                    sInst += " {IMP}";
                }
                else if (instruction.AddressingMode == InstructionAddressingModes.Immediate)
                {
                    value = ReadMemory(address);
                    address++;
                    sInst += $"#${value:X2} {{IMM}}";
                }
                else if (instruction.AddressingMode == InstructionAddressingModes.ZeroPage)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = 0x00;
                    sInst += $"${lowByte:X2} {{ZP0}}";
                }
                else if (instruction.AddressingMode == InstructionAddressingModes.ZeroPageX)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    sInst += $"${lowByte:X2},X {{ZPX}}";
                }
                else if (instruction.AddressingMode == InstructionAddressingModes.ZeroPageY)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = 0x00;
                    sInst += $"${lowByte:X2},Y {{ZPY}}";
                }
                else if (instruction.AddressingMode == InstructionAddressingModes.IndirectX)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = 0x00;
                    sInst += $"(${lowByte:X2},X) {{IZX}}";
                }
                else if (instruction.AddressingMode == InstructionAddressingModes.IndirectY)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = 0x00;
                    sInst += $"(${lowByte:X2},) Y {{IZY}}";
                }

                else if (instruction.AddressingMode == InstructionAddressingModes.Absolute)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = ReadMemory(address);
                    address++;
                    sInst += $"${highByte << 8 | lowByte:X4} {{ABS}}";
                }
                else if (instruction.AddressingMode == InstructionAddressingModes.AbsoluteX)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = ReadMemory(address);
                    address++;
                    sInst += $"${highByte << 8 | lowByte:X4}, X {{ABX}}";
                }
                else if (instruction.AddressingMode == InstructionAddressingModes.AbsoluteY)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = ReadMemory(address);
                    address++;
                    sInst += $"${highByte << 8 | lowByte:X4}, Y {{ABY}}";
                }
                else if (instruction.AddressingMode == InstructionAddressingModes.Indirect)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = ReadMemory(address);
                    address++;
                    sInst += $"(${highByte << 8 | lowByte:X4}) {{IND}}";
                }
                else if (instruction.AddressingMode == InstructionAddressingModes.Relative)
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
                bus?.CpuWrite(startAddress + i, program[i]);
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