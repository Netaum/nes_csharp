using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;
using Emulator.Components;
using Emulator.Components.Enums;
using Emulator.Components.Interfaces;
using Emulator.DebugTools;
using NesUI.Helpers;

namespace NesUI
{
    public partial class MainWindow : Window
    {
        private Bus? _bus = null!;
        private DispatcherTimer _timer = new DispatcherTimer();
        private List<ScreenBinding> _emulatorScreenBindings;
        private bool _showMemory = false;
        private int _selectedPalette = 0;
        private bool _runEmulation = false;
        private List<PaletteViewer> _paletteViewers;


        [MemberNotNull(nameof(_emulatorScreenBindings))]
        [MemberNotNull(nameof(_paletteViewers))]
        private void InitializeEmulatorWindow()
        {
            var emulatorScreen = new ScreenBinding(ScreenSelection.EmulatorScreen, EmulatorWindow, 256, 240);
            var patternTable0 = new ScreenBinding(ScreenSelection.PatternTable1, PatternTable0, 128, 128);
            var patternTable1 = new ScreenBinding(ScreenSelection.PatternTable2, PatternTable1, 128, 128);
            _emulatorScreenBindings = new List<ScreenBinding>
            {
                emulatorScreen,
                patternTable0,
                patternTable1
            };

            DebugGrid.ColumnDefinitions = new ColumnDefinitions();
            for (int i = 0; i < 32; i++)
            {
                DebugGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            }

            var squares = new List<Avalonia.Controls.Shapes.Rectangle>();
            _paletteViewers = new List<PaletteViewer>();

            for (int i = 0; i < 32; i++)
            {
                if (i % 4 == 0)
                {
                    if (squares.Count > 0)
                    {
                        _paletteViewers.Add(new PaletteViewer(squares));
                    }
                    squares = new List<Avalonia.Controls.Shapes.Rectangle>();
                }
                var rect = new Avalonia.Controls.Shapes.Rectangle { Fill = Brushes.Yellow, StrokeThickness = 0 };
                Grid.SetColumn(rect, i);
                DebugGrid.Children.Add(rect);
                squares.Add(rect);
            }

            _paletteViewers.Add(new PaletteViewer(squares));

            for (int i = 0; i < 32; i++)
            {
                if (i % 4 == 0)
                {
                    var border = new Avalonia.Controls.Shapes.Rectangle { Fill = Brushes.Transparent, StrokeThickness = 1, Stroke = Brushes.Black };
                    Grid.SetColumnSpan(border, 4);
                    Grid.SetColumn(border, i);
                    DebugGrid.Children.Add(border);
                    int viewIndex = i / 4;
                    _paletteViewers[viewIndex].SetBorderRectangle(border);
                }
            }
        }

        [MemberNotNull(nameof(_bus))]
        public void SetBus(Bus bus)
        {
            _bus = bus;
            _bus.Reset();
        }

        private static void DrawMemoryPage(TextBlock textBlock, ICpu cpu, int address)
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
                textBlock.Inlines!.Add(new Run(line));
                textBlock.Inlines!.Add(new LineBreak());
            }
        }

        private static IImmutableBrush GetFlagBrush(ICpu cpu, Flags6502 flag)
        {
            return cpu.GetStatusFlag(flag) ? Brushes.Red : Brushes.Yellow;
        }

        private static void DrawCpuRegisters(TextBlock textBlock, ICpu cpu)
        {
            textBlock.Inlines!.AddRange(
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
            ]);
        }

        private static void DrawCode(TextBlock textblock, ICpu cpu, int memoryMark, int numberOfLines)
        {

            var code = Disassemble.FromMemory(cpu, 0x0000, 0xFFFF);
            var currentMemory = memoryMark - (numberOfLines / 2);

            var splice = code.Where(w => w.Key >= currentMemory)
                             .Take(numberOfLines)
                             .ToDictionary(k => k.Key, v => v.Value);

            foreach (var line in splice)
            {
                var brush = line.Key == memoryMark ? Brushes.Yellow : Brushes.White;
                textblock.Inlines!.Add(new Run(line.Value.ToString()) { Foreground = brush });
                textblock.Inlines!.Add(new LineBreak());
            }
        }

        private static void UpdateFrame(Bus bus)
        {
            do
            {
                bus.Clock();
            } while (!bus.Ppu.FrameComplete);

            do
            {
                bus.Clock();
            } while (!bus.Cpu.Complete);

            bus.Ppu.FrameComplete = false;
        }

        private static void UpdateScreens(Bus bus, List<ScreenBinding> emulatorScreens, int palette)
        {
            foreach (var _emulatorScreenBinding in emulatorScreens)
                _emulatorScreenBinding.OnUpdate(bus.Ppu, palette);
        }

        private static void UpdateCode(Bus bus, TextBlock contentBlock)
        {
            contentBlock.Inlines = new InlineCollection();
            DrawCpuRegisters(contentBlock, bus.Cpu);
            contentBlock.Inlines!.Add(new LineBreak());
            DrawCode(contentBlock, bus.Cpu, bus.Cpu.ProgramCounter, 30);
        }

        private static void UpdateMemory(Bus bus, TextBlock contentBlock)
        {
            contentBlock.Inlines = new InlineCollection();
            DrawMemoryPage(contentBlock, bus.Cpu, 0x0000);
            contentBlock.Inlines!.Add(new LineBreak());
            DrawMemoryPage(contentBlock, bus.Cpu, 0x8000);
        }


        private static void UpdateContent(Bus bus, TextBlock contentBlock, bool drawMemory)
        {
            if (drawMemory)
            {
                UpdateMemory(bus, contentBlock);
            }
            else
            {
                UpdateCode(bus, contentBlock);
            }
        }

        private static void UpdatePaletteVisuals(Bus bus, List<PaletteViewer> paletteViewers, int selectedPalette)
        {
            for (int i = 0; i < paletteViewers.Count; i++)
            {
                paletteViewers[i].SetActive(i == selectedPalette);
                paletteViewers[i].OnUpdate(bus.Ppu, i);
            }
        }

        private static void UpdateView(
                    Bus bus,
                    TextBlock contentBlock,
                    List<ScreenBinding> emulatorScreens,
                    bool drawMemory,
                    int selectedPalette,
                    List<PaletteViewer> paletteViewers)
        {
            UpdateFrame(bus);
            UpdateScreens(bus, emulatorScreens, selectedPalette);
            UpdateContent(bus, contentBlock, drawMemory);
            UpdatePaletteVisuals(bus, paletteViewers, selectedPalette);
        }

        private static void OnTick(
            Bus bus,
            TextBlock contentBlock,
            List<ScreenBinding> emulatorScreens,
            bool runEmulation,
            bool drawMemory,
            int selectedPalette,
            List<PaletteViewer> paletteViewers)
        {
            if (!runEmulation)
                return;
            UpdateView(bus, contentBlock, emulatorScreens, drawMemory, selectedPalette, paletteViewers);
        }
    }
}