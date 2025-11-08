using System;
using System.Collections.Generic;
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
    private List<ScreenBinding> _emulatorScreenBindings;

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

    [MemberNotNull(nameof(_emulatorScreenBindings))]
    private void InitializeEmulatorWindow()
    {
        var emulatorScreen = new ScreenBinding(ScreenSelection.EmulatorScreen, EmulatorWindow, 256, 240);
        var patternTable0 = new ScreenBinding(ScreenSelection.PatternTable1, PatternTable0, 128, 128);
        var patternTable1 = new ScreenBinding(ScreenSelection.PatternTable2, PatternTable1, 128, 128);
        _emulatorScreenBindings = new  List<ScreenBinding>
        {
            emulatorScreen,
            patternTable0,
            patternTable1
        };
    }

    public MainWindow(Bus bus)
    : this()
    {

        SetBus(bus);
        //RegistersTextBlock.Text = "";
        //CodeTextBlock.Text = "";

        _timer.Interval = TimeSpan.FromMilliseconds(33);

        _timer.Tick += (sender, e) =>
        {
            do
            {
                _bus!.Clock();
            } while (!_bus.Ppu.FrameComplete);

            _bus.Ppu.FrameComplete = false;
            
            foreach (var _emulatorScreenBinding in _emulatorScreenBindings)
                _emulatorScreenBinding.OnUpdate(_bus.Ppu);
        };

        _timer.Start();
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
        var customBitmap = bus.Ppu.GetScreen();
        image.Source = UpdateBitmap(bitmap, customBitmap);
    }
}