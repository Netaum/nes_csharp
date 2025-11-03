using System.Data.Common;
using System.Text;
using emulator.components.Enums;
using emulator.components.Interfaces;


namespace pixelgraph;

public partial class FrMain : Form
{
    private static readonly Font _font = new Font("Lucida Console", 14, FontStyle.Regular);
    private static readonly Font _font1 = new Font("Lucida Console", 11, FontStyle.Regular);
    private static readonly StringFormat _drawFormat = new StringFormat();
    private readonly IBus _bus;

    public FrMain(IBus bus)
    {
        InitializeComponent();

        _bus = bus;
        _bus.Reset();
        Render();
    }

    private static void DrawMemoryPage(float x, float y, SizeF fontSize, int address, ICpu cpu, Graphics graphics)
    {
        StringBuilder sb = new StringBuilder();
        for (int row = 0; row < 16; row++)
        {
            sb.Clear();
            sb.AppendFormat("${0:X4}:", address);
            for (int col = 0; col < 16; col++)
            {
                sb.AppendFormat(" {0:X2}", cpu.ReadMemory(address));
                address++;
            }
            string line = sb.ToString();
            graphics.DrawString(line, _font1, Brushes.White, x, y, _drawFormat);
            y += fontSize.Height + 2;
        }
    }

    private static void DrawRegister(float x, float y, ICpu cpu, Flags6502 flag, Graphics graphics)
    {
        var color = cpu.GetStatusFlag(flag) ? Brushes.Red : Brushes.Yellow;
        graphics.DrawString($" {flag}", _font1, color, x, y, _drawFormat);
    }

    private static void DrawCpu(float x, float y, SizeF fontSize, ICpu cpu, Graphics graphics)
    {
        graphics.DrawString("STATUS:", _font1, Brushes.White, x, y, _drawFormat);
        var localX = x + fontSize.Width * 8;
        var localY = y;
        DrawRegister(localX, localY, cpu, Flags6502.N, graphics);
        localX += fontSize.Width * 2;
        DrawRegister(localX, localY, cpu, Flags6502.V, graphics);
        localX += fontSize.Width * 2;

        graphics.DrawString($" -", _font1, Brushes.White, localX, localY, _drawFormat);
        localX += fontSize.Width * 2;
        DrawRegister(localX, localY, cpu, Flags6502.B, graphics);
        localX += fontSize.Width * 2;
        DrawRegister(localX, localY, cpu, Flags6502.D, graphics);
        localX += fontSize.Width * 2;
        DrawRegister(localX, localY, cpu, Flags6502.I, graphics);
        localX += fontSize.Width * 2;
        DrawRegister(localX, localY, cpu, Flags6502.Z, graphics);
        localX += fontSize.Width * 2;
        DrawRegister(localX, localY, cpu, Flags6502.C, graphics);

        localX = x;
        localY += fontSize.Height + 2;

        graphics.DrawString($"PC: ${cpu.ProgramCounter:X4}", _font1, Brushes.White, localX, localY, _drawFormat);
        localY += fontSize.Height + 2;
        graphics.DrawString($"A: ${cpu.AccumulatorRegister:X2}  [{cpu.AccumulatorRegister}]", _font1, Brushes.White, localX, localY, _drawFormat);
        localY += fontSize.Height + 2;
        graphics.DrawString($"X: ${cpu.XRegister:X2}  [{cpu.XRegister}]", _font1, Brushes.White, localX, localY, _drawFormat);
        localY += fontSize.Height + 2;
        graphics.DrawString($"Y: ${cpu.YRegister:X2}  [{cpu.YRegister}]", _font1, Brushes.White, localX, localY, _drawFormat);
        localY += fontSize.Height + 2;
        graphics.DrawString($"Stack P: ${cpu.StackPointer:X4}", _font1, Brushes.White, localX, localY, _drawFormat);

    }

    private static void DrawCode(float x, float y, SizeF size, Dictionary<int, string> code, int memoryMark, int numberOfLines, Graphics graphics)
    {
        int outputedCode = 0;
        int currentMemory = memoryMark - (numberOfLines / 2);
        float localX = x;
        float localY = y;

        while (outputedCode < numberOfLines && currentMemory < 0xFFFF)
        {
            if (code.ContainsKey(currentMemory))
            {
                var brush = currentMemory == memoryMark ? Brushes.Yellow : Brushes.White;
                graphics.DrawString(code[currentMemory], _font1, brush, localX, localY, _drawFormat);
                localY += size.Height + 2;
                outputedCode++;
            }
            currentMemory++;
        }
    }

    private void Render()
    {
        using var bmp = new Bitmap(pictureBox.Width, pictureBox.Height);
        using var gfx = Graphics.FromImage(bmp);

        var size = gfx.MeasureString("A", _font1, new PointF(0, 0), StringFormat.GenericTypographic);


        gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        gfx.Clear(Color.Blue);

        float x = 10f;
        float y = 10f;

        //DrawMemoryPage(x, y, size, 0x0000, _bus.Cpu, gfx);
        y = y + ((size.Height + 2) * 18);
        //DrawMemoryPage(x, y, size, 0x8000, _bus.Cpu, gfx);

        y = 10f;
        x = size.Width * 60;
        DrawCpu(x, y, size, _bus.Cpu, gfx);

        y += (size.Height + 2) * 7;

        var code = _bus.Cpu.Disassemble(0x0000, 0xFFFF);
        DrawCode(x, y, size, code, _bus.Cpu.ProgramCounter, 27, gfx);

        var img = _bus.Ppu.GetScreen();

        gfx.DrawImage(img, 10f, 10f);

        pictureBox.Image?.Dispose();
        pictureBox.Image = (Bitmap)bmp.Clone();
    }

    private async void BtnClock_Click(object sender, EventArgs e)
    {
        await Clock();
    }

    private async Task Clock()
    {
        await Task.Run(() =>
        {
            while (true)
            {
                do
                {
                    _bus.Clock();
                } while (!_bus.Ppu.FrameComplete);
                _bus.Ppu.FrameComplete = false;

                Render();
                //Thread.Sleep(5);
            }
        });
    }

}
