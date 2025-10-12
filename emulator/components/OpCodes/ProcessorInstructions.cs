using emulator.components.Interfaces;
using emulator.components.OpCodes.Implementations;

namespace emulator.components.OpCodes
{
    public static class ProcessorInstructions
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
}