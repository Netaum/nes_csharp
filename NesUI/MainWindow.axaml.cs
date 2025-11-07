using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Emulator.Components;
using Emulator.Components.Enums;
using Emulator.Components.Interfaces;
using Emulator.Debug;

namespace NesUI;

public partial class MainWindow : Window
{
    private Bus? _bus = null!;
    private DispatcherTimer _timer = new DispatcherTimer();
    private WriteableBitmap _emulatorBitmap;

    [MemberNotNull(nameof(_bus))]
    public void SetBus(Bus bus)
    {
        _bus = bus;
        _bus.Reset();
    }

    public void ClockAction(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button)
            return;

        var cpu = _bus!.Cpu;

        do
        {
            _bus.Clock();
        } while (!cpu.Complete);

        do
        {
            _bus.Clock();
        } while (cpu.Complete);
    }

    public void FrameAdvanceAction(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button)
            return;

        var cpu = _bus!.Cpu;

        do
        {
            _bus.Clock();
        } while (!_bus.Ppu.FrameComplete);

        do
        {
            _bus.Clock();
        } while (!cpu.Complete);

        _bus.Ppu.FrameComplete = false;
        EmulatorWindow.Source = UpdateBitmap(_emulatorBitmap, _bus);
        EmulatorWindow.InvalidateVisual();
    }

    public void ResetAction(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button)
            return;

        _bus!.Reset();
    }

    public MainWindow()
    {
        InitializeComponent();
        InitializeEmulatorWindow();
    }

    private static void DrawMemoryPage(StackPanel panel, ICpu cpu, int address)
    {
        StringBuilder sb = new StringBuilder();
        panel.Children.Clear();
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
            TextBlock txtBlock = new TextBlock
            {
                Text = line,
                Foreground = Brushes.White,
                FontFamily = "Courier New",
                FontSize = 14,
                Height = 16
            };
            panel.Children.Add(txtBlock);
        }
    }

    private static IImmutableBrush GetFlagBrush(ICpu cpu, Flags6502 flag)
    {
        return cpu.GetStatusFlag(flag) ? Brushes.Red : Brushes.Yellow;
    }

    private static void DrawCpuRegisters(TextBlock textBlock, ICpu cpu)
    {
        textBlock.Inlines =
        [
            new Run("STATUS: "),
            new Run(" N") { Foreground = GetFlagBrush(cpu, Flags6502.N) },
            new Run(" V") { Foreground = GetFlagBrush(cpu, Flags6502.V) },
            new Run(" -"),
            new Run(" B") { Foreground = GetFlagBrush(cpu, Flags6502.B) },
            new Run(" D") { Foreground = GetFlagBrush(cpu, Flags6502.D) },
            new Run(" I") { Foreground = GetFlagBrush(cpu, Flags6502.I) },
            new Run(" Z") { Foreground = GetFlagBrush(cpu, Flags6502.Z) },
            new Run(" C") { Foreground = GetFlagBrush(cpu, Flags6502.C) },
            new LineBreak(),
            new Run($"PC: ${cpu.ProgramCounter:X4}"),
            new LineBreak(),
            new Run($"A:  ${cpu.AccumulatorRegister:X2}  [{cpu.AccumulatorRegister}]"),
            new LineBreak(),
            new Run($"X:  ${cpu.XRegister:X2}  [{cpu.XRegister}]"),
            new LineBreak(),
            new Run($"Y:  ${cpu.YRegister:X2}  [{cpu.YRegister}]"),
            new LineBreak(),
            new Run($"SP: ${cpu.StackPointer:X4}"),
        ];
    }
    private static void DrawCode(TextBlock textblock, ICpu cpu, int memoryMark, int numberOfLines)
    {

        var code = Disassemble.FromMemory(cpu, 0x0000, 0xFFFF);
        var currentMemory = memoryMark - (numberOfLines / 2);

        var splice = code.Where(w => w.Key >= currentMemory)
                         .Take(numberOfLines)
                         .ToDictionary(k => k.Key, v => v.Value);

        textblock.Inlines = [new LineBreak()];
        foreach (var line in splice)
        {
            var brush = line.Key == memoryMark ? Brushes.Yellow : Brushes.White;
            textblock.Inlines.Add(new Run(line.Value.ToString()) { Foreground = brush });
            textblock.Inlines.Add(new LineBreak());
        }
    }

    [MemberNotNull(nameof(_emulatorBitmap))]
    private void InitializeEmulatorWindow()
    {
        _emulatorBitmap = new WriteableBitmap(PixelSize.FromSize(new Size(256, 240), 1), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        //EmulatorWindow.Source = _emulatorBitmap;
    }

    public MainWindow(Bus bus)
    : this()
    {
        InitializeComponent();
        SetBus(bus);
        //RegistersTextBlock.Text = "";
        //CodeTextBlock.Text = "";

        _timer.Interval = TimeSpan.FromMilliseconds(33);

        _timer.Tick += (sender, e) =>
        {
            /*
            DrawEmulation(
                _bus,
                MemoryPage0,
                MemoryPage1,
                RegistersTextBlock,
                CodeTextBlock,
                EmulatorWindow,
                _emulatorBitmap);
                */
            //EmulatorWindow.Source = UpdateBitmap(_emulatorBitmap, bus);
        };

        _timer.Start();
    }

    private unsafe static WriteableBitmap UpdateBitmap(WriteableBitmap bitmap, Bus bus)
    {
        using ILockedFramebuffer lockedFramebuffer = bitmap.Lock();

        void* backBuffer = (void*)lockedFramebuffer.Address;
        Span<byte> buffer = new Span<byte>(backBuffer, 256 * 240 * 4);
        var img = bus.Ppu.GetScreen().ToBgraFormat();

        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = img[i];
        }
        return bitmap;
    }

    private static void DrawEmulation(
            Bus bus,
            StackPanel memoryPage0,
            StackPanel memoryPage1,
            TextBlock registersTextBlock,
            TextBlock codeTextBlock,
            Image image,
            WriteableBitmap bitmap)
    {
        DrawMemoryPage(memoryPage0, bus.Cpu, 0x0000);
        DrawMemoryPage(memoryPage1, bus.Cpu, 0x8000);

        DrawCpuRegisters(registersTextBlock, bus.Cpu);
        DrawCode(codeTextBlock, bus.Cpu, bus.Cpu.ProgramCounter, 30);
        image.Source = UpdateBitmap(bitmap, bus);
    }
}