namespace nes_csharp.components
{
    public interface IOpCodes
    {
        int ADC(); int AND(); int ASL(); int BCC();
        int BCS(); int BEQ(); int BIT(); int BMI();
        int BNE(); int BPL(); int BRK(); int BVC();
        int BVS(); int CLC(); int CLD(); int CLI();
        int CLV(); int CMP(); int CPX(); int CPY();
        int DEC(); int DEX(); int DEY(); int EOR();
        int INC(); int INX(); int INY(); int JMP();
        int JSR(); int LDA(); int LDX(); int LDY();
        int LSR(); int NOP(); int ORA(); int PHA();
        int PHP(); int PLA(); int PLP(); int ROL();
        int ROR(); int RTI(); int RTS(); int SBC();
        int SEC(); int SED(); int SEI(); int STA();
        int STX(); int STY(); int TAX(); int TAY();
        int TSX(); int TXA(); int TXS(); int TYA();
        int XXX();
    }
}