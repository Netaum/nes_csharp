using System.Text;
using emulator.components;
using emulator.components.Enums;
using emulator.components.Interfaces;


namespace pixelgraph;

public partial class FrMain : Form
{
    private static readonly Font _font = new Font("Lucida Console", 14, FontStyle.Regular);
    private static readonly Font _font1 = new Font("Lucida Console", 11, FontStyle.Regular);
    private readonly IBus _bus;

    public FrMain(IBus bus)
    {
        InitializeComponent();

        _bus = bus;
        _bus.Reset();

        string rom = "A2 0A 8E 00 00 A2 03 8E 01 00 AC 00 00 A9 00 18 6D 01 00 88 D0 FA 8D 02 00 EA EA EA";
        var bytes = rom.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
        Draw();
    }

    private void Draw()
    {
        DrawMemoryPage(rtxtMemPg1, 0x0000, _bus.Cpu);
        DrawMemoryPage(rtxtMemPg2, 0x8000, _bus.Cpu);
        DrawCpu(rtxtCpu, _bus.Cpu);
        var code = _bus.Cpu.Disassemble(0x0000, 0xFFFF);
        DrawCode(rtxtCode, code, _bus.Cpu.ProgramCounter, 34);
    }

    private void BtnClock_Click(object sender, EventArgs e)
    {
        do
        {
            _bus.Clock();
        } while (!_bus.Cpu.Complete);

        do
        {
            _bus.Clock();
        } while (_bus.Cpu.Complete);

        Draw();
    }

    private static void DrawCode(RichTextBox rtxt, Dictionary<int, string> code, int memoryMark, int numberOfLines)
    {
        SetOutput(rtxt, _font1);
        int outputedCode = 0;
        int currentMemory = memoryMark - (numberOfLines / 2);
        while (outputedCode < numberOfLines && currentMemory < 0xFFFF)
        {
            if (code.ContainsKey(currentMemory))
            {
                if(currentMemory == memoryMark)
                {
                    rtxt.SelectionColor = Color.Yellow;
                }
                else
                {
                    rtxt.SelectionColor = Color.White;
                }
                rtxt.AppendText(code[currentMemory] + "\n");
                outputedCode++;
            }
            currentMemory++;
        }
    }

    private static void SetOutput(RichTextBox rtxt, Font font)
    {
        rtxt.Clear();
        rtxt.Font = font;
        rtxt.BackColor = Color.DarkBlue;
        rtxt.ForeColor = Color.White;
        rtxt.ReadOnly = true;
        rtxt.TabStop = false;
        rtxt.BorderStyle = BorderStyle.None;
        rtxt.ScrollBars = RichTextBoxScrollBars.None;
    }

    private static void DrawRegister(RichTextBox rtxt, ICpu cpu, Flags6502 flag)
    {
        var color = cpu.GetStatusFlag(flag) ? Color.Red : Color.Yellow;
        rtxt.SelectionColor = color;
        rtxt.AppendText($" {flag}");
    }

    private static void DrawCpu(RichTextBox rtxt, ICpu cpu)
    {
        SetOutput(rtxt, _font);
        rtxt.AppendText("STATUS:");
        DrawRegister(rtxt, cpu, Flags6502.N);
        DrawRegister(rtxt, cpu, Flags6502.V);
        rtxt.AppendText(" -");
        DrawRegister(rtxt, cpu, Flags6502.B);
        DrawRegister(rtxt, cpu, Flags6502.D);
        DrawRegister(rtxt, cpu, Flags6502.I);
        DrawRegister(rtxt, cpu, Flags6502.Z);
        DrawRegister(rtxt, cpu, Flags6502.C);
        rtxt.SelectionColor = Color.White;
        rtxt.AppendText($"\nPC: ${cpu.ProgramCounter:X4}\n");
        rtxt.AppendText($"A: ${cpu.AccumulatorRegister:X2}  [{cpu.AccumulatorRegister}]\n");
        rtxt.AppendText($"X: ${cpu.XRegister:X2}  [{cpu.XRegister}]\n");
        rtxt.AppendText($"Y: ${cpu.YRegister:X2}  [{cpu.YRegister}]\n");
        rtxt.AppendText($"Stack P: ${cpu.StackPointer:X4}\n");
    } 

    private static void DrawMemoryPage(RichTextBox rtxt, int address, ICpu cpu)
    {
        SetOutput(rtxt, _font);

        StringBuilder sb = new StringBuilder();
        for(int row = 0; row < 16; row++)
        {
            sb.AppendFormat("${0:X4}:", address);
            for (int col = 0; col < 16; col++)
            {
                sb.AppendFormat(" {0:X2}", cpu.ReadMemory(address));
                address++;
            }
            sb.AppendLine();
        }

       rtxt.Text = sb.ToString();
    }
}
