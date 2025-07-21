using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nes_csharp.components
{
    public interface ICpu
    {
        void ConnectBus(IBus bus);

        void Clock();
        void Reset();
        void Interrupt();
        void NonMaskableInterrupt();
        int ReadMemory(int address);

        Instruction[] Instructions { get; }

        int Fetch();

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

        public Ocl6502()
        {
            lookupInstructionsTable = BuildInstructionSet(this, this);
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
                int additionalCycle1 = instruction.AddressingMode.Invoke();
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

            if (instruction.AddressingMode != Implied)
            {
                fetchedValue = ReadMemory(absoluteAddress);
            }

            return fetchedValue;
        }


        private static Instruction[] BuildInstructionSet(IAddressingModes addressingModes, IOpCodes opCodes)
        {
            var instructions = new Instruction[]
            {

                new Instruction( "BRK", opCodes.BRK, addressingModes.Immediate, 7 ),new Instruction( "ORA", opCodes.ORA, addressingModes.IndirectX, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 8 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 3 ),new Instruction( "ORA", opCodes.ORA, addressingModes.ZeroPage, 3 ),new Instruction( "ASL", opCodes.ASL, addressingModes.ZeroPage, 5 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 5 ),new Instruction( "PHP", opCodes.PHP, addressingModes.Implied, 3 ),new Instruction( "ORA", opCodes.ORA, addressingModes.Immediate, 2 ),new Instruction( "ASL", opCodes.ASL, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 4 ),new Instruction( "ORA", opCodes.ORA, addressingModes.Absolute, 4 ),new Instruction( "ASL", opCodes.ASL, addressingModes.Absolute, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 6 ),
                new Instruction( "BPL", opCodes.BPL, addressingModes.Relative, 2 ),new Instruction( "ORA", opCodes.ORA, addressingModes.IndirectY, 5 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 8 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 4 ),new Instruction( "ORA", opCodes.ORA, addressingModes.ZeroPageX, 4 ),new Instruction( "ASL", opCodes.ASL, addressingModes.ZeroPageX, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 6 ),new Instruction( "CLC", opCodes.CLC, addressingModes.Implied, 2 ),new Instruction( "ORA", opCodes.ORA, addressingModes.AbsoluteY, 4 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 7 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 4 ),new Instruction( "ORA", opCodes.ORA, addressingModes.AbsoluteX, 4 ),new Instruction( "ASL", opCodes.ASL, addressingModes.AbsoluteX, 7 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 7 ),
                new Instruction( "JSR", opCodes.JSR, addressingModes.Absolute, 6 ),new Instruction( "AND", opCodes.AND, addressingModes.IndirectX, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 8 ),new Instruction( "BIT", opCodes.BIT, addressingModes.ZeroPage, 3 ),new Instruction( "AND", opCodes.AND, addressingModes.ZeroPage, 3 ),new Instruction( "ROL", opCodes.ROL, addressingModes.ZeroPage, 5 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 5 ),new Instruction( "PLP", opCodes.PLP, addressingModes.Implied, 4 ),new Instruction( "AND", opCodes.AND, addressingModes.Immediate, 2 ),new Instruction( "ROL", opCodes.ROL, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "BIT", opCodes.BIT, addressingModes.Absolute, 4 ),new Instruction( "AND", opCodes.AND, addressingModes.Absolute, 4 ),new Instruction( "ROL", opCodes.ROL, addressingModes.Absolute, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 6 ),
                new Instruction( "BMI", opCodes.BMI, addressingModes.Relative, 2 ),new Instruction( "AND", opCodes.AND, addressingModes.IndirectY, 5 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 8 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 4 ),new Instruction( "AND", opCodes.AND, addressingModes.ZeroPageX, 4 ),new Instruction( "ROL", opCodes.ROL, addressingModes.ZeroPageX, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 6 ),new Instruction( "SEC", opCodes.SEC, addressingModes.Implied, 2 ),new Instruction( "AND", opCodes.AND, addressingModes.AbsoluteY, 4 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 7 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 4 ),new Instruction( "AND", opCodes.AND, addressingModes.AbsoluteX, 4 ),new Instruction( "ROL", opCodes.ROL, addressingModes.AbsoluteX, 7 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 7 ),
                new Instruction( "RTI", opCodes.RTI, addressingModes.Implied, 6 ),new Instruction( "EOR", opCodes.EOR, addressingModes.IndirectX, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 8 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 3 ),new Instruction( "EOR", opCodes.EOR, addressingModes.ZeroPage, 3 ),new Instruction( "LSR", opCodes.LSR, addressingModes.ZeroPage, 5 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 5 ),new Instruction( "PHA", opCodes.PHA, addressingModes.Implied, 3 ),new Instruction( "EOR", opCodes.EOR, addressingModes.Immediate, 2 ),new Instruction( "LSR", opCodes.LSR, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "JMP", opCodes.JMP, addressingModes.Absolute, 3 ),new Instruction( "EOR", opCodes.EOR, addressingModes.Absolute, 4 ),new Instruction( "LSR", opCodes.LSR, addressingModes.Absolute, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 6 ),
                new Instruction( "BVC", opCodes.BVC, addressingModes.Relative, 2 ),new Instruction( "EOR", opCodes.EOR, addressingModes.IndirectY, 5 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 8 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 4 ),new Instruction( "EOR", opCodes.EOR, addressingModes.ZeroPageX, 4 ),new Instruction( "LSR", opCodes.LSR, addressingModes.ZeroPageX, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 6 ),new Instruction( "CLI", opCodes.CLI, addressingModes.Implied, 2 ),new Instruction( "EOR", opCodes.EOR, addressingModes.AbsoluteY, 4 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 7 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 4 ),new Instruction( "EOR", opCodes.EOR, addressingModes.AbsoluteX, 4 ),new Instruction( "LSR", opCodes.LSR, addressingModes.AbsoluteX, 7 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 7 ),
                new Instruction( "RTS", opCodes.RTS, addressingModes.Implied, 6 ),new Instruction( "ADC", opCodes.ADC, addressingModes.IndirectX, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 8 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 3 ),new Instruction( "ADC", opCodes.ADC, addressingModes.ZeroPage, 3 ),new Instruction( "ROR", opCodes.ROR, addressingModes.ZeroPage, 5 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 5 ),new Instruction( "PLA", opCodes.PLA, addressingModes.Implied, 4 ),new Instruction( "ADC", opCodes.ADC, addressingModes.Immediate, 2 ),new Instruction( "ROR", opCodes.ROR, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "JMP", opCodes.JMP, addressingModes.Indirect, 5 ),new Instruction( "ADC", opCodes.ADC, addressingModes.Absolute, 4 ),new Instruction( "ROR", opCodes.ROR, addressingModes.Absolute, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 6 ),
                new Instruction( "BVS", opCodes.BVS, addressingModes.Relative, 2 ),new Instruction( "ADC", opCodes.ADC, addressingModes.IndirectY, 5 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 8 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 4 ),new Instruction( "ADC", opCodes.ADC, addressingModes.ZeroPageX, 4 ),new Instruction( "ROR", opCodes.ROR, addressingModes.ZeroPageX, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 6 ),new Instruction( "SEI", opCodes.SEI, addressingModes.Implied, 2 ),new Instruction( "ADC", opCodes.ADC, addressingModes.AbsoluteY, 4 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 7 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 4 ),new Instruction( "ADC", opCodes.ADC, addressingModes.AbsoluteX, 4 ),new Instruction( "ROR", opCodes.ROR, addressingModes.AbsoluteX, 7 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 7 ),
                new Instruction( "???", opCodes.NOP, addressingModes.Implied, 2 ),new Instruction( "STA", opCodes.STA, addressingModes.IndirectX, 6 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 6 ),new Instruction( "STY", opCodes.STY, addressingModes.ZeroPage, 3 ),new Instruction( "STA", opCodes.STA, addressingModes.ZeroPage, 3 ),new Instruction( "STX", opCodes.STX, addressingModes.ZeroPage, 3 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 3 ),new Instruction( "DEY", opCodes.DEY, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 2 ),new Instruction( "TXA", opCodes.TXA, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "STY", opCodes.STY, addressingModes.Absolute, 4 ),new Instruction( "STA", opCodes.STA, addressingModes.Absolute, 4 ),new Instruction( "STX", opCodes.STX, addressingModes.Absolute, 4 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 4 ),
                new Instruction( "BCC", opCodes.BCC, addressingModes.Relative, 2 ),new Instruction( "STA", opCodes.STA, addressingModes.IndirectY, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 6 ),new Instruction( "STY", opCodes.STY, addressingModes.ZeroPageX, 4 ),new Instruction( "STA", opCodes.STA, addressingModes.ZeroPageX, 4 ),new Instruction( "STX", opCodes.STX, addressingModes.ZeroPageY, 4 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 4 ),new Instruction( "TYA", opCodes.TYA, addressingModes.Implied, 2 ),new Instruction( "STA", opCodes.STA, addressingModes.AbsoluteY, 5 ),new Instruction( "TXS", opCodes.TXS, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 5 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 5 ),new Instruction( "STA", opCodes.STA, addressingModes.AbsoluteX, 5 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 5 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 5 ),
                new Instruction( "LDY", opCodes.LDY, addressingModes.Immediate, 2 ),new Instruction( "LDA", opCodes.LDA, addressingModes.IndirectX, 6 ),new Instruction( "LDX", opCodes.LDX, addressingModes.Immediate, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 6 ),new Instruction( "LDY", opCodes.LDY, addressingModes.ZeroPage, 3 ),new Instruction( "LDA", opCodes.LDA, addressingModes.ZeroPage, 3 ),new Instruction( "LDX", opCodes.LDX, addressingModes.ZeroPage, 3 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 3 ),new Instruction( "TAY", opCodes.TAY, addressingModes.Implied, 2 ),new Instruction( "LDA", opCodes.LDA, addressingModes.Immediate, 2 ),new Instruction( "TAX", opCodes.TAX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "LDY", opCodes.LDY, addressingModes.Absolute, 4 ),new Instruction( "LDA", opCodes.LDA, addressingModes.Absolute, 4 ),new Instruction( "LDX", opCodes.LDX, addressingModes.Absolute, 4 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 4 ),
                new Instruction( "BCS", opCodes.BCS, addressingModes.Relative, 2 ),new Instruction( "LDA", opCodes.LDA, addressingModes.IndirectY, 5 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 5 ),new Instruction( "LDY", opCodes.LDY, addressingModes.ZeroPageX, 4 ),new Instruction( "LDA", opCodes.LDA, addressingModes.ZeroPageX, 4 ),new Instruction( "LDX", opCodes.LDX, addressingModes.ZeroPageY, 4 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 4 ),new Instruction( "CLV", opCodes.CLV, addressingModes.Implied, 2 ),new Instruction( "LDA", opCodes.LDA, addressingModes.AbsoluteY, 4 ),new Instruction( "TSX", opCodes.TSX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 4 ),new Instruction( "LDY", opCodes.LDY, addressingModes.AbsoluteX, 4 ),new Instruction( "LDA", opCodes.LDA, addressingModes.AbsoluteX, 4 ),new Instruction( "LDX", opCodes.LDX, addressingModes.AbsoluteY, 4 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 4 ),
                new Instruction( "CPY", opCodes.CPY, addressingModes.Immediate, 2 ),new Instruction( "CMP", opCodes.CMP, addressingModes.IndirectX, 6 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 8 ),new Instruction( "CPY", opCodes.CPY, addressingModes.ZeroPage, 3 ),new Instruction( "CMP", opCodes.CMP, addressingModes.ZeroPage, 3 ),new Instruction( "DEC", opCodes.DEC, addressingModes.ZeroPage, 5 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 5 ),new Instruction( "INY", opCodes.INY, addressingModes.Implied, 2 ),new Instruction( "CMP", opCodes.CMP, addressingModes.Immediate, 2 ),new Instruction( "DEX", opCodes.DEX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "CPY", opCodes.CPY, addressingModes.Absolute, 4 ),new Instruction( "CMP", opCodes.CMP, addressingModes.Absolute, 4 ),new Instruction( "DEC", opCodes.DEC, addressingModes.Absolute, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 6 ),
                new Instruction( "BNE", opCodes.BNE, addressingModes.Relative, 2 ),new Instruction( "CMP", opCodes.CMP, addressingModes.IndirectY, 5 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 8 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 4 ),new Instruction( "CMP", opCodes.CMP, addressingModes.ZeroPageX, 4 ),new Instruction( "DEC", opCodes.DEC, addressingModes.ZeroPageX, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 6 ),new Instruction( "CLD", opCodes.CLD, addressingModes.Implied, 2 ),new Instruction( "CMP", opCodes.CMP, addressingModes.AbsoluteY, 4 ),new Instruction( "NOP", opCodes.NOP, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 7 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 4 ),new Instruction( "CMP", opCodes.CMP, addressingModes.AbsoluteX, 4 ),new Instruction( "DEC", opCodes.DEC, addressingModes.AbsoluteX, 7 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 7 ),
                new Instruction( "CPX", opCodes.CPX, addressingModes.Immediate, 2 ),new Instruction( "SBC", opCodes.SBC, addressingModes.IndirectX, 6 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 8 ),new Instruction( "CPX", opCodes.CPX, addressingModes.ZeroPage, 3 ),new Instruction( "SBC", opCodes.SBC, addressingModes.ZeroPage, 3 ),new Instruction( "INC", opCodes.INC, addressingModes.ZeroPage, 5 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 5 ),new Instruction( "INX", opCodes.INX, addressingModes.Implied, 2 ),new Instruction( "SBC", opCodes.SBC, addressingModes.Immediate, 2 ),new Instruction( "NOP", opCodes.NOP, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.SBC, addressingModes.Implied, 2 ),new Instruction( "CPX", opCodes.CPX, addressingModes.Absolute, 4 ),new Instruction( "SBC", opCodes.SBC, addressingModes.Absolute, 4 ),new Instruction( "INC", opCodes.INC, addressingModes.Absolute, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 6 ),
                new Instruction( "BEQ", opCodes.BEQ, addressingModes.Relative, 2 ),new Instruction( "SBC", opCodes.SBC, addressingModes.IndirectY, 5 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 8 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 4 ),new Instruction( "SBC", opCodes.SBC, addressingModes.ZeroPageX, 4 ),new Instruction( "INC", opCodes.INC, addressingModes.ZeroPageX, 6 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 6 ),new Instruction( "SED", opCodes.SED, addressingModes.Implied, 2 ),new Instruction( "SBC", opCodes.SBC, addressingModes.AbsoluteY, 4 ),new Instruction( "NOP", opCodes.NOP, addressingModes.Implied, 2 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 7 ),new Instruction( "???", opCodes.NOP, addressingModes.Implied, 4 ),new Instruction( "SBC", opCodes.SBC, addressingModes.AbsoluteX, 4 ),new Instruction( "INC", opCodes.INC, addressingModes.AbsoluteX, 7 ),new Instruction( "???", opCodes.XXX, addressingModes.Implied, 7 ),
            };

            return instructions;
        }

        private Dictionary<int, string> Disassemble(int start, int stop)
        {
            int address = start;
            int value = 0x00;
            int lowByte = 0x00;
            int highByte = 0x00;
            int lineAddress = 0x0000;

            var disassembly = new Dictionary<int, string>();

            while (address <= stop)
            {
                lineAddress = address;
                string sInst = $"${address:X4}: ";
                int opcode = ReadMemory(address);
                Instruction instruction = lookupInstructionsTable[opcode];

                address++;
                sInst += $"{instruction.Name} ";

                if (instruction.AddressingMode == Implied)
                {
                    sInst += " {IMP}";
                }
                else if (instruction.AddressingMode == Immediate)
                {
                    value = ReadMemory(address);
                    address++;
                    sInst += $"#${value:X2} {{IMM}}";
                }
                else if (instruction.AddressingMode == ZeroPage)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = 0x00;
                    sInst += $"${lowByte:X2} {{ZP0}}";
                }
                else if (instruction.AddressingMode == ZeroPageX)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = 0x00;
                    sInst += $"${lowByte:X2},X {{ZPX}}";
                }
                else if (instruction.AddressingMode == ZeroPageY)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = 0x00;
                    sInst += $"${lowByte:X2},Y {{ZPY}}";
                }
                else if (instruction.AddressingMode == IndirectX)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = 0x00;
                    sInst += $"(${lowByte:X2},X) {{IZX}}";
                }
                else if (instruction.AddressingMode == IndirectY)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = 0x00;
                    sInst += $"(${lowByte:X2},) Y {{IZY}}";
                }

                else if (instruction.AddressingMode == Absolute)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = ReadMemory(address);
                    address++;
                    sInst += $"${highByte << 8 | lowByte:X4} {{ABS}}";
                }
                else if (instruction.AddressingMode == AbsoluteX)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = ReadMemory(address);
                    address++;
                    sInst += $"${highByte << 8 | lowByte:X4}, X {{ABX}}";
                }
                else if (instruction.AddressingMode == AbsoluteY)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = ReadMemory(address);
                    address++;
                    sInst += $"${highByte << 8 | lowByte:X4}, Y {{ABY}}";
                }
                else if (instruction.AddressingMode == Indirect)
                {
                    lowByte = ReadMemory(address);
                    address++;
                    highByte = ReadMemory(address);
                    address++;
                    sInst += $"(${highByte << 8 | lowByte:X4}) {{IND}}";
                }
                else if (instruction.AddressingMode == Relative)
                {
                    value = ReadMemory(address);
                    address++;
                    sInst += $"${value:X2} [$ {address + value:X4}] {{REL}}";
                }
                
                disassembly[lineAddress] = sInst;
            }       

            return disassembly;
        }   
    }
}