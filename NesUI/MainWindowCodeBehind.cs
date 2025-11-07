using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CustomTypes;
using Emulator.Components;
using Emulator.Components.Enums;
using Emulator.Components.Interfaces;
using Emulator.Debug;

namespace NesUI
{
    public enum ScreenSelection
    {
        EmulatorScreen,
        PatternTable1,
        PatternTable2
    }

    public class ScreenBinding
    {
        private WriteableBitmap _writeableBitmap;
        private Image _imageControl;
        private ScreenSelection _screenSelection;

        public ScreenBinding(ScreenSelection screenSelection, Image imageControl, WriteableBitmap writeableBitmap)
        {
            _screenSelection = screenSelection;
            _imageControl = imageControl;
            _writeableBitmap = writeableBitmap;
        }

        public WriteableBitmap WriteableBitmap => _writeableBitmap;
        public Image ImageControl => _imageControl;
        public ScreenSelection ScreenSelection => _screenSelection;

        public WriteableBitmap OnUpdate(IPpu ppu)
        {
            var baseBitmap = _screenSelection switch
            {
                ScreenSelection.EmulatorScreen => ppu.GetScreen(),
                ScreenSelection.PatternTable1 => ppu.GetPatternTable(0, 1),
                ScreenSelection.PatternTable2 => ppu.GetPatternTable(1, 1),
                _ => throw new ArgumentOutOfRangeException()
            };
            return UpdateBitmap(_writeableBitmap, baseBitmap);
        }
        
        private unsafe static WriteableBitmap UpdateBitmap(WriteableBitmap bitmap, BaseBitmap baseBitmap)
        {
            using ILockedFramebuffer lockedFramebuffer = bitmap.Lock();

            void* backBuffer = (void*)lockedFramebuffer.Address;
            Span<byte> buffer = new Span<byte>(backBuffer, baseBitmap.Width * baseBitmap.Height * 4);
            var img = baseBitmap.ToBgraFormat();

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = img[i];
            }
            return bitmap;
        }
    }

    public partial class MainWindow : Window
    {
        private Bus? _bus = null!;
        private DispatcherTimer _timer = new DispatcherTimer();
        private List<ScreenBinding> _emulatorScreens;
        private bool _showMemory = false;

        [MemberNotNull(nameof(_emulatorScreens))]
        private void InitializeEmulatorWindow()
        {
            var writeableEmuScreen = new WriteableBitmap(PixelSize.FromSize(new Size(256, 240), 2), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
            var patternTable1 = new WriteableBitmap(PixelSize.FromSize(new Size(256, 240), 1), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
            var patternTable2 = new WriteableBitmap(PixelSize.FromSize(new Size(256, 240), 1), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);

            EmulatorWindow.Source = writeableEmuScreen;
            PatternTable1.Source = patternTable1;
            PatternTable2.Source = patternTable2;

            _emulatorScreens = new List<ScreenBinding>
            {
                new ScreenBinding(ScreenSelection.EmulatorScreen, EmulatorWindow, writeableEmuScreen),
                new ScreenBinding(ScreenSelection.PatternTable1, PatternTable1, patternTable1),
                new ScreenBinding(ScreenSelection.PatternTable2, PatternTable2, patternTable2),
            };
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

        private static void DrawEmulation(
            Bus bus,
            TextBlock contentBlock,
            List<ScreenBinding> emulatorScreens,
            bool drawMemory)
        {
            contentBlock.Inlines = new InlineCollection();

            if (drawMemory)
            {
                DrawMemoryPage(contentBlock, bus.Cpu, 0x0000);
                contentBlock.Inlines!.Add(new LineBreak());
                DrawMemoryPage(contentBlock, bus.Cpu, 0x8000);
            }
            else
            {
                DrawCpuRegisters(contentBlock, bus.Cpu);
                contentBlock.Inlines!.Add(new LineBreak());
                DrawCode(contentBlock, bus.Cpu, bus.Cpu.ProgramCounter, 30);
            }

            var emu = emulatorScreens.First(w => w.ScreenSelection == ScreenSelection.EmulatorScreen);
            var image = emu.ImageControl;

            image.Source = emu.OnUpdate(bus.Ppu);

            

        }
    }
}